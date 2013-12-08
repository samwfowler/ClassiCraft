using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdFixGrass : Command {
        public override string Name {
            get { return "FixGrass"; }
        }

        public override string Syntax {
            get { return "/fixgrass"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            for ( ushort xx = 0; xx < p.Level.Width; xx++ ) {
                for ( ushort zz = 0; zz < p.Level.Depth; zz++ ) {
                    BufferPos bpos = new BufferPos();

                    for ( ushort yy = 0; yy < p.Level.Height; yy++ ) {
                        if ( p.Level.GetBlock( xx, yy, zz ) != Block.Air ) {
                            bpos = new BufferPos( xx, yy, zz, p.Level.GetBlock( xx, yy, zz ) );
                        }
                    }

                    if ( bpos.Type == Block.Dirt ) {
                        p.Level.Blockchange( bpos.X, bpos.Y, bpos.Z, Block.Grass );
                    }
                }
            }

            p.SendMessage( "&cFixGrass: &eFixed grass." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Turns all soil exposed to light into grass." );
        }

    }
}
