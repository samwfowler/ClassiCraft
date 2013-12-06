using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdCopy : Command {
        public override string Name {
            get { return "Copy"; }
        }

        public override string Syntax {
            get { return "/copy"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Builder; }
        }

        public override void Use( Player p, string args ) {
            if ( p.Level.BuildPermission > p.Rank.Permission ) {
                p.SendMessage( "&cYou aren't permitted to build here." );
                return;
            }

            p.SendMessage( "&cCopy: &ePlace a block at the first corner." );
            p.OnBlockChange += Blockchange1;
        }

        ushort x1;
        ushort y1;
        ushort z1;

        void Blockchange1( Player p, ushort x, ushort y, ushort z, byte type ) {
            p.ClearBlockChange();
            byte b = p.Level.GetBlock( x, y, z );
            p.SendSetBlock( x, y, z, b );

            x1 = x;
            y1 = y;
            z1 = z;

            p.SendMessage( "&cCopy: &ePlace a block at the second corner." );
            p.OnBlockChange += Blockchange2;
        }

        void Blockchange2( Player p, ushort x2, ushort y2, ushort z2, byte type ) {
            p.ClearBlockChange();
            byte b = p.Level.GetBlock( x2, y2, z2 );
            p.SendSetBlock( x2, y2, z2, b );

            ushort MinX = Math.Min( x1, x2 );
            ushort MaxX = Math.Max( x1, x2 );
            ushort XDiff = (ushort)(MaxX - MinX + 1);
            ushort MinY = Math.Min( y1, y2 );
            ushort MaxY = Math.Max( y1, y2 );
            ushort YDiff = (ushort)(MaxY - MinY + 1);
            ushort MinZ = Math.Min( z1, z2 );
            ushort MaxZ = Math.Max( z1, z2 );
            ushort ZDiff = (ushort)(MaxZ - MinZ + 1);

            List<BufferPos> buffer = new List<BufferPos>();

            for ( ushort x = MinX; x <= MaxX; x++ ) {
                for ( ushort y = MinY; y <= MaxY; y++ ) {
                    for ( ushort z = MinZ; z <= MaxZ; z++ ) {
                        byte currBlock = p.Level.GetBlock( x, y, z );
                        buffer.Add( new BufferPos( (ushort)(x - x1), (ushort)(y - y1), (ushort)(z - z1), currBlock ) );
                    }
                }
            }

            BufferCopy bcopy = new BufferCopy( MinX, MaxX, MinY, MaxY, MinZ, MaxZ, buffer );
            p.BufferCopy = bcopy;
            p.SendMessage( "&cCopy: &eCopied " + buffer.Count + " blocks. (" + XDiff + "x" + YDiff + "x" + ZDiff + ")" );
            p.SendMessage( "&cCopy: &eUse /paste to paste the blocks." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Copies a box of blocks." );
        }
    }
}
