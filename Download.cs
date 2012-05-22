using Smoothget;
using Smoothget.Mkv;
using Smoothget.Mp4;
using Smoothget.Smooth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
namespace Smoothget.Download {
  // Download progress for a single track.
  // This is a pure data class, please don't add logic.
  // Cannot use a `struct' here, because its fields are read-only once the constructor returns.
  internal class Track {
    public TrackInfo TrackInfo;
    public ulong NextStartTime;  // Start time of the next chunk to download.
    public int DownloadedChunkCount;
    public Track(TrackInfo trackInfo) {
      this.TrackInfo = trackInfo;
      this.DownloadedChunkCount = 0;
    }
  }

  // This is a pure data class, please don't add logic.
  // Don't convert MediaSample to a struct, it makes the executable 512 bytes larger.
  internal class MediaSample {
    public long Offset;  // In bytes.
    public ulong StartTime;
    public int Length;  // In bytes.
    public bool IsKeyFrame;
    public MediaSample(long offset, int length, ulong startTime, bool isKeyFrame) {
      this.Offset = offset;
      this.Length = length;
      this.StartTime = startTime;
      this.IsKeyFrame = isKeyFrame;
    }
  }

  public interface IStoppable {
    void Stop();
  }

  public delegate void SetupStop(bool isLive, IStoppable stoppable);
  public delegate void DisplayDuration(ulong reachedTicks, ulong totalTicks);

  // To be passed to WriteMkv when isCombo == true.
  internal class DownloadingMediaDataSource : IMediaDataSource, IStoppable {
    // Usually Tracks has 2 elements: one for the video track and one for the audio track.
    private IList<Track> Tracks;
    private string ManifestParentPath;
    private ulong MinStartTime;
    private ulong TotalDuration;
    private ulong TimeScale;
    private DisplayDuration DisplayDuration;

    // Usually TrackSamples has 2 elements: one for the video track and one for the audio track.
    private IList<MediaSample>[] TrackSamples;
    private int[] TrackSampleStartIndexes;
    // this.TrackFirstBytes[i] corresponds to this.TrackSamples[i][this.TrackSampleStartIndexes[i]].GetBytes(),
    // or null if not converted yet.
    private MediaDataBlock[] TrackFirstBlocks;
    // this.TrackFirstFileDatas[i] contains the whole file for this.TrackSamples[i][this.TrackSampleStartIndexes[i]] or
    // it's null.
    private byte[][] TrackFirstFileDatas;
    private IChunkStartTimeReceiver ChunkStartTimeReceiver;
    private bool IsLive;
    public volatile bool IsStopped;
    private ulong StopAfter;
    private ulong TotalTicks;  // For ETA calculation.

    // The object created may modify trackSamples in a destructive way, to save memory.
    // Expects tracks[...].NextStartTime and tracks[...].DownloadedChunkCount to be initialized.
    public DownloadingMediaDataSource(IList<Track> tracks, string manifestParentPath,
                                      ulong timeScale, bool isLive, ulong stopAfter, ulong totalTicks,
                                      DisplayDuration displayDuration) {
      int trackCount = tracks.Count;
      this.Tracks = tracks;
      this.ManifestParentPath = manifestParentPath;
      this.TimeScale = timeScale;
      this.DisplayDuration = displayDuration;
      this.IsLive = isLive;
      this.StopAfter = stopAfter;
      this.TotalDuration = 0;
      this.MinStartTime = ulong.MaxValue;
      this.IsStopped = false;
      for (int i = 0; i < trackCount; ++i) {
        ulong chunkStartTime = tracks[i].NextStartTime;
        if (this.MinStartTime > chunkStartTime) {
          this.MinStartTime = chunkStartTime;
        }
      }
      this.TotalTicks = totalTicks;
      this.TrackSamples = new IList<MediaSample>[trackCount];  // Items initialized to null.
      for (int i = 0; i < trackCount; ++i) {
        this.TrackSamples[i] = new List<MediaSample>();
      }
      this.TrackSampleStartIndexes = new int[trackCount];  // Items initialized to 0.
      this.TrackFirstBlocks = new MediaDataBlock[trackCount];  // Items initialized to null.
      this.TrackFirstFileDatas = new byte[trackCount][];  // Items initialized to null.
      this.ChunkStartTimeReceiver = null;
    }

    /*implements IStoppable*/ public void Stop() {
      this.IsStopped = true;  // TODO: Is `volatile' enough to make this thread-safe?
    }

    /*implements*/ public int GetTrackCount() {
      return this.TrackFirstBlocks.Length;
    }

    /*implements*/ public ulong GetTrackEndTime(int trackIndex) {
      return this.Tracks[trackIndex].NextStartTime;
    }

    /*implements*/ public void StartChunks(IChunkStartTimeReceiver chunkStartTimeReceiver) {
      this.ChunkStartTimeReceiver = chunkStartTimeReceiver;
      for (int trackIndex = 0; trackIndex < this.Tracks.Count; ++trackIndex) {
        // Propagate the start time of the verify first chunk of the track. If resuming from a .muxstate,
        // this also checks that the .muxstate is consistent with what we want to do.
        // TODO: If not consistent, don't use the .muxstate instead of making the process abort.
        this.ChunkStartTimeReceiver.SetChunkStartTime(
            trackIndex, this.Tracks[trackIndex].DownloadedChunkCount, this.Tracks[trackIndex].NextStartTime);
      }
    }

    // Returns false on EOF, true on success.
    private bool DownloadNextChunk(int trackIndex) {
      IList<MediaSample> mediaSamples = this.TrackSamples[trackIndex];
      Track track = this.Tracks[trackIndex];
      if ((track.DownloadedChunkCount >= track.TrackInfo.Stream.ChunkCount && !this.IsLive) ||
          this.IsStopped) return false;  // EOF.
      this.TrackSampleStartIndexes[trackIndex] = 0;
      mediaSamples.Clear();
      while (mediaSamples.Count == 0) {  // Download next chunk.
        int chunkIndex = track.DownloadedChunkCount;
        if ((chunkIndex >= track.TrackInfo.Stream.ChunkCount && !this.IsLive) ||
            this.IsStopped) return false;
        ulong chunkStartTime = track.NextStartTime;
        if (this.IsLive && chunkStartTime * 1e7 / this.TimeScale >= this.StopAfter) return false;
        if (track.TrackInfo.Stream.ChunkList.Count > chunkIndex) {
          ulong chunkStartTimeInList = track.TrackInfo.Stream.ChunkList[chunkIndex].StartTime;
          if (chunkStartTime != chunkStartTimeInList) {
            throw new Exception(
              "StartTime mismatch in .ism and chunk files: ism=" + chunkStartTimeInList +
              " file=" + chunkStartTime + " track=" + trackIndex + " chunk=" + chunkIndex);
          }
        }
        byte[] contents = Downloader.DownloadChunk(track.TrackInfo, mediaSamples, chunkStartTime, this.ManifestParentPath,
                                                   this.IsLive, out track.NextStartTime);
        if (contents == null) {
          this.IsStopped = true;
          // The URL has been printed by DownloadChunk above.
          // TODO: Finish muxing the .mkv so the user gets something complete.
          throw new Exception("Error downloading chunk " + chunkIndex + " of track " + trackIndex);
        }
        ++track.DownloadedChunkCount;
        if (track.TrackInfo.Stream.ChunkList.Count > chunkIndex) {
          ulong nextStartTimeInList =
              chunkStartTime + track.TrackInfo.Stream.ChunkList[chunkIndex].Duration;
          if (track.NextStartTime != nextStartTimeInList) {
            throw new Exception(
              "next StartTime mismatch in .ism and chunk files: ism=" + nextStartTimeInList +
              " file=" + track.NextStartTime + " track=" + trackIndex +
              " chunk=" + chunkIndex);
          }
        }
        this.ChunkStartTimeReceiver.SetChunkStartTime(
            trackIndex, track.DownloadedChunkCount, track.NextStartTime);
        this.TrackFirstFileDatas[trackIndex] = contents;
        // Notify the listener of the successful download.
        ulong trackTotalDuration = track.NextStartTime - this.MinStartTime;
        if (this.TotalDuration < trackTotalDuration) {
          this.TotalDuration = trackTotalDuration;
          ulong reachedTicks = (ulong)(this.TotalDuration * 1e7 / this.TimeScale);
          this.DisplayDuration(reachedTicks, this.TotalTicks);
        }
      }
      return true;
    }

    /*implements*/ public MediaDataBlock PeekBlock(int trackIndex) {
      if (this.TrackFirstBlocks[trackIndex] != null) return this.TrackFirstBlocks[trackIndex];
      IList<MediaSample> mediaSamples = this.TrackSamples[trackIndex];
      int k = this.TrackSampleStartIndexes[trackIndex];
      if (k >= mediaSamples.Count) {  // Finished processing this chunk, download next chunk.
        if (!DownloadNextChunk(trackIndex)) return null;
        k = 0;
      }

      MediaSample mediaSample = mediaSamples[k];
      mediaSamples[k] = null;  // Save memory once this function returns.
      return this.TrackFirstBlocks[trackIndex] = new MediaDataBlock(
          new ArraySegment<byte>(this.TrackFirstFileDatas[trackIndex], (int)mediaSample.Offset, mediaSample.Length),
          mediaSample.StartTime, mediaSample.IsKeyFrame);
    }

    /*implements*/ public void ConsumeBlock(int trackIndex) {
      if (this.TrackFirstBlocks[trackIndex] == null && this.PeekBlock(trackIndex) == null) {
        throw new Exception("ASSERT: No MediaSample to consume.");
      }
      this.TrackFirstBlocks[trackIndex] = null;  // Save memory and signify for the next call to PeekBlock.
      if (++this.TrackSampleStartIndexes[trackIndex] >= this.TrackSamples[trackIndex].Count) {
        this.TrackSamples[trackIndex].Clear();  // Save memory.
        this.TrackFirstFileDatas[trackIndex] = null;  // Save memory.
      }
    }

    /*implements:*/ public void ConsumeBlocksUntil(int trackIndex, ulong startTime) {
      // This is correct for ETA only if ConsumeBlocksUntil is called once.
      int k = this.TrackSampleStartIndexes[trackIndex];
      IList<MediaSample> mediaSamples = this.TrackSamples[trackIndex];
      int mediaSampleCount = mediaSamples.Count;
      if (this.TrackFirstBlocks[trackIndex] != null) {
        // TODO: Test this branch.
        if (this.TrackFirstBlocks[trackIndex].StartTime > startTime) return;  // Nothing to consume.
        this.TrackFirstBlocks[trackIndex] = null;  // Save memory and signify for the next call to PeekBlock.
        if (k < mediaSampleCount && mediaSamples[k].StartTime > startTime) {
          throw new Exception("ASSERT: Inconsistent TrackFirstBlocks and TrackSamples.");
        }
      }
      Track track = this.Tracks[trackIndex];
      // GetChunkStartTime returns ulong.MaxValue if the chunk index is too large for it. Good. It shouldn't happen
      // though, because the next start time after track.DownloadedChunkCount is always available.
      if (k < mediaSampleCount &&
          this.ChunkStartTimeReceiver.GetChunkStartTime(trackIndex, track.DownloadedChunkCount) > startTime) {
        // We may find where to stop within the current chunk (mediaSamples).
        // TODO: Test this branch.
        for (; k < mediaSampleCount; ++k) {
          if (mediaSamples[k].StartTime > startTime) {
            this.TrackSampleStartIndexes[trackIndex] = k;
            return;
          }
        }
      }
      // Consumed the whole mediaSamples.
      mediaSamples.Clear();  // Save memory.
      this.TrackFirstFileDatas[trackIndex] = null;  // Save memory.
      this.TrackSampleStartIndexes[trackIndex] = 0;

      // Consume chunks which start too early. This step makes resuming downloads very fast, because there are whole
      // chunk files which we don't have to download again.
      //
      // GetChunkStartTime returns ulong.MaxValue if the chunk index is too large for it. Good.
      //
      // We could use track.TrackInfo.Stream.ChunkList here to consume more in this loop, but by design we don't rely
      // on ChunkList. Using ChunkList here wouldn't make resuming previous downloads (using .muxstate) faster,
      // because GetChunkStartTime provides the necessary speedup for that.
      //
      // TODO: Do a binary search. (The effect of this optimization would be negligible.)
      ulong nextNextChunkStartTime;
      while ((nextNextChunkStartTime = this.ChunkStartTimeReceiver.GetChunkStartTime(
                  trackIndex, track.DownloadedChunkCount + 1)) <= startTime) {
             // Consume chunk with index track.DownloadedChunkCount.
             track.NextStartTime = nextNextChunkStartTime;
             ++track.DownloadedChunkCount;
      }

      // At this point the next chunk (track.DownloadedChunkCount) starts <= startTime, and the chunk after that
      // (track.DownloadedChunkCount + 1) doesn't exist or starts > startTime. So we load the next chunk, and consume
      // every mediaSample starting <= startTime in it. That's enough, because there is nothing to consume in further
      // chunks (starting with the ``after that'' chunk). We have a `while' loop in case GetChunkStartTime above
      // has returned ulong.MaxValue, so we have do download more in order to figure out what to consume.
      while (DownloadNextChunk(trackIndex)) {
        mediaSampleCount = mediaSamples.Count;
        if (mediaSampleCount == 0) {
          throw new Exception("ASSERT: Expected media samples after download.");
        }
        for (k = 0; k < mediaSampleCount; ++k) {
          if (mediaSamples[k].StartTime > startTime) {
            this.TrackSampleStartIndexes[trackIndex] = k;
            return;
          }
        }
        mediaSamples.Clear();  // Save memory.
      }
      // Just reached EOF on trackIndex.
      this.TrackSamples[trackIndex].Clear();  // Save memory.
      this.TrackFirstFileDatas[trackIndex] = null;  // Save memory.
    }
  }

  // TODO: Move most of Downloader outside Downloader.
  public class Downloader {
    // Exactly one of manifestUri and manifestPath must be set.
    public static void DownloadAndMux(Uri manifestUri, string manifestPath, string mkvPath, bool isDeterministic, TimeSpan stopAfter,
                                      SetupStop setupStop, DisplayDuration displayDuration) {
      string manifestParentPath = null;  // A null indicates a remote manifest file.
      ManifestInfo manifestInfo;
      if (manifestPath != null) {
        manifestParentPath = Path.GetDirectoryName(manifestPath);
        Console.WriteLine("Parsing local manifest file: " + manifestPath);
        using (FileStream manifestStream = new FileStream(manifestPath, FileMode.Open)) {
          manifestInfo = ManifestInfo.ParseManifest(manifestStream, /*manifestUri:*/new Uri(LOCAL_URL_PREFIX));
        }
                        } else {
        Console.WriteLine("Downloading and parsing manifest: " + manifestUri);
        WebClient webClient = new WebClient();
        using (Stream manifestStream = webClient.OpenRead(manifestUri)) {
          manifestInfo = ManifestInfo.ParseManifest(manifestStream, manifestUri);
        }
      }
      Console.Write(manifestInfo.GetDescription());
      
      IList<Track> tracks = new List<Track>();
      foreach (StreamInfo streamInfo in manifestInfo.SelectedStreams) {
        foreach (TrackInfo trackInfo in streamInfo.SelectedTracks) {
          tracks.Add(new Track(trackInfo));
        }
      }
      IList<TrackEntry> trackEntries = new List<TrackEntry>();
      IList<IList<MediaSample>> trackSamples = new List<IList<MediaSample>>();
      for (int i = 0; i < tracks.Count; ++i) {
        trackEntries.Add(tracks[i].TrackInfo.TrackEntry);
        trackEntries[i].TrackNumber = (ulong)(i + 1);
        trackSamples.Add(new List<MediaSample>());
      }
      for (int i = 0; i < tracks.Count; i++) {
        // TODO: Add a facility to start live streams from a later chunk (it was chunkIndex=10 previously).
        // Our design allows for an empty ChunkList, in case live streams are growing.
        tracks[i].NextStartTime = tracks[i].TrackInfo.Stream.ChunkList.Count == 0 ? 0 :
                                  tracks[i].TrackInfo.Stream.ChunkList[0].StartTime;
      }
      // TODO: Test for live streams (see the StackOverflow question).
      Console.WriteLine("Also muxing selected tracks to MKV: " + mkvPath);
      try {
        if (Directory.GetParent(mkvPath) != null &&
            !Directory.GetParent(mkvPath).Exists)
          Directory.GetParent(mkvPath).Create();
      } catch (IOException) {
        // TODO: Add nicer error reporting, without a stack trace.
        throw new Exception("Cannot not create the directory of .mkv: " + mkvPath);
      }
      ulong maxTrackEndTimeHint = manifestInfo.Duration;
      for (int i = 0; i < tracks.Count; ++i) {
        IList<ChunkInfo> chunkInfos = tracks[i].TrackInfo.Stream.ChunkList;
        int j = chunkInfos.Count - 1;
        if (j >= 0) {  // Our design allows for an empty ChunkList.
          ulong trackDuration = chunkInfos[j].StartTime + chunkInfos[j].Duration;
          if (maxTrackEndTimeHint < trackDuration) maxTrackEndTimeHint = trackDuration;
        }
      }
      // The .muxstate file is approximately 1/5441.43 of the size of the .mkv.
      // The .muxstate file is around 28.088 bytes per second. TODO: Update this after n.
      // Sometimes totalDuration of video is 1156420602, audio is 1156818141 (larger), so we just take the maximum.
      string muxStatePath = Path.ChangeExtension(mkvPath, "muxstate");
      string muxStateOldPath = muxStatePath + ".old";
      byte[] oldMuxState = null;
      if (File.Exists(muxStatePath)) {  // False for directories.
        using (FileStream fileStream = new FileStream(muxStatePath, FileMode.Open)) {
          oldMuxState = ReadFileStream(fileStream);
        }
        if (oldMuxState.Length > 0) {
          // File.Move fails with IOException if the destination already exists.
          // C# and .NET SUXX: There is no atomic overwrite-move.
          try {
            File.Move(muxStatePath, muxStateOldPath);
          } catch (IOException) {
            File.Replace(muxStatePath, muxStateOldPath, null, true);
          }
        }
      }
      DownloadingMediaDataSource source = new DownloadingMediaDataSource(
          tracks, manifestParentPath, manifestInfo.TimeScale,
                            manifestInfo.IsLive, (ulong)stopAfter.Ticks, manifestInfo.TotalTicks, displayDuration);
      setupStop(manifestInfo.IsLive, source);
      MuxStateWriter muxStateWriter = new MuxStateWriter(new FileStream(muxStatePath, FileMode.Create));
      try {
        MkvUtils.WriteMkv(mkvPath, trackEntries, source, maxTrackEndTimeHint, manifestInfo.TimeScale, isDeterministic,
                          oldMuxState, muxStateWriter);
      } finally {
        muxStateWriter.Close();
      }
      File.Delete(muxStatePath);
      if (File.Exists(muxStateOldPath)) {
        File.Delete(muxStateOldPath);
      }
    }

    private static readonly string LOCAL_URL_PREFIX = "http://local/";

    // Modifies track in place, and appends to mediaSamples.
    // Returns null on network failure or empty file, otherwise it returns a non-empty array.
    // The chunk file contents are returned, and are not saved to disk.
    internal static byte[] DownloadChunk(TrackInfo trackInfo, IList<MediaSample> mediaSamples, ulong chunkStartTime,
                                         string manifestParentPath, bool isLive, out ulong nextStartTime) {
      nextStartTime = 0;  // Set even if null is returned.
      string chunkUrl = trackInfo.Stream.GetChunkUrl(trackInfo.Bitrate, chunkStartTime);
      // TODO: Move TrackInfo away from Track, keep only fields necessary here, excluding ChunkList.
      byte[] downloadedBytes;  // Will be set below.
      if (manifestParentPath != null) {  // It was a local manifest, so read the chunk from a local file.
        if (!chunkUrl.StartsWith(LOCAL_URL_PREFIX)) {
          throw new Exception("ASSERT: Missing local URL prefix.");
        }
        // Example chunk URL: "http://local/QualityLevels(900000)/Fragments(video=0)".
        // TODO: Maybe this needs some further unescaping of %5A etc. (can be tested locally).
        string chunkDownloadedPath = manifestParentPath + Path.DirectorySeparatorChar +
            chunkUrl.Substring(LOCAL_URL_PREFIX.Length).Replace('/', Path.DirectorySeparatorChar);
        using (FileStream fileStream = new FileStream(chunkDownloadedPath, FileMode.Open)) {
          downloadedBytes = ReadFileStream(fileStream);
        }
        if (downloadedBytes.Length == 0) {
          Console.WriteLine();
          Console.WriteLine("Local chunk file empty: " + chunkDownloadedPath);
          return null;
        }
      } else {  // Download from the web.
        WebClient webClient = new WebClient();
        try {
          // TODO: What's the timeout on this?
          downloadedBytes = webClient.DownloadData(chunkUrl);
        } catch (WebException) {
          Thread.Sleep(isLive ? 4000 : 2000);
          try {
            downloadedBytes = webClient.DownloadData(chunkUrl);
          } catch (WebException) {
            Thread.Sleep(isLive ? 6000 : 3000);
            try {
              downloadedBytes = webClient.DownloadData(chunkUrl);
            } catch (WebException) {
              // It's an acceptable behavior to stop downloading live streams after 10 seconds.
              // If it's really live, there should be a new chunk update available every 10 seconds.
              Console.WriteLine();
              Console.WriteLine("Error downloading chunk " + chunkUrl);
              return null;
            }
          }
        }
      }
      if (downloadedBytes.Length == 0) {
        Console.WriteLine();
        Console.WriteLine("Chunk empty: " + chunkUrl);
        return null;
      }
      Fragment fragment = new Fragment(downloadedBytes, 0, downloadedBytes.Length);
      // This appends to mediaSamples.
      nextStartTime = ParseFragment(fragment, mediaSamples, trackInfo.Stream.Type, chunkStartTime);
      if (nextStartTime <= chunkStartTime) {
        throw new Exception("Found empty chunk.");
      }
      return downloadedBytes;
    }

    // TODO: Move this to a generic utility class.
    private static byte[] ReadFileStream(FileStream fileStream) {
      int fileSize = (int)fileStream.Length;  // TODO: Can this be negative etc. for non-regular files?
      if (fileSize <= 0) return new byte[0];
      byte[] array = new byte[fileSize];
      if (fileSize != fileStream.Read(array, 0, fileSize)) {
        throw new Exception("ASSERT: Short read from MediaSample file " + fileStream.Name + ", wanted " + fileSize);
      }
      if (0 != fileStream.Read(array, 0, 1)) {
        throw new Exception("ASSERT: Long read from MediaSample file " + fileStream.Name + ", wanted " + fileSize);
      }
      return array;
    }

    // Appends to `samples'.
    // Returns nextStartTime.
    private static ulong ParseFragment(Fragment fragment, IList<MediaSample> samples, MediaStreamType type,
                                       ulong chunkStartTime) {
                        // A fragment is a ``chunk'' (with a corresponding <c d=....> in its duration) in the ISM manifest file.
      TrackFragmentBox traf = fragment.moof.traf;
      if (traf.tfxd != null) {
        chunkStartTime = traf.tfxd.FragmentAbsoluteTime;
      }
      ulong nextStartTime = 0uL;
      if (traf.tfrf != null && traf.tfrf.Array.Length > 0u) {
        nextStartTime = traf.tfrf.Array[0].FragmentAbsoluteTime;
      }
      long sampleOffset = fragment.mdat.Start;
      uint defaultSampleSize = traf.tfhd.default_sample_size;
      uint sampleSize = defaultSampleSize;
      uint defaultSampleDuration = traf.tfhd.default_sample_duration;
      uint duration = defaultSampleDuration;
      ulong totalDuration = 0;
      uint sampleCount = traf.trun.sample_count;
      TrackRunBox.Element[] array = defaultSampleSize == 0u || defaultSampleDuration == 0u ? traf.trun.array : null;
      for (uint i = 0; i < sampleCount; ++i) {
        if (defaultSampleSize == 0u) {
          sampleSize = array[i].sample_size;
        }
        if (defaultSampleDuration == 0u) {
          duration = array[i].sample_duration;
        }
        // We add a few dozen MediaSample entries for a chunk.
        samples.Add(new MediaSample(sampleOffset, (int)sampleSize, chunkStartTime,
                                    /*isKeyFrame:*/i == 0 || type == MediaStreamType.Audio));
        chunkStartTime += (ulong)duration;
        totalDuration += (ulong)duration;
        sampleOffset += sampleSize;
      }
      return nextStartTime != 0uL ? nextStartTime : chunkStartTime;
    }
  }
}
