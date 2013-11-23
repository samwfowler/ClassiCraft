using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public struct BufferPos {
        public ushort X;
        public ushort Y;
        public ushort Z;
        public byte Type;

        public BufferPos( ushort x, ushort y, ushort z, byte type ) {
            X = x;
            Y = y;
            Z = z;
            Type = type;
        }
    }
}
