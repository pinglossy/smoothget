using System;
using System.IO;
using System.Collections.Generic;
namespace Smoothget.Mp4 {
  // The 4 bytes (MSB first) of these values correspond to the 4 bytes of the name (e.g. 'f', 't', 'y', p' for ftyp).
  public enum ID : uint {
    uuid = 1970628964u,
    sdtp = 1935963248u,
    moof = 1836019558u,
    mfhd = 1835427940u,
    traf = 1953653094u,
    tfhd = 1952868452u,
    trun = 1953658222u,
    mdat = 1835295092u,
    unsupported = 0u,
    tfrf = 1u,
    tfxd = 2u,
    min = 100u,
  }

  public class Fragment {
    public MovieFragmentBox moof;
    public MediaDataBox mdat;
    public Fragment(byte[] boxBytes, int start, int end) {
      while (start < end) {
        Box box = Mp4Utils.GetBox(boxBytes, ref start, end);
        if (box == null) {
        } else if (box.ID == ID.mdat) {
          this.mdat = (box as MediaDataBox);
        } else if (box.ID == ID.moof) {
          this.moof = (box as MovieFragmentBox);
        }
      }
    }
  }

  public class Box {
    public ID ID;
    public Box(ID id) {
      this.ID = id;
    }
  }

  public class MediaDataBox : Box {
    public int Start;
    public MediaDataBox(byte[] boxBytes, int start, int end) : base(ID.mdat) {
      this.Start = start;
    }
  }

  public class MovieFragmentHeaderBox : Box {
    public uint SequenceNumber;
    public MovieFragmentHeaderBox(byte[] boxBytes, int start, int end) : base(ID.mfhd) {
      if (end - start != 8) {
        throw new Exception("Invalid '" + base.ID + "' length!");
      }
      start += 4;
      this.SequenceNumber = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
    }
  }

  public class MovieFragmentBox : Box {
    public MovieFragmentHeaderBox mfhd;
    public TrackFragmentBox traf;
    public MovieFragmentBox(byte[] boxBytes, int start, int end) : base(ID.moof) {
      while (start < end) {
        Box box = Mp4Utils.GetBox(boxBytes, ref start, end);
        if (box == null) {
        } else if (box.ID == ID.traf) {
          this.traf = (box as TrackFragmentBox);
        } else if (box.ID == ID.mfhd) {
          this.mfhd = (box as MovieFragmentHeaderBox);
        }
      }
    }
  }

  public class TrackFragmentBox : Box {
    public TrackFragmentHeaderBox tfhd;
    public TrackRunBox trun;
    public SampleDependencyTypeBox sdtp;
    public TfrfBox tfrf;
    public TfxdBox tfxd;
    public TrackFragmentBox(byte[] boxBytes, int start, int end) : base(ID.traf) {
      while (start < end) {
        Box box = Mp4Utils.GetBox(boxBytes, ref start, end);
        if (box != null) {
          ID iD = box.ID;
          if (iD == ID.tfhd) {
            this.tfhd = (box as TrackFragmentHeaderBox);
          } else if (iD == ID.sdtp) {
            this.sdtp = (box as SampleDependencyTypeBox);
          } else if (iD == ID.trun) {
            this.trun = (box as TrackRunBox);
          } else if (iD == ID.tfrf) {
            this.tfrf = (box as TfrfBox);
          } else if (iD == ID.tfxd) {
            this.tfxd = (box as TfxdBox);
          }
        }
      }
    }
  }

  public class TfxdBox : Box {
    public byte Version;
    public byte[] Flags;
    public ulong FragmentAbsoluteTime;
    public ulong FragmentDuration;
    public TfxdBox(byte[] boxBytes, int start, int end) : base(ID.tfxd) {
      // TODO: Don't read and populate unused fields.
      this.Version = Mp4Utils.ReadReverseBytes(boxBytes, 1, ref start, end)[0];
      this.Flags = Mp4Utils.ReadReverseBytes(boxBytes, 3, ref start, end);
      if (this.Version == 0) {
        this.FragmentAbsoluteTime = (ulong)BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(
            boxBytes, 4, ref start, end), 0);
        this.FragmentDuration = (ulong)BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      } else {
        if (this.Version != 1) {
          throw new Exception("Invalid TfxdBox version '" + this.Version + "'!");
        }
        this.FragmentAbsoluteTime = BitConverter.ToUInt64(Mp4Utils.ReadReverseBytes(boxBytes, 8, ref start, end), 0);
        this.FragmentDuration = BitConverter.ToUInt64(Mp4Utils.ReadReverseBytes(boxBytes, 8, ref start, end), 0);
      }
    }
  }

  public class SampleDependencyTypeBox : Box {
    public class Element {
      public byte reserved;
      public byte sample_depends_on;
      public byte sample_is_depended_on;
      public byte sample_has_redundancy;
      public Element(byte reserved, byte sample_depends_on, byte sample_is_depended_on, byte sample_has_redundancy) {
        this.reserved = reserved;
        this.sample_depends_on = sample_depends_on;
        this.sample_is_depended_on = sample_is_depended_on;
        this.sample_has_redundancy = sample_has_redundancy;
      }
    }
    public uint version;
    public SampleDependencyTypeBox.Element[] array;
    public SampleDependencyTypeBox(byte[] boxBytes, int start, int end) : base(ID.sdtp) {
      this.version = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      int count = end - start;
      this.array = new SampleDependencyTypeBox.Element[count];
      for (int i = 0; i < count; i++) {
        byte b = Mp4Utils.ReadReverseBytes(boxBytes, 1, ref start, end)[0];
        byte reserved = (byte)(b >> 6);
        b <<= 2;
        byte sample_depends_on = (byte)(b >> 6);
        b <<= 2;
        byte sample_is_depended_on = (byte)(b >> 6);
        b <<= 2;
        byte sample_has_redundancy = (byte)(b >> 6);
        this.array[i] = new SampleDependencyTypeBox.Element(
            reserved, sample_depends_on, sample_is_depended_on, sample_has_redundancy);
      }
    }
  }

  public class TfrfBox : Box {
    public class Element {
      public ulong FragmentAbsoluteTime;
      public ulong FragmentDuration;
      public Element(byte[] boxBytes, byte version, ref int start, int end) {
        if (version == 0) {
          this.FragmentAbsoluteTime = (ulong)BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
          this.FragmentDuration = (ulong)BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
        } else {
          if (version != 1) {
            throw new Exception("Invalid TfrfBox version '" + version + "'!");
          }
          this.FragmentAbsoluteTime = BitConverter.ToUInt64(Mp4Utils.ReadReverseBytes(boxBytes, 8, ref start, end), 0);
          this.FragmentDuration = BitConverter.ToUInt64(Mp4Utils.ReadReverseBytes(boxBytes, 8, ref start, end), 0);
        }
      }
    }
    public byte Version;
    public byte[] Flags;
    public Element[] Array;
    public TfrfBox(byte[] boxBytes, int start, int end) : base(ID.tfrf) {
      this.Version = Mp4Utils.ReadReverseBytes(boxBytes, 1, ref start, end)[0];
      this.Flags = Mp4Utils.ReadReverseBytes(boxBytes, 3, ref start, end);
      int fragmentCount = (int)Mp4Utils.ReadReverseBytes(boxBytes, 1, ref start, end)[0];
      this.Array = new TfrfBox.Element[fragmentCount];
      for (int i = 0; i < fragmentCount; i++) {
        this.Array[i] = new TfrfBox.Element(boxBytes, this.Version, ref start, end);
      }
      // TODO: Do we want to test start == end (in other classes as well?).
    }
  }

  public class TrackFragmentHeaderBox : Box {
    public uint tf_flags;
    public uint track_ID;
    public ulong base_data_offset;
    public uint sample_description_index;
    public uint default_sample_duration;
    public uint default_sample_size;
    public uint default_sample_flags;
    public bool base_data_offset_present;
    public bool sample_description_index_present;
    public bool default_sample_duration_present;
    public bool default_sample_size_present;
    public bool default_sample_flags_present;
    public bool duration_is_empty;
    public TrackFragmentHeaderBox(byte[] boxBytes, int start, int end) : base(ID.tfhd) {
      // TODO: Don't store field this.tr_flags etc. if not used.
      this.tf_flags = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      this.base_data_offset_present = ((1u & this.tf_flags) != 0u);
      this.sample_description_index_present = ((2u & this.tf_flags) != 0u);
      this.default_sample_duration_present = ((8u & this.tf_flags) != 0u);
      this.default_sample_size_present = ((16u & this.tf_flags) != 0u);
      this.default_sample_flags_present = ((32u & this.tf_flags) != 0u);
      this.duration_is_empty = ((65536u & this.tf_flags) != 0u);
      this.track_ID = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      if (this.base_data_offset_present) {
        this.base_data_offset = BitConverter.ToUInt64(Mp4Utils.ReadReverseBytes(boxBytes, 8, ref start, end), 0);
      }
      if (this.sample_description_index_present) {
        this.sample_description_index = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(
            boxBytes, 4, ref start, end), 0);
      }
      if (this.default_sample_duration_present) {
        this.default_sample_duration = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(
            boxBytes, 4, ref start, end), 0);
      }
      if (this.default_sample_size_present) {
        this.default_sample_size = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      }
      if (this.default_sample_flags_present) {
        this.default_sample_flags = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      }
    }
  }

  public class TrackRunBox : Box {
    public struct Element {
      public uint sample_duration;
      public uint sample_size;
      public uint sample_flags;
      public uint sample_composition_time_offset;
      public Element(uint sample_duration, uint sample_size, uint sample_flags, uint sample_composition_time_offset) {
        this.sample_duration = sample_duration;
        this.sample_size = sample_size;
        this.sample_flags = sample_flags;
        this.sample_composition_time_offset = sample_composition_time_offset;
      }
    }
    public uint tr_flags;
    public uint sample_count;
    public int data_offset;
    public uint first_sample_flags;
    public TrackRunBox.Element[] array;
    public bool data_offset_present;
    public bool first_sample_flags_present;
    public bool sample_duration_present;
    public bool sample_size_present;
    public bool sample_flags_present;
    public bool sample_composition_time_offsets_present;
    public TrackRunBox(byte[] boxBytes, int start, int end) : base(ID.trun) {
      // TODO: Don't store field this.tr_flags etc. if not used.
      this.tr_flags = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      this.data_offset_present = ((1u & this.tr_flags) != 0u);
      this.first_sample_flags_present = ((4u & this.tr_flags) != 0u);
      this.sample_duration_present = ((256u & this.tr_flags) != 0u);
      this.sample_size_present = ((512u & this.tr_flags) != 0u);
      this.sample_flags_present = ((1024u & this.tr_flags) != 0u);
      this.sample_composition_time_offsets_present = ((2048u & this.tr_flags) != 0u);
      this.sample_count = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      if (this.data_offset_present) {
        this.data_offset = BitConverter.ToInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      }
      if (this.first_sample_flags_present) {
        this.first_sample_flags = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      }
      this.array = new TrackRunBox.Element[this.sample_count];
      for (int i = 0; i < this.array.Length; i++) {
        uint sample_duration = 0u;
        uint sample_size = 0u;
        uint sample_flags = 0u;
        uint sample_composition_time_offset = 0u;
        if (this.sample_duration_present) {
          sample_duration = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
        }
        if (this.sample_size_present) {
          sample_size = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
        }
        if (this.sample_flags_present) {
          sample_flags = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
        }
        if (this.sample_composition_time_offsets_present) {
          sample_composition_time_offset = BitConverter.ToUInt32(Mp4Utils.ReadReverseBytes(boxBytes, 4, ref start, end), 0);
        }
        this.array[i] = new TrackRunBox.Element(sample_duration, sample_size, sample_flags, sample_composition_time_offset);
      }
    }
  }

  internal class Mp4Utils {
    public static byte[] ReadReverseBytes(byte[] boxBytes, int count, ref int start, int end) {
      if (start + count > end) {
        throw new Exception("Short read, wanted " + count);
      }
      byte[] array = new byte[count];
      Buffer.BlockCopy(boxBytes, start, array, 0, count);
      start += count;
      Array.Reverse(array);
      return array;
    }
    public static Box GetBox(byte[] boxBytes, ref int start, int end) {
      // TODO: Integrate ReadReverseBytes and BitConverter.ToUint*.
      int length = BitConverter.ToInt32(ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      uint idNum = BitConverter.ToUInt32(ReadReverseBytes(boxBytes, 4, ref start, end), 0);
      if (length == 1) {
        // TODO: Test this.
        length = (int)BitConverter.ToUInt64(ReadReverseBytes(boxBytes, 8, ref start, end), 0) - 16;
      } else if (length == 0) {
        length = end - start;  // TODO: `offset' seems to be correct. Test this.
      } else {
        length -= 8;
      }
      if (length < 0) {
        throw new Exception("Length too small.");
      }
      int contentStart = start;
      start += length;
      if (start > end) {
        throw new Exception("Box '" + idNum + "' ends outside the file!");
      }
      switch (idNum) {
        case (uint)ID.mdat: { return new MediaDataBox(boxBytes, contentStart, start); }
        case (uint)ID.mfhd: { return new MovieFragmentHeaderBox(boxBytes, contentStart, start); }
        case (uint)ID.moof: { return new MovieFragmentBox(boxBytes, contentStart, start); }
        case (uint)ID.sdtp: { return new SampleDependencyTypeBox(boxBytes, contentStart, start); }
        case (uint)ID.tfhd: { return new TrackFragmentHeaderBox(boxBytes, contentStart, start); }
        case (uint)ID.traf: { return new TrackFragmentBox(boxBytes, contentStart, start); }
        case (uint)ID.trun: { return new TrackRunBox(boxBytes, contentStart, start); }
        case (uint)ID.uuid: {
          Guid uUID = new Guid(ReadReverseBytes(boxBytes, 8, ref contentStart, start));
          if (uUID == TfxdGuid) {
            return new TfxdBox(boxBytes, contentStart, start);
          } else if (uUID == TfrfGuid) {
            return new TfrfBox(boxBytes, contentStart, start);
          } else if (uUID == PiffGuid) {
            throw new Exception("DRM protected data!");
          }
          break;
        }
      }
      return null;
    }
    private static readonly Guid TfxdGuid = new Guid("6D1D9B05-42D5-44E6-80E2-141DAFF757B2");
    private static readonly Guid TfrfGuid = new Guid("D4807EF2-CA39-4695-8E54-26CB9E46A79F");
    // mp4parser.boxes.piff.PiffSampleEncryptionBox
    private static readonly Guid PiffGuid = new Guid("A2394F52-5A9B-4F14-A244-6C427C648DF4");
  }
}
