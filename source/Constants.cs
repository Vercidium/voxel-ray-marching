using System;
using System.Collections.Generic;
using System.Text;

namespace raymarching
{
    public static class Constants
    {
        public const int MAP_SIZE_X = 512;
        public const int MAP_SIZE_Y = 160;
        public const int MAP_SIZE_Z = 512;

        public const int CHUNK_SIZE = 32;
        public const int CHUNK_SIZE_SQUARED = 1024;
        public const int CHUNK_SIZE_CUBED = 32768;

        public const int CHUNK_AMOUNT_X = 16;
        public const int CHUNK_AMOUNT_Y = 5;
        public const int CHUNK_AMOUNT_Z = 16;

        public const int MASK = 0x1f;
        public const byte SHIFT = 5;
    }

    public enum Axis
    {
        None = 0,
        X,
        Y,
        Z,
    }
}
