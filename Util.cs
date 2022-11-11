using System.IO;

namespace ACSharp {
    static class Util {
        public static void Seek(this BinaryReader r, long offset) { r.BaseStream.Seek(offset, SeekOrigin.Current); }
    }
}
