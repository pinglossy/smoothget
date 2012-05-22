using Smoothget;
using Smoothget.Mkv;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
namespace Smoothget.Smooth {
  public enum MediaStreamType {
    Audio,
    Video,
    Script
  }

  internal class ManifestInfo {
    public uint MajorVersion;
    public uint MinorVersion;
    public ulong Duration;
    public bool IsLive;
    public ulong TimeScale;
    public Uri Uri;
    public IDictionary<string, string> Attributes;
    public IList<StreamInfo> AvailableStreams;
    public IList<StreamInfo> SelectedStreams;
    public ulong TotalTicks;
    private ManifestInfo(XmlNode element, Uri uri) {
      this.Uri = uri;
      if (element.Name != "SmoothStreamingMedia") {
        throw new Exception("Source element is not a(n) SmoothStreamingMedia element!");
      }
      this.Attributes = Parse.Attributes(element);
      this.MajorVersion = Parse.UInt32Attribute(this.Attributes, "MajorVersion");
      this.MinorVersion = Parse.UInt32Attribute(this.Attributes, "MinorVersion");
      this.Duration = Parse.UInt64Attribute(this.Attributes, "Duration");
      this.IsLive = false;
      if (this.Attributes.ContainsKey("IsLive")) {
        this.IsLive = Parse.BoolAttribute(this.Attributes, "IsLive");
      }
      this.TimeScale = 10000000uL;
      if (this.Attributes.ContainsKey("TimeScale")) {
        this.TimeScale = Parse.UInt64Attribute(this.Attributes, "TimeScale");
      }
      this.AvailableStreams = new List<StreamInfo>();
      foreach (XmlNode element2 in element.SelectNodes("StreamIndex")) {  // Automatic cast to XmlNode.
        this.AvailableStreams.Add(new StreamInfo(element2, this.Uri));
      }
      this.SelectedStreams = new List<StreamInfo>();
      for (int i = 0; i < this.AvailableStreams.Count; i++) {
        if (this.AvailableStreams[i].Type != MediaStreamType.Script) {
          this.SelectedStreams.Add(this.AvailableStreams[i]);
        }
      }
      this.TotalTicks = this.IsLive ? 0 : (ulong)(this.Duration / this.TimeScale * 10000000.0);
    }
    public static ManifestInfo ParseManifest(Stream manifestStream, Uri manifestUri) {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(manifestStream);
      XmlNode xmlNode = xmlDocument.SelectSingleNode("SmoothStreamingMedia");
      if (xmlNode == null) {
        throw new Exception(string.Format("Manifest root element must be <{0}>!", "SmoothStreamingMedia"));
      }
      return new ManifestInfo(xmlNode, manifestUri);
    }
    public string GetDescription() {
      string text = "";  // TODO: Use something like a StringBuilder to avoid O(n^2) concatenation.
      text += "Manifest:\n";
      text += "  Duration: ";
      if (this.IsLive) {
        text += "LIVE\n";
      }
      else {
        text += new TimeSpan((long)this.TotalTicks) + "\n";
      }
      for (int i = 0; i < this.AvailableStreams.Count; i++) {
        object obj = text;
        text = string.Concat(new object[] { obj, "  Stream ", i + 1, ": ", this.AvailableStreams[i].Type, "\n" });
        switch (this.AvailableStreams[i].Type) {
          case MediaStreamType.Audio:
          case MediaStreamType.Video: {
            foreach (TrackInfo trackInfo in this.AvailableStreams[i].AvailableTracks) {
              text += "    " + trackInfo.Description;
              if (this.AvailableStreams[i].SelectedTracks.Contains(trackInfo)) {
                text += " [selected]";
              }
              text += "\n";
            }
            break;
          }
          case MediaStreamType.Script: {
            text += "    Script ignored.\n";
            break;
          }
          default: {
            text += "    WARNING: Unsupported track of type " + this.AvailableStreams[i].Type + "\n";
            break;
          }
        }
      }
      return text;
    }
  }

  internal class StreamInfo {
    private string pureUrl;
    public IDictionary<string, string> Attributes;
    public IDictionary<string, string> CustomAttributes;
    public IList<TrackInfo> AvailableTracks;
    public IList<TrackInfo> SelectedTracks;
    public IList<ChunkInfo> ChunkList;
    public string Subtype;
    public MediaStreamType Type;
    public int ChunkCount;
    public StreamInfo(XmlNode element, Uri manifestUri) {
      if (element.Name != "StreamIndex") {
        throw new Exception("Source element is not a(n) StreamIndex element!");
      }
      this.Attributes = Parse.Attributes(element);
      this.CustomAttributes = Parse.CustomAttributes(element);
      this.Type = Parse.MediaStreamTypeAttribute(this.Attributes, "Type");
      this.Subtype = this.Attributes.ContainsKey("Subtype") ? Parse.StringAttribute(this.Attributes, "Subtype") : "";
      if (this.Attributes.ContainsKey("Url")) {
        this.CheckUrlAttribute();
      }
      this.AvailableTracks = new List<TrackInfo>();
      XmlNodeList xmlNodeList = element.SelectNodes("QualityLevel");
      int i;
      for (i = 0; i < xmlNodeList.Count; ++i) {
        TrackInfo trackInfo;
        if (this.Type == MediaStreamType.Audio) {
          trackInfo = new AudioTrackInfo(xmlNodeList[i], this.Attributes, (uint)i, this);
        } else if (this.Type == MediaStreamType.Video) {
          trackInfo = new VideoTrackInfo(xmlNodeList[i], this.Attributes, (uint)i, this);
        } else {
          continue;
        }
        int num = 0;
        while (num < this.AvailableTracks.Count && this.AvailableTracks[num].Bitrate > trackInfo.Bitrate) {
          num++;
        }
        this.AvailableTracks.Insert(num, trackInfo);
      }
      this.ChunkList = new List<ChunkInfo>();
      XmlNodeList xmlNodeList2 = element.SelectNodes("c");
      ulong num2 = 0uL;
      for (i = 0; i < xmlNodeList2.Count; i++) {
        ChunkInfo chunkInfo = new ChunkInfo(xmlNodeList2[i], (uint)i, num2);
        this.ChunkList.Add(chunkInfo);
        num2 += chunkInfo.Duration;
      }
      if (this.Attributes.ContainsKey("Chunks")) {
        uint chunkCount = Parse.UInt32Attribute(this.Attributes, "Chunks");
        if (this.ChunkList.Count > 0 && this.ChunkList.Count != chunkCount) {
          throw new Exception("Chunk count mismatch: c=" + this.ChunkList.Count + " chunks=" + chunkCount);
        }
        this.ChunkCount = (int)chunkCount;
      } else {
        this.ChunkCount = this.ChunkList.Count;  // Can be 0 if no `<c' tags.
      }
      this.pureUrl = manifestUri.AbsoluteUri;
      this.pureUrl = this.pureUrl.Substring(0, this.pureUrl.LastIndexOf('/'));
      this.SelectedTracks = new List<TrackInfo>();
      if (this.AvailableTracks.Count > 0) {
        this.SelectedTracks.Add(this.AvailableTracks[0]);
      }
    }
    private void CheckUrlAttribute() {
      string text = Parse.StringAttribute(this.Attributes, "Url");
      string[] array = text.Split(new char[] {
        '/'
      });
      if (array.Length != 2) {
        throw new Exception("Invalid UrlPattern!");
      }
      string text2 = array[0];
      string text3 = array[1];
      array = text2.Split(new char[] {
        '(', 
        ')'
      });
      if (array.Length != 3 || array[2].Length != 0) {
        throw new Exception("Invalid QualityLevelsPattern!");
      }
      if (array[0] != "QualityLevels") {
        throw new Exception("Invalid QualityLevelsNoun!");
      }
      string text4 = array[1];
      array = text4.Split(new char[] {
        ','
      });
      if (array.Length > 2) {
        throw new Exception("Invalid QualityLevelsPredicatePattern!");
      }
      if (array[0] != "{bitrate}" && array[0] != "{Bitrate}") {
        throw new Exception("Missing BitrateSubstitution!");
      }
      if (array.Length == 2 && array[1] != "{CustomAttributes}") {
        throw new Exception("Missing CustomAttributesSubstitution!");
      }
      array = text3.Split(new char[] {
        '(', 
        ')'
      });
      if (array.Length != 3 || array[2].Length != 0) {
        throw new Exception("Invalid FragmentsPattern!");
      }
      if (array[0] != "Fragments") {
        throw new Exception("Invalid FragmentsNoun!");
      }
      string text5 = array[1];
      array = text5.Split(new char[] {
        '='
      });
      if (array.Length != 2) {
        throw new Exception("Invalid FragmentsPatternPredicate!");
      }
      if (this.Attributes.ContainsKey("Name")) {
        if (array[0] != Parse.StringAttribute(this.Attributes, "Name")) {
          throw new Exception("Missing TrackName!");
        }
      }
      else {
        if (array[0] != Parse.StringAttribute(this.Attributes, "Type")) {
          throw new Exception("Missing TrackName!");
        }
      }
      if (array[1] != "{start time}" && array[1] != "{start_time}") {
        throw new Exception("Missing StartTimeSubstitution!");
      }
    }
    public string GetChunkUrl(uint bitrate, ulong startTime) {
      return this.pureUrl + "/" + Parse.StringAttribute(this.Attributes, "Url")
                    .Replace("{bitrate}", bitrate.ToString())
                    .Replace("{Bitrate}", bitrate.ToString())
                    .Replace("{start time}", startTime.ToString())
                    .Replace("{start_time}", startTime.ToString());
    }
  }

  internal class TrackInfo {
    public IDictionary<string, string> Attributes;
    public uint Bitrate;
    public IDictionary<string, string> CustomAttributes;
    public uint Index;
    public StreamInfo Stream;
    public string Description;
    public TrackEntry TrackEntry;
    public TrackInfo(XmlNode element, uint index, StreamInfo stream) {
      if (element.Name != "QualityLevel") {
        throw new Exception("Source element is not a(n) QualityLevel element!");
      }
      this.Attributes = Parse.Attributes(element);
      this.CustomAttributes = Parse.CustomAttributes(element);
      this.Index = index;
      if (this.Attributes.ContainsKey("Index")) {
        this.Index = Parse.UInt32Attribute(this.Attributes, "Index");
      }
      if (this.Index != index) {
        throw new Exception("Missing quality level index: " + index);
      }
      this.Bitrate = Parse.UInt32Attribute(this.Attributes, "Bitrate");
      this.Stream = stream;
    }
  }

  internal class ChunkInfo {
    public uint Index;
    public ulong Duration;
    public ulong StartTime;
    public IDictionary<string, string> Attributes;
    public ChunkInfo(XmlNode element, uint index, ulong starttime) {
      if (element.Name != "c") {
        throw new Exception("Source element is not a(n) c element!");
      }
      this.Attributes = Parse.Attributes(element);
      this.Index = index;
      if (this.Attributes.ContainsKey("n")) {
        this.Index = Parse.UInt32Attribute(this.Attributes, "n");
      }
      if (this.Index != index) {
        throw new Exception("Missing chunk index: " + index);
      }
      this.StartTime = starttime;
      if (this.Attributes.ContainsKey("t")) {
        this.StartTime = Parse.UInt64Attribute(this.Attributes, "t");
      }
      if (this.Attributes.ContainsKey("d")) {
        this.Duration = Parse.UInt64Attribute(this.Attributes, "d");
      }
    }
  }

  internal class AudioTrackInfo : TrackInfo {
    private static string GetCodecNameForAudioTag(ushort audioTag) {
      switch (audioTag) {
        case 1: { return "LPCM"; }
        case 85: { return "MP3"; }
        case 255: case 5633: { return "AAC"; }
        case 353: { return "WMA2"; }
        case 354: { return "WMAP"; }
        case 65534: { return "Vendor-extensible format"; }
        default: { throw new Exception("Unsupported AudioTag '" + audioTag + "'!"); }
      }
    }
    private class WaveFormatEx {
      public ushort wFormatTag;
      public ushort nChannels;
      public uint nSamplesPerSec;
      public uint nAvgBytesPerSec;
      public ushort nBlockAlign;
      public ushort wBitsPerSample;
      public byte[] DecoderSpecificData;  // Max size is 65535 bytes.
      public WaveFormatEx(byte[] data) {
        if (data == null || data.Length < 18) {
          throw new Exception("Invalid WaveFormatEx data!");
        }
        ushort num = BitConverter.ToUInt16(data, 16);
        if (data.Length != (int)(18 + num)) {
          throw new Exception("Invalid cbSize value!");
        }
        this.wFormatTag = BitConverter.ToUInt16(data, 0);
        this.nChannels = BitConverter.ToUInt16(data, 2);
        this.nSamplesPerSec = (uint)BitConverter.ToUInt16(data, 4);
        this.nAvgBytesPerSec = (uint)BitConverter.ToUInt16(data, 8);
        this.nBlockAlign = BitConverter.ToUInt16(data, 12);
        this.wBitsPerSample = BitConverter.ToUInt16(data, 14);
        this.DecoderSpecificData = new byte[(int)num];
        Buffer.BlockCopy(data, 18, this.DecoderSpecificData, 0, this.DecoderSpecificData.Length);
      }
      public WaveFormatEx(ushort wFormatTag, ushort nChannels, uint nSamplesPerSec, uint nAvgBytesPerSec, ushort nBlockAlign,
                          ushort wBitsPerSample, byte[] DecoderSpecificData) {
        if (DecoderSpecificData != null && DecoderSpecificData.Length > 65535) {
          throw new Exception("DecoderSpecificData too long.");
        }
        this.wFormatTag = wFormatTag;
        this.nChannels = nChannels;
        this.nSamplesPerSec = nSamplesPerSec;
        this.nAvgBytesPerSec = nAvgBytesPerSec;
        this.nBlockAlign = nBlockAlign;
        this.wBitsPerSample = wBitsPerSample;
        this.DecoderSpecificData = DecoderSpecificData;
      }
      public byte[] GetBytes() {
        byte[] array = new byte[18 + this.DecoderSpecificData.Length];
        Buffer.BlockCopy(BitConverter.GetBytes(this.wFormatTag), 0, array, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(this.nChannels), 0, array, 2, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(this.nSamplesPerSec), 0, array, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(this.nAvgBytesPerSec), 0, array, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(this.nBlockAlign), 0, array, 12, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(this.wBitsPerSample), 0, array, 14, 2);
        Buffer.BlockCopy(BitConverter.GetBytes((ushort)this.DecoderSpecificData.Length), 0, array, 16, 2);
        if (array.Length != 18) {
          Buffer.BlockCopy(this.DecoderSpecificData, 0, array, 18, this.DecoderSpecificData.Length);
        }
        return array;
      }
    }
    private static readonly uint[] MP4_SamplingRate = new uint[] {
      96000u, 88200u, 64000u, 48000u, 44100u, 32000u, 24000u, 22050u, 16000u, 12000u, 11025u, 8000u, 7350u, 0u, 0u, 0u };
    private static readonly string MP4_Channels = "\x00\x01\x02\x03\x04\x05\x06\x08";
    private static byte[] GetAudioSpecificConfigBytes(uint samplingRate, byte numberOfChannels) {
      // public enum Profile : byte {	MAIN = 1, LC, SSR, LTP,	SBR,Scalable }
      // ushort num = (ushort)((ushort)profile << 11);
      ushort num = (ushort)((ushort)2 << 11);  // Profile.LC.
      int num2 = 0;
      while (MP4_SamplingRate[num2] != samplingRate && num2 < MP4_SamplingRate.Length) {
        num2++;
      }
      if (num2 > MP4_SamplingRate.Length) {
        throw new Exception("Invalid sampling rate!");
      }
      num += (ushort)((ushort)num2 << 7);
      num2 = 0;
      while (MP4_Channels[num2] != numberOfChannels && num2 < MP4_Channels.Length) {
        num2++;
      }
      if (num2 > MP4_Channels.Length) {
        throw new Exception("Invalid number of channels!");
      }
      num += (ushort)((ushort)num2 << 3);
      return Utils.InplaceReverseBytes(BitConverter.GetBytes(num));
    }
    
    public AudioTrackInfo(XmlNode element, IDictionary<string, string> streamAttributes, uint index, StreamInfo stream) : base(element, index, stream) {
      WaveFormatEx waveFormatEx;
      if (base.Attributes.ContainsKey("WaveFormatEx")) {
        byte[] data = Parse.HexStringAttribute(base.Attributes, "WaveFormatEx");
        waveFormatEx = new WaveFormatEx(data);
      }
      else {
        ushort wFormatTag = Parse.UInt16Attribute(base.Attributes, "AudioTag");
        ushort nChannels = Parse.UInt16Attribute(base.Attributes, "Channels");
        uint nSamplesPerSec = Parse.UInt32Attribute(base.Attributes, "SamplingRate");
        uint num = Parse.UInt32Attribute(base.Attributes, "Bitrate");
        ushort nBlockAlign = Parse.UInt16Attribute(base.Attributes, "PacketSize");
        ushort wBitsPerSample = Parse.UInt16Attribute(base.Attributes, "BitsPerSample");
        byte[] decoderSpecificData = Parse.HexStringAttribute(base.Attributes, "CodecPrivateData");
        waveFormatEx = new WaveFormatEx(wFormatTag, nChannels, nSamplesPerSec, num / 8u, nBlockAlign, wBitsPerSample, decoderSpecificData);
      }
      byte[] audioInfoBytes = MkvUtils.GetAudioInfoBytes(
          waveFormatEx.nSamplesPerSec, (ulong)waveFormatEx.nChannels, (ulong)waveFormatEx.wBitsPerSample);
      switch (waveFormatEx.wFormatTag) {
        case 353: case 354: {
          base.TrackEntry = new TrackEntry(TrackType.Audio, audioInfoBytes, CodecID.A_MS, waveFormatEx.GetBytes());
          break;
        }
        case 255: case 5633: {
          base.TrackEntry = new TrackEntry(TrackType.Audio, audioInfoBytes, CodecID.A_AAC, GetAudioSpecificConfigBytes(
              waveFormatEx.nSamplesPerSec, (byte)waveFormatEx.nChannels));
          break;
        }
        case 1: {
          throw new Exception("Unsupported audio format: 'LPCM'!");
        }
        case 65534: {
          throw new Exception("Unsupported audio format: 'Vendor-extensible format'!");
        }
        default: {
          throw new Exception("Unsupported AudioTag: '" + waveFormatEx.wFormatTag + "'");
        }
      }
      if (base.Attributes.ContainsKey("Name")) {
        base.TrackEntry.Name = Parse.StringAttribute(streamAttributes, "Name");
      }
      base.TrackEntry.Language = LanguageID.Hungarian;  // TODO: Make this configurable.
      base.Description = string.Format("{0} {1} channels {2} Hz @ {3} kbps", new object[] {
          GetCodecNameForAudioTag(waveFormatEx.wFormatTag), waveFormatEx.nChannels, waveFormatEx.nSamplesPerSec, 
          base.Bitrate / 1000u });
    }
  }

  internal class VideoTrackInfo : TrackInfo {
    private static byte[] GetBitmapInfoHeaderBytes(int biWidth, int biHeight, ushort biPlanes, ushort biBitCount,
                                                   uint biCompression, uint biSizeImage, int biXPelsPerMeter, int biYPelsPerMeter,
                                                   uint biClrUsed, uint biClrImportant, byte[] codecPrivateData) {
      int biSize = 40 + codecPrivateData.Length;
      byte[] array = new byte[biSize];
      Buffer.BlockCopy(BitConverter.GetBytes(biSize), 0, array, 0, 4);
      Buffer.BlockCopy(BitConverter.GetBytes(biWidth), 0, array, 4, 4);
      Buffer.BlockCopy(BitConverter.GetBytes(biHeight), 0, array, 8, 4);
      Buffer.BlockCopy(BitConverter.GetBytes(biPlanes), 0, array, 12, 2);
      Buffer.BlockCopy(BitConverter.GetBytes(biBitCount), 0, array, 14, 2);
      Buffer.BlockCopy(BitConverter.GetBytes(biCompression), 0, array, 16, 4);
      Buffer.BlockCopy(BitConverter.GetBytes(biSizeImage), 0, array, 20, 4);
      Buffer.BlockCopy(BitConverter.GetBytes(biXPelsPerMeter), 0, array, 24, 4);
      Buffer.BlockCopy(BitConverter.GetBytes(biYPelsPerMeter), 0, array, 28, 4);
      Buffer.BlockCopy(BitConverter.GetBytes(biClrUsed), 0, array, 32, 4);
      Buffer.BlockCopy(BitConverter.GetBytes(biClrImportant), 0, array, 36, 4);
      Buffer.BlockCopy(codecPrivateData, 0, array, 40, codecPrivateData.Length);
      return array;
    }
    public VideoTrackInfo(XmlNode element, IDictionary<string, string> streamAttributes, uint index, StreamInfo stream)
        : base(element, index, stream) {
      uint pixelWidth = base.Attributes.ContainsKey("MaxWidth") ? Parse.UInt32Attribute(base.Attributes, "MaxWidth") :
                        base.Attributes.ContainsKey("Width") ? Parse.UInt32Attribute(base.Attributes, "Width") :
                        streamAttributes.ContainsKey("MaxWidth") ? Parse.UInt32Attribute(streamAttributes, "MaxWidth") : 0u;
      if (pixelWidth == 0u) {
        throw new Exception("Missing video width attribute!");
      }
      uint pixelHeight = base.Attributes.ContainsKey("MaxHeight") ? Parse.UInt32Attribute(base.Attributes, "MaxHeight") :
                         base.Attributes.ContainsKey("Height") ? Parse.UInt32Attribute(base.Attributes, "Height") :
                         streamAttributes.ContainsKey("MaxHeight") ? Parse.UInt32Attribute(streamAttributes, "MaxHeight") : 0u;
      if (pixelHeight == 0u) {
        throw new Exception("Missing video height attribute!");
      }
      uint displayWidth = streamAttributes.ContainsKey("DisplayWidth") ?
                          Parse.UInt32Attribute(streamAttributes, "DisplayWidth") : 0u;
      if (displayWidth == 0u) {
        displayWidth = pixelWidth;
      }
      uint displayHeight = streamAttributes.ContainsKey("DisplayHeight") ?
                           Parse.UInt32Attribute(streamAttributes, "DisplayHeight") : 0u;
      if (displayHeight == 0u) {
        displayHeight = pixelHeight;
      }
      byte[] videoInfoBytes = MkvUtils.GetVideoInfoBytes(
          (ulong)pixelWidth, (ulong)pixelHeight, (ulong)displayWidth, (ulong)displayHeight);
      byte[] codecPrivateData = base.Attributes.ContainsKey("CodecPrivateData") ?
                                Parse.HexStringAttribute(base.Attributes, "CodecPrivateData") : null;
      if (codecPrivateData == null) {
        throw new Exception("Missing CodecPrivateData attribute!");
      }
      string fourcc = base.Attributes.ContainsKey("FourCC") ? Parse.StringAttribute(base.Attributes, "FourCC") :
                      streamAttributes.ContainsKey("FourCC") ? Parse.StringAttribute(streamAttributes, "FourCC") : null;
      switch (fourcc) {
        case "WVC1": {
          base.TrackEntry = new TrackEntry(
              TrackType.Video, videoInfoBytes, CodecID.V_MS, VideoTrackInfo.GetVfWCodecPrivate(
                  pixelWidth, pixelHeight, fourcc, codecPrivateData));
          break;
        }
        case "H264": {
          ushort nalUnitLengthField = 4;
          if (base.Attributes.ContainsKey("NALUnitLengthField")) {
            nalUnitLengthField = Parse.UInt16Attribute(base.Attributes, "NALUnitLengthField");
          }
          base.TrackEntry = new TrackEntry(
              TrackType.Video, videoInfoBytes, CodecID.V_AVC,
              this.GetAVCCodecPrivate(codecPrivateData, nalUnitLengthField));
          break;
        }
        case null: {
          throw new Exception("Missing FourCC attribute!");
        }
        default: {
          throw new Exception("Unsupported video FourCC: '" + fourcc + "'");
        }
      }
      if (base.Attributes.ContainsKey("Name")) {
        base.TrackEntry.Name = Parse.StringAttribute(streamAttributes, "Name");
      }
      base.TrackEntry.Language = LanguageID.Hungarian;  // TODO: Make this configurable.
      base.Description = string.Format("{0} {1}x{2} ({3}x{4}) @ {5} kbps", new object[] {
          fourcc, pixelWidth, pixelHeight, displayWidth, displayHeight, base.Bitrate / 1000u });
    }
    private static byte[] GetAVCCodecPrivate(byte[] codecPrivateData, ushort nalUnitLengthField) {
      switch (nalUnitLengthField) {
        case 1: case 2:	case 4: {
          string text = Utils.HexEncodeString(codecPrivateData);
          if (string.IsNullOrEmpty(text)) {
            throw new Exception("Invalid AVC1 attribute: CodecPrivateData");
          }
          string[] array = text.Split(new string[] { "00000001" }, 0);
          if (array.Length != 3) {
            throw new Exception("Invalid AVC1 attribute: CodecPrivateData");
          }
          byte[] array2 = Utils.HexDecodeString(array[1]);
          if (array2 == null || array2.Length < 3) {
            throw new Exception("Invalid SPS in CodecPrivateData!");
          }
          byte[] array3 = Utils.HexDecodeString(array[2]);
          if (array3 == null) {
            throw new Exception("Invalid PPS in CodecPrivateData!");
          }
          return GetAVCDecoderConfigurationBytes(
              array2[1], array2[2], array2[3], (byte)(nalUnitLengthField - 1),
              new byte[1][] { array2 }, new byte[1][] { array3 });
        }
      }
      throw new Exception("Invalid AVC1 attribute: NALUnitLengthField");
    }
    private static byte[] GetAVCDecoderConfigurationBytes(
        byte AVCProfileIndication, byte profile_compatibility, byte AVCLevelIndication,
        byte lengthSizeMinusOne, byte[][] sequenceParameterSetNALUnits, byte[][] pictureParameterSetNALUnits) {
      if (lengthSizeMinusOne != 0 && lengthSizeMinusOne != 1 && lengthSizeMinusOne != 3) {
        throw new Exception("Invalid lengthSizeMinusOne value in AVCDecoderConfigurationRecord!");
      }
      if (sequenceParameterSetNALUnits.Length > 31) {
        throw new Exception("Invalid sequenceParameterSetNALUnits count in AVCDecoderConfigurationRecord!");
      }
      if (pictureParameterSetNALUnits.Length > 255) {
        throw new Exception("Invalid pictureParameterSetNALUnits count in AVCDecoderConfigurationRecord!");
      }
      int i = 7;
      int limitS = sequenceParameterSetNALUnits.Length;
      for (int b = 0; b < limitS; ++b) {
        i += 2 + sequenceParameterSetNALUnits[b].Length;
      }
      int limitP = pictureParameterSetNALUnits.Length;
      for (int b = 0; b < limitP; ++b) {
        i += 2 + pictureParameterSetNALUnits[b].Length;
      }
      byte[] array = new byte[i];
      array[0] = 1;  // configurationVersion.
      array[1] = AVCProfileIndication;
      array[2] = profile_compatibility;
      array[3] = AVCLevelIndication;
      array[4] = (byte)(252 ^ lengthSizeMinusOne);
      array[5] = (byte)(224 ^ limitS);
      i = 6;
      for (int b = 0; b < limitS; ++b) {
        int size = sequenceParameterSetNALUnits[b].Length;
        array[i] = (byte)(size >> 8);
        array[i + 1] = (byte)(size & 255);
        i += 2;
        Buffer.BlockCopy(sequenceParameterSetNALUnits[b], 0, array, i, size);
        i += size;
      }
      array[i++] = (byte)limitP;
      for (int b = 0; b < limitP; ++b) {
        int size = pictureParameterSetNALUnits[b].Length;
        array[i] = (byte)(size >> 8);
        array[i + 1] = (byte)(size & 255);
        i += 2;
        Buffer.BlockCopy(pictureParameterSetNALUnits[b], 0, array, i, size);
        i += size;
      }
      return array;
    }
    private static byte[] GetVfWCodecPrivate(uint width, uint height, string fourCC, byte[] codecPrivateData) {
      if (width > 2147483647u) {  // int.MaxValue
        throw new Exception("Invalid video width value!");
      }
      if (height > 2147483647u) {  // int.MaxValue
        throw new Exception("Invalid video height value!");
      }
      if (fourCC.Length != 4) {
        throw new Exception("Invalid video FourCC value!");
      }
      return GetBitmapInfoHeaderBytes(
          (int)width, (int)height, 1, 24,
          /*biCompression:*/BitConverter.ToUInt32(Encoding.ASCII.GetBytes(fourCC), 0),
          width * height * 24u / 8u, 0, 0, 0u, 0u, codecPrivateData);
    }
  }

  internal class Parse {
    public static IDictionary<string, string> Attributes(XmlNode element) {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      foreach (XmlAttribute xmlAttribute in element.Attributes) {
        dictionary.Add(xmlAttribute.Name, xmlAttribute.Value);
      }
      return dictionary;
    }
    public static IDictionary<string, string> CustomAttributes(XmlNode element) {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      XmlNode xmlNode = element.SelectSingleNode("CustomAttributes");
      if (xmlNode != null) {
        foreach (XmlNode xmlNode2 in xmlNode.SelectNodes("Attribute")) {
          dictionary.Add(xmlNode2.Attributes.GetNamedItem("Name").Value, xmlNode2.Attributes.GetNamedItem("Value").Value);
        }
      }
      return dictionary;
    }
    public static string StringAttribute(IDictionary<string, string> attributes, string key) {
      if (!attributes.ContainsKey(key)) {
        throw new Exception(key + " key is missing!");
      }
      return attributes[key];
    }
    public static bool BoolAttribute(IDictionary<string, string> attributes, string key) {
      string text = Parse.StringAttribute(attributes, key);
      bool result;
      if (!bool.TryParse(text, out result)) {
        throw new Exception("Cannot parse the " + key + " key!");
      }
      return result;
    }
    public static ushort UInt16Attribute(IDictionary<string, string> attributes, string key) {
      string text = Parse.StringAttribute(attributes, key);
      ushort result;
      if (!ushort.TryParse(text, out result)) {
        throw new Exception("Cannot parse the " + key + " key!");
      }
      return result;
    }
    public static uint UInt32Attribute(IDictionary<string, string> attributes, string key) {
      string text = Parse.StringAttribute(attributes, key);
      uint result;
      if (!uint.TryParse(text, out result)) {
        throw new Exception("Cannot parse the " + key + " key!");
      }
      return result;
    }
    public static ulong UInt64Attribute(IDictionary<string, string> attributes, string key) {
      string text = Parse.StringAttribute(attributes, key);
      ulong result;
      if (!ulong.TryParse(text, out result)) {
        throw new Exception("Cannot parse the " + key + " key!");
      }
      return result;
    }
    public static byte[] HexStringAttribute(IDictionary<string, string> attributes, string key) {
      return Utils.HexDecodeString(Parse.StringAttribute(attributes, key));
    }
    public static MediaStreamType MediaStreamTypeAttribute(IDictionary<string, string> attributes, string key) {
      switch (Parse.StringAttribute(attributes, key)) {
        case "video": { return MediaStreamType.Video; }
        case "audio": { return MediaStreamType.Audio; }
        case "text": { return MediaStreamType.Script; }
        default: { throw new Exception("Cannot parse the " + key + " key!"); }
      }
    }
  }
}
