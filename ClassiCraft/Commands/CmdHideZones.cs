using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdHideZones : Command {
        public override string Name {
            get { return "HideZones"; }
        }

        public override string Syntax {
            get { return "/hidezones"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Member; }
        }

        public override void Use( Player p, string args ) {
            ZoneDB.ZoneList.ForEach( delegate( Zone z ) {
                if ( z.Level == p.Level ) {
                    List<BufferPos> buffer = new List<BufferPos>();

                    ushort smallx = Math.Min( z.x1, z.x2 );
                    ushort bigx = Math.Max( z.x1, z.x2 );
                    ushort smally = Math.Min( z.y1, z.y2 );
                    ushort bigy = Math.Max( z.y1, z.y2 );
                    ushort smallz = Math.Min( z.z1, z.z2 );
                    ushort bigz = Math.Max( z.z1, z.z2 );

                    for ( ushort xx = smallx; xx <= bigx; xx++ ) {
                        for ( ushort yy = smally; yy <= bigy; yy++ ) {
                            for ( ushort zz = smallz; zz <= bigz; zz++ ) {
                                byte b = p.Level.GetBlock( xx, yy, zz );
                                p.SendSetBlock( xx, yy, zz, b );
                            }
                        }
                    }
                }
            } );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Hides all revealed zones." );
        }
    }
}
