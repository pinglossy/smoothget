using Smoothget.Download;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: AssemblyVersion("3.0.0.3")]

namespace Smoothget {
  internal interface IUrlProcessor {
    // Processes inputUrls sequentially, appending the results to outputUrls.
    void Process(IList<string> inputUrls, IList<string> outputUrls);
    int GetOrder();
  }

  internal class ManifestUrlProcessor : IUrlProcessor {
    /*implements*/ public void Process(IList<string> inputUrls, IList<string> outputUrls) {
      foreach (string url in inputUrls) {
        if (url.EndsWith("/manifest") || url.EndsWith("/Manifest")) {
          outputUrls.Add(url.Substring(0, url.Length - 9));  // "/Manifest".Length == 9.
        } else {
          outputUrls.Add(url);
        }
      }
    }
    /*implements*/ public int GetOrder() { return 999; }
  }

  internal class MainClass {
    private static void Main(string[] args) {
      Logo();
      int j;
      bool isDeterministic = false;
      for (j = 0; j < args.Length; ++j) {
        if (args[j] == "--") {
          ++j;
          break;
        } else if (args[j].Length == 0 || args[j] == "-" || args[j][0] != '-') {
          break;
        } else if (args[j] == "--det") {
          isDeterministic = true;
        }
      }
      if (args.Length < j + 2) {
        Help();
      }
      int lastIdx = args.Length - 1;
      string downloadDirectory = args[lastIdx].Trim(Path.GetInvalidFileNameChars()).Trim(Path.GetInvalidPathChars());
      Console.WriteLine("Download directory: " + downloadDirectory);
      string[] urls = new string[lastIdx - j];
      for (int i = j; i < lastIdx; ++i) {
        urls[i - j] = args[i];
      }
      IList<string> partUrls = ProcessUrls(urls);
      Console.WriteLine("Parts to download:");
      for (int i = 0; i < partUrls.Count; i++) {
        Console.WriteLine("  Part URL: " + partUrls[i]);
      }
      Console.WriteLine();
      for (int i = 0; i < partUrls.Count; i++) {
        RecordAndMux(partUrls[i], downloadDirectory, isDeterministic);
      }
      Console.WriteLine("All downloading and muxing done.");
    }

    // May return the same reference (urls).
    private static IList<string> ProcessUrls(IList<string> urls) {
      Type urlProcessorType = typeof(IUrlProcessor);
      List<IUrlProcessor> urlProcessors = new List<IUrlProcessor>();
      foreach (Type type in urlProcessorType.Assembly.GetTypes()) {
        if (type.IsClass && urlProcessorType.IsAssignableFrom(type)) {
          urlProcessors.Add((IUrlProcessor)type.GetConstructor(Type.EmptyTypes).Invoke(null));
        }
      }
      urlProcessors.Sort(CompareUrlProcessorOrder);
      foreach (IUrlProcessor urlProcessor in urlProcessors) {
        List<string> nextUrls = new List<string>();
        urlProcessor.Process(urls, nextUrls);
        urls = nextUrls;
      }
      return urls;
    }

    private static int CompareUrlProcessorOrder(IUrlProcessor a, IUrlProcessor b) {
      return a.GetOrder().CompareTo(b.GetOrder());
    }

    // A thin wrapper for callbacks of duration progress reporting and stopping.
    private class MuxingInteractiveState {
      private bool hasDisplayedDuration;
      private ulong startTicks;
      private ulong reachedBaseTicks;
      private Thread thread;
      private IStoppable stoppable;
      public MuxingInteractiveState() {
        this.hasDisplayedDuration = false;
        this.startTicks = 0;
      }
      public void SetupStop(bool isLive, IStoppable stoppable) {
        if (this.thread != null) {
          throw new Exception("ASSERT: Unexpected thread.");
        }
        if (isLive) {
          this.stoppable = stoppable;
          Console.WriteLine("Press any key to stop recording!");
          this.thread = new Thread(new ThreadStart(StopRecoding));
          this.thread.Start();
        }
      }
      public void Abort() {
        if (this.thread != null) this.thread.Abort();
      }
      public void StopRecoding() {  // Runs in a separate thread parallel with DownloadAndMux.
        Console.ReadKey(true);
        this.stoppable.Stop();
      }
      public void DisplayDuration(ulong reachedTicks, ulong totalTicks) {
        if (!this.hasDisplayedDuration) {
          this.hasDisplayedDuration = true;
          Console.Error.WriteLine("Recording duration:");
        }
        ulong eta = 0;
        if (reachedTicks > 0) {
          ulong nowTicks = (ulong)DateTime.Now.Ticks;
          if (this.startTicks == 0) {
            this.startTicks = nowTicks;
            this.reachedBaseTicks = reachedTicks;
          } else {
            // TODO: Improve the ETA calculation, it seems to be jumping up and down.
            // Here nowTicks and this.startTicks are measured in real time.
            // Here totalTicks, reachedTicks and this.reachedBaseTicks are measured in video
            // timecode.
            double etaDouble = (nowTicks - this.startTicks + 0.0) *
                (totalTicks - reachedTicks) / (reachedTicks - this.reachedBaseTicks);
            if (etaDouble > 0.0 && etaDouble < 3.6e12) eta = (ulong)etaDouble;  // < 100 hours.
          }
        }
        // TODO: Use a StringBuilder.
        string msg = "\r" + new TimeSpan((long)(reachedTicks - reachedTicks % 10000000));
        if (eta != 0) {
          msg += ", ETA " + new TimeSpan((long)(eta - eta % 10000000));  // Round down to whole seconds.
        } else {
          msg += "              ";  // Clear the end (ETA) of the previously displayed message.
        }
        Console.Write(msg);
      }
    }
    private static void RecordAndMux(string ismFileName, string outputDirectory, bool isDeterministic) {
      string mkvPath = outputDirectory + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(ismFileName) + ".mkv";
      string muxStatePath = Path.ChangeExtension(mkvPath, "muxstate");
      if (File.Exists(mkvPath) && !File.Exists(muxStatePath)) {
        Console.WriteLine("Already downloaded MKV: " + mkvPath);
        Console.WriteLine();
        return;
      }
      Console.WriteLine("Will mux to MKV: " + mkvPath);
      string manifestUrl = ismFileName + "/manifest";
      Uri manifestUri;
      string manifestPath;
      if (manifestUrl.StartsWith("http://") || manifestUrl.StartsWith("https://")) {
        manifestUri = new Uri(manifestUrl);
        manifestPath = null;
      } else if (manifestUrl.StartsWith("file://")) {
        // Uri.LocalPath converts %20 back to a space etc. (unlike Uri.AbsolutePath).
        // TODO: Does Uri.LocalPath work on Windows (drive letters, / to \ etc.)?
        manifestUri = null;
        manifestPath = new Uri(manifestUrl).LocalPath;
      } else {
        manifestUri = null;
        manifestPath = manifestUrl;
      }
      DateTime now = DateTime.Now;
      MuxingInteractiveState muxingInteractiveState = new MuxingInteractiveState();
      Downloader.DownloadAndMux(manifestUri, manifestPath, mkvPath, isDeterministic,
                                new TimeSpan(10, 0, 0),  // 10 hours, 0 minutes, 0 seconds for live streams.
                                muxingInteractiveState.SetupStop,
                                muxingInteractiveState.DisplayDuration);
      muxingInteractiveState.Abort();
      Console.Error.WriteLine();
      Console.WriteLine("Downloading finished in " + DateTime.Now.Subtract(now).ToString());
    }
    private static void Logo() {
      AssemblyName name = Assembly.GetEntryAssembly().GetName();
      Console.WriteLine(string.Concat(new object[] { name.Name, " v", name.Version }));
      Console.WriteLine();
    }
    private static void Help() {
      // Console.WriteLine(" Soda Media Center");  // TODO: Where does it come crom?
      AssemblyName name = Assembly.GetEntryAssembly().GetName();
      Console.WriteLine("Microsoft IIS Smooth Streaming downloader and muxer to MKV.");
      Console.WriteLine();
      Console.WriteLine("Supported media stream formats:");  // TODO: Really only these?
      Console.WriteLine("- Audio: AAC, WMA");
      Console.WriteLine("- Video: H264, VC-1");
      Console.WriteLine();
      Console.WriteLine("Usage:");
      Console.WriteLine(name.Name + " [<flag> ...] <source> [...] <output-directory>");
      Console.WriteLine("<source> is an .ism (or /manifest) file or URL.");
      Console.WriteLine("<output-directory> can be just . , a properly named file will be created.");
      Console.WriteLine("Many temporary files and subdirs may be created and left in <output-directory>.");
      Console.WriteLine("--det  Enable deterministic MKV output (no random, no current time).");
      Environment.Exit(1);
    }
  }
}
