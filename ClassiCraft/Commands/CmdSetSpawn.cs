using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdSetSpawn : Command {
        public override string Name {
            get { return "SetSpawn"; }
        }

        public override string Syntax {
            get { return "/setspawn"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            ushort x = (ushort)( p.Pos[0] / 32 );
            ushort y = (ushort)( p.Pos[1] / 32 );
            ushort z = (ushort)( p.Pos[2] / 32 );
            byte rotx = p.Rot[0];
            byte roty = p.Rot[1];

            p.Level.SpawnX = x;
            p.Level.SpawnY = y;
            p.Level.SpawnZ = z;
            p.Level.SpawnRX = rotx;
            p.Level.SpawnRY = roty;

            p.SendMessage( "&cSetSpawn: &eSet spawn to your current location." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Sets spawn to current location." );
        }

    }
}
