using System;
using System.Collections.Generic;
using System.Text;

namespace raymarching
{
    public class Chunk
    {
        public Block[] data = new Block[Constants.CHUNK_SIZE_CUBED];
    }
}
