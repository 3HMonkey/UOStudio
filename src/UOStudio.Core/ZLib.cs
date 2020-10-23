using System;
using System.Runtime.InteropServices;
using ClassicUO.Utility;

namespace UOStudio.Core
{
    public static class ZLib
    {
        private static readonly ICompressor _compressor;

        static ZLib()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (Environment.Is64BitProcess)
                {
                    _compressor = new CompressorUnix64();
                }
                else
                {
                    _compressor = new ManagedUniversal();
                }
            }
            else
            {
                _compressor = new Compressor64();
            }
        }


        public static void Decompress(byte[] source, int offset, byte[] dest, int length)
        {
            _compressor.Decompress(dest, ref length, source, source.Length - offset);
        }

        public static void Decompress(IntPtr source, int sourceLength, int offset, IntPtr dest, int length)
        {
            _compressor.Decompress(dest, ref length, source, sourceLength - offset);
        }

        private enum ZLibQuality
        {
            Default = -1,

            None = 0,

            Speed = 1,
            Size = 9
        }

        private enum ZLibError
        {
            VersionError = -6,
            BufferError = -5,
            MemoryError = -4,
            DataError = -3,
            StreamError = -2,
            FileError = -1,

            Okay = 0,

            StreamEnd = 1,
            NeedDictionary = 2
        }


        private interface ICompressor
        {
            string Version { get; }

            ZLibError Compress(byte[] dest, ref int destLength, byte[] source, int sourceLength);
            ZLibError Compress(byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibQuality quality);

            ZLibError Decompress(byte[] dest, ref int destLength, byte[] source, int sourceLength);
            ZLibError Decompress(IntPtr dest, ref int destLength, IntPtr source, int sourceLength);
        }

        private sealed class Compressor64 : ICompressor
        {
            public string Version => SafeNativeMethods.zlibVersion();

            public ZLibError Compress(byte[] dest, ref int destLength, byte[] source, int sourceLength) => SafeNativeMethods.compress(dest, ref destLength, source, sourceLength);

            public ZLibError Compress
                (byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibQuality quality) =>
                SafeNativeMethods.compress2(dest, ref destLength, source, sourceLength, quality);

            public ZLibError Decompress(byte[] dest, ref int destLength, byte[] source, int sourceLength) => SafeNativeMethods.uncompress(dest, ref destLength, source, sourceLength);

            public ZLibError Decompress(IntPtr dest, ref int destLength, IntPtr source, int sourceLength) => SafeNativeMethods.uncompress(dest, ref destLength, source, sourceLength);

            private class SafeNativeMethods
            {
                [DllImport("zlib")]
                internal static extern string zlibVersion();

                [DllImport("zlib")]
                internal static extern ZLibError compress
                    (byte[] dest, ref int destLength, byte[] source, int sourceLength);

                [DllImport("zlib")]
                internal static extern ZLibError compress2
                    (byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibQuality quality);

                [DllImport("zlib")]
                internal static extern ZLibError uncompress(byte[] dest, ref int destLen, byte[] source, int sourceLen);

                [DllImport("zlib")]
                internal static extern ZLibError uncompress(IntPtr dest, ref int destLen, IntPtr source, int sourceLen);
            }
        }

        private sealed class CompressorUnix64 : ICompressor
        {
            public string Version => SafeNativeMethods.zlibVersion();

            public ZLibError Compress(byte[] dest, ref int destLength, byte[] source, int sourceLength)
            {
                long destLengthLong = destLength;
                ZLibError z = SafeNativeMethods.compress(dest, ref destLengthLong, source, sourceLength);
                destLength = (int) destLengthLong;

                return z;
            }

            public ZLibError Compress
                (byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibQuality quality)
            {
                long destLengthLong = destLength;
                ZLibError z = SafeNativeMethods.compress2(dest, ref destLengthLong, source, sourceLength, quality);
                destLength = (int) destLengthLong;

                return z;
            }

            public ZLibError Decompress(byte[] dest, ref int destLength, byte[] source, int sourceLength)
            {
                long destLengthLong = destLength;
                ZLibError z = SafeNativeMethods.uncompress(dest, ref destLengthLong, source, sourceLength);
                destLength = (int) destLengthLong;

                return z;
            }

            public ZLibError Decompress(IntPtr dest, ref int destLength, IntPtr source, int sourceLength)
            {
                return SafeNativeMethods.uncompress(dest, ref destLength, source, sourceLength);
            }

            private class SafeNativeMethods
            {
                [DllImport("libz")]
                internal static extern string zlibVersion();

                [DllImport("libz")]
                internal static extern ZLibError compress
                    (byte[] dest, ref long destLength, byte[] source, long sourceLength);

                [DllImport("libz")]
                internal static extern ZLibError compress2
                    (byte[] dest, ref long destLength, byte[] source, long sourceLength, ZLibQuality quality);

                [DllImport("libz")]
                internal static extern ZLibError uncompress
                    (byte[] dest, ref long destLen, byte[] source, long sourceLen);

                [DllImport("libz")]
                internal static extern ZLibError uncompress(IntPtr dest, ref int destLen, IntPtr source, int sourceLen);
            }
        }

        private sealed class ManagedUniversal : ICompressor
        {
            public string Version => "1.2.11";

            public ZLibError Compress(byte[] dest, ref int destLength, byte[] source, int sourceLength)
            {
                ZLibManaged.Compress(dest, ref destLength, source);

                return ZLibError.Okay;
            }

            public ZLibError Compress
                (byte[] dest, ref int destLength, byte[] source, int sourceLength, ZLibQuality quality)
            {
                return Compress(dest, ref destLength, source, sourceLength);
            }

            public ZLibError Decompress(byte[] dest, ref int destLength, byte[] source, int sourceLength)
            {
                ZLibManaged.Decompress(source, 0, sourceLength, 0, dest, destLength);

                return ZLibError.Okay;
            }

            public ZLibError Decompress(IntPtr dest, ref int destLength, IntPtr source, int sourceLength)
            {
                ZLibManaged.Decompress(source, sourceLength, 0, dest, destLength);

                return ZLibError.Okay;
            }
        }
    }
}
