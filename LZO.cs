using System.Runtime.InteropServices;

namespace ACSharp {
	static class LZO {
#if WIN32
        [DllImport("Assets\\ACSharp\\lzo.dll", CharSet = CharSet.None, CallingConvention = CallingConvention.Cdecl)]
        private static extern int __lzo_init_v2_32(uint v, int s1, int s2, int s3, int s4, int s5, int s6, int s7, int s8, int s9);

        [DllImport("Assets\\ACSharp\\lzo.dll", CharSet = CharSet.None, CallingConvention = CallingConvention.Cdecl)]
        private static extern int lzo1c_decompress_safe(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);
#else
        [DllImport("lzo2_64.dll")]
        private static extern int __lzo_init_v2(uint v, int s1, int s2, int s3, int s4, int s5, int s6, int s7, int s8, int s9);

        [DllImport("lzo2_64.dll")]
        private static extern int lzo1x_decompress(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);

        [DllImport("lzo2_64.dll")]
        private static extern int lzo2a_decompress(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);
#endif

        private static byte[] workMem = new byte[16384L * 8];

        public enum Algorithm {
            LZO1X = 0,
            LZO1X_ = 1, // 0 and 1 refer to the same algorithm
            LZO2A = 2,
            LZO1C = 5
        }

        public static byte[] Decompress(byte[] input, int decompressedSize, int compressionType) {
#if WIN32
            if (__lzo_init_v2_32(1, -1, -1, -1, -1, -1, -1, -1, -1, -1) != 0)
#else
            if (__lzo_init_v2(1, -1, -1, -1, -1, -1, -1, -1, -1, -1) != 0)
#endif
                return new byte[] { };

            byte[] output = new byte[decompressedSize];
            int outputSize = decompressedSize;
            if(compressionType == 2) lzo2a_decompress(input, input.Length, output, ref outputSize, workMem);
            else lzo1x_decompress(input, input.Length, output, ref outputSize, workMem);
            return output;
        }
    }
}
