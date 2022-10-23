
using System;

namespace ACSharp {
    public static class Compression {
        public static byte[] Decompress(byte[] compressedData, byte compressionType, ushort decompressedSize) {
            if (compressionType < 3) {
                return LZO.Decompress(compressedData, decompressedSize, compressionType);
            }
            Console.WriteLine($"Unknown compression type {compressionType}");
            return null;
        }
    }
}