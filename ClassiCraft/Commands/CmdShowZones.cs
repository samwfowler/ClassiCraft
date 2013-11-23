using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdShowZones : Command {
        public override string Name {
            get { return "ShowZones"; }
        }

        public override string Syntax {
            get { return "/showzones"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Member; }
        }

        public override void Use( Player p, string args ) {
            int zoneCount = 0;

            ZoneDB.ZoneList.ForEach( delegate( Zone z ) {
                if ( z.Level == p.Level ) {
                    List<BufferPos> buffer = new List<BufferPos>();

                    for ( ushort xx = z.x1; xx <= z.x2; xx++ ) {
                        for ( ushort yy = z.y1; yy <= z.y2; yy++ ) {
                            for ( ushort zz = z.z1; zz <= z.z2; zz++ ) {
                                buffer.Add( new BufferPos( xx, yy, zz, Block.Green ) );
                            }
                        }
                    }

                    foreach ( BufferPos bp in buffer ) {
                        p.SendSetBlock( bp.X, bp.Y, bp.Z, bp.Type );
                    }

                    zoneCount++;
                }
            } );

            if ( zoneCount > 0 ) {
                p.SendMessage( "&cShowZones: &eRevealed all zones in level." );
            } else {
                p.SendMessage( "&cShowZones: &eNo zones to reveal." );
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "Reveals all zones." );
        }
    }
}
