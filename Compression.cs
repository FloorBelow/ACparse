
using System;

namespace ACSharp {
    public static class Compression {
        public static byte[] Decompress(byte[] compressedData, byte compressionType, int decompressedSize) {
            if (compressionType < 3) {
                return LZO.Decompress(compressedData, decompressedSize, compressionType);
            }
            if(compressionType == 8) {
                return Oodle.Decompress(compressedData, decompressedSize);
            }
            Console.WriteLine($"Unknown compression type {compressionType}");
            return null;
        }
    }
}