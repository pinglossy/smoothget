using System;
using System.Collections.Generic;
using System.Globalization;
namespace Smoothget {
  public class Utils {
    public static byte[] InplaceReverseBytes(byte[] a) {
      Array.Reverse(a);
      return a;
    }
    public static byte[] CombineByteArrays(IList<byte[]> arrays) {
      int totalSize = 0;
      for (int i = 0; i < arrays.Count; ++i) {
        totalSize += arrays[i].Length;
      }
      byte[] array = new byte[totalSize];
      int pos = 0;
      for (int i = 0; i < arrays.Count; ++i) {
        Buffer.BlockCopy(arrays[i], 0, array, pos, arrays[i].Length);
        pos += arrays[i].Length;
      }
      return array;
    }
    public static byte[] CombineByteArraysAndArraySegments(IList<byte[]> arrays, IList<ArraySegment<byte>> arraySegments) {
      int totalSize = 0;
      if (arrays != null) {
        for (int i = 0; i < arrays.Count; ++i) {
          totalSize += arrays[i].Length;
        }
      }
      for (int i = 0; i < arraySegments.Count; ++i) {
        totalSize += arraySegments[i].Count;
      }
      byte[] array = new byte[totalSize];
      int pos = 0;
      if (arrays != null) {
        for (int i = 0; i < arrays.Count; ++i) {
          Buffer.BlockCopy(arrays[i], 0, array, pos, arrays[i].Length);
          pos += arrays[i].Length;
        }
      }
      for (int i = 0; i < arraySegments.Count; ++i) {
        Buffer.BlockCopy(arraySegments[i].Array, arraySegments[i].Offset, array, pos, arraySegments[i].Count);
        pos += arraySegments[i].Count;
      }
      return array;
    }
    public static byte[] CombineBytes(byte[] a, byte[] b) {
      byte[] array = new byte[a.Length + b.Length];
      Buffer.BlockCopy(a, 0, array, 0, a.Length);
      Buffer.BlockCopy(b, 0, array, a.Length, b.Length);
      return array;
    }
    public static byte[] CombineBytes(byte[] a, byte[] b, byte[] c) {
      byte[] array = new byte[a.Length + b.Length + c.Length];
      Buffer.BlockCopy(a, 0, array, 0, a.Length);
      Buffer.BlockCopy(b, 0, array, a.Length, b.Length);
      Buffer.BlockCopy(c, 0, array, a.Length + b.Length, c.Length);
      return array;
    }
    public static string HexEncodeString(byte[] bytes) {
      // This is simple and fast, but the complicated implementation below is faster,
      // http://stackoverflow.com/questions/623104/c-sharp-byte-to-hex-string
      // return BitConverter.ToString(input).Replace("-", "");
      char[] c = new char[bytes.Length << 1];
      byte b;
      for(int bx = 0, cx = 0; bx < bytes.Length; ++bx)  {
        b = ((byte)(bytes[bx] >> 4));
        c[cx++] = (char)(b > 9 ? b + 0x57 : b + 0x30);
        b = ((byte)(bytes[bx] & 0x0F));
        c[cx++]=(char)(b > 9 ? b + 0x57 : b + 0x30);
      }
      return new string(c);
    }
    public static byte[] HexDecodeString(string hexEncodedData) {
      // This is a fast implementation from http://stackoverflow.com/questions/623104/c-sharp-byte-to-hex-string
      if (hexEncodedData == null) return null;
      byte[] buffer = new byte[hexEncodedData.Length >> 1];
      char c;
      for (int bx = 0, sx = 0; bx < buffer.Length; ++bx) {
        c = hexEncodedData[sx++];
        if (((uint)c - (uint)'0') > 9 && ((uint)c - (uint)'A') > 5 && ((uint)c - (uint)'a') > 5) return null;
        buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);
        c = hexEncodedData[sx++];
        if (((uint)c - (uint)'0') > 9 && ((uint)c - (uint)'A') > 5 && ((uint)c - (uint)'a') > 5) return null;
        buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
      }
      return buffer;
    }
    // TODO: Save code size by using Encoding.ASCII and HexEncodeString.
    public static byte[] HexDecodeBytes(byte[] hexEncodedData, int start, int end) {
      // This is a fast implementation based on http://stackoverflow.com/questions/623104/c-sharp-byte-to-hex-string
      if (hexEncodedData == null) return null;
      if (start < 0) start = 0;
      if (end >= hexEncodedData.Length) end = hexEncodedData.Length;
      if (start >= end) return new byte[] {};
      if (((end - start) & 1) != 0) return null;
      byte[] buffer = new byte[(end - start) >> 1];
      byte c;
      for (int bx = 0, sx = start; bx < buffer.Length; ++bx) {
        c = hexEncodedData[sx++];
        if (((uint)c - (uint)'0') > 9 && ((uint)c - (uint)'A') > 5 && ((uint)c - (uint)'a') > 5) return null;
        buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);
        c = hexEncodedData[sx++];
        if (((uint)c - (uint)'0') > 9 && ((uint)c - (uint)'A') > 5 && ((uint)c - (uint)'a') > 5) return null;
        buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
      }
      return buffer;
    }
    public static string EscapeString(string s) {
      // This is simpler and less bloated than the CSharpCodeProvider in
      // http://stackoverflow.com/questions/323640/can-i-convert-a-c-sharp-string-value-to-an-escaped-string-literal
      // TODO: Save memory by doing less concatenations.
      return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
    // Always makes a copy.
    public static byte[] GetSubBytes(byte[] bytes, int start, int end) {
      if (bytes == null) return null;
      if (start < 0) start = 0;
      if (end >= bytes.Length) end = bytes.Length;
      if (start >= end) return new byte[] {};
      byte[] subBytes = new byte[end - start];
      Buffer.BlockCopy(bytes, start, subBytes, 0, end - start);
      return subBytes;
    }
    public static bool ArePrefixBytesEqual(byte[] a, byte[] b, int size) {
      for (int i = 0; i < size; ++i) {
        if (a[i] != b[i]) return false;
      }
      return true;
    }
  }
}
