using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public struct BufferCopy {
        public ushort x1, x2, y1, y2, z1, z2;
        public List<BufferPos> copyBuffer;

        public BufferCopy( ushort x, ushort xx, ushort y, ushort yy, ushort z, ushort zz, List<BufferPos> cb ) {
            x1 = x;
            x2 = xx;
            y1 = y;
            y2 = yy;
            z1 = z;
            z2 = zz;
            copyBuffer = cb;
        }
    }
}
