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
            get { return PermissionLevel.Builder; }
        }

        public override void Use( Player p, string args ) {
            int zoneCount = 0;

            ZoneDB.ZoneList.ForEach( delegate( Zone z ) {
                if ( z.Level == p.Level ) {
                    List<BufferPos> buffer = new List<BufferPos>();

                    ushort smallx = Math.Min(z.x1, z.x2);
                    ushort bigx = Math.Max( z.x1, z.x2 );
                    ushort smally = Math.Min( z.y1, z.y2 );
                    ushort bigy = Math.Max( z.y1, z.y2 );
                    ushort smallz = Math.Min( z.z1, z.z2 );
                    ushort bigz = Math.Max( z.z1, z.z2 );

                    for ( ushort xx = smallx; xx <= bigx; xx++ ) {
                        for ( ushort yy = smally; yy <= bigy; yy++ ) {
                            for ( ushort zz = smallz; zz <= bigz; zz++ ) {
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
