using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdReplaceAll : Command {
        public override string Name {
            get { return "ReplaceAll"; }
        }

        public override string Syntax {
            get { return "/replaceall [material] [material]"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Architect; }
        }

        public override void Use( Player p, string args ) {
            byte material1 = 0;
            byte material2 = 0;

            if ( args != "" ) {
                material1 = Block.Byte( args.Split(' ')[0] );
                if ( material1 < 0 || material1 > 49 ) {
                    p.SendMessage( "&cMaterial \"&f" + args + "&c\" was not found." );
                    return;
                }
            }

            if ( args != "" ) {
                material2 = Block.Byte( args.Split( ' ' )[1] );
                if ( material2 < 0 || material2 > 49 ) {
                    p.SendMessage( "&cMaterial \"&f" + args + "&c\" was not found." );
                    return;
                }
            }

            if ( p.Level.BuildPermission > p.Rank.Permission ) {
                p.SendMessage( "&cYou aren't permitted to build here." );
                return;
            }

            List<BufferPos> buffer = new List<BufferPos>();

            for ( ushort xx = 0; xx < p.Level.Width; xx++ ) {
                for ( ushort yy = 0; yy < p.Level.Height; yy++ ) {
                    for ( ushort zz = 0; zz < p.Level.Depth; zz++ ) {
                        byte currBlock = p.Level.GetBlock( xx, yy, zz );
                        if ( currBlock == material1 ) {
                            buffer.Add( new BufferPos( xx, yy, zz, material2 ) );
                        }
                    }
                }
            }

            foreach ( BufferPos bpos in buffer ) {
                p.Level.Blockchange( p, bpos.X, bpos.Y, bpos.Z, bpos.Type );
            }

            p.SendMessage( "&cReplaceAll: &eReplaced " + Block.Name( material1 ) + " with " + Block.Name( material2 ) + ", modified " + buffer.Count + " blocks." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Replaces one material with another in a whole level." );
        }
    }
}
