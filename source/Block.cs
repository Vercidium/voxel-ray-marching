using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace raymarching
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Block
    {
        public byte kind;
    }
}
