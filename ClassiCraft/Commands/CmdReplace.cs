using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdReplace : Command {
        public override string Name {
            get { return "Replace"; }
        }

        public override string Syntax {
            get { return "/replace [material] [material]"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Architect; }
        }

        public override void Use( Player p, string args ) {
            if ( args != "" ) {
                material1 = Block.Byte( args.Split(' ')[0] );
                if ( material1 < 0 || material1 > 49 ) {
                    p.SendMessage( "&cMaterial \"&f" + args + "&c\" was not found." );
                    return;
                }
            } else {
                usecurrentmaterial = true;
            }

            if ( args != "" ) {
                material2 = Block.Byte( args.Split( ' ' )[1] );
                if ( material1 < 0 || material1 > 49 ) {
                    p.SendMessage( "&cMaterial \"&f" + args + "&c\" was not found." );
                    return;
                }
            }

            if ( p.Level.BuildPermission > p.Rank.Permission ) {
                p.SendMessage( "&cYou aren't permitted to build here." );
                return;
            }

            p.SendMessage( "&cReplace: &ePlace a block at the first corner." );
            p.OnBlockChange += Blockchange1;
        }

        ushort x1;
        ushort y1;
        ushort z1;
        byte material1;
        byte material2;
        bool usecurrentmaterial = false;

        void Blockchange1( Player p, ushort x, ushort y, ushort z, byte type ) {
            p.ClearBlockChange();
            byte b = p.Level.GetBlock( x, y, z );
            p.SendSetBlock( x, y, z, b );

            x1 = x;
            y1 = y;
            z1 = z;

            p.SendMessage( "&cReplace: &ePlace a block at the second corner." );
            p.OnBlockChange += Blockchange2;
        }

        void Blockchange2( Player p, ushort x2, ushort y2, ushort z2, byte type ) {
            p.ClearBlockChange();
            byte b = p.Level.GetBlock( x2, y2, z2 );
            p.SendSetBlock( x2, y2, z2, b );

            if ( usecurrentmaterial ) {
                material1 = type;
            }

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
                        byte currBlock = p.Level.GetBlock(x, y, z);
                        if ( currBlock == material1 ) {
                            buffer.Add( new BufferPos( x, y, z, material2 ) );
                        }
                    }
                }
            }

            foreach ( BufferPos bpos in buffer ) {
                p.Level.Blockchange( p, bpos.X, bpos.Y, bpos.Z, bpos.Type );
            }

            p.SendMessage( "&cReplace: &eReplaced " + Block.Name(material1) + " with " + Block.Name(material2) + ", modified " + buffer.Count + " blocks. (" + XDiff + "x" + YDiff + "x" + ZDiff + ")" );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Replaces one material with another." );
        }
    }
}
