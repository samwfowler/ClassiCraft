using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdSpawn : Command {
        public override string Name {
            get { return "Spawn"; }
        }

        public override string Syntax {
            get { return "/spawn"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            ushort x = (ushort)( p.Level.SpawnX * 32 );
            ushort y = (ushort)( p.Level.SpawnY * 32 );
            ushort z = (ushort)( p.Level.SpawnZ * 32 );
            byte rotx = p.Level.SpawnRX;
            byte roty = p.Level.SpawnRY;

            Player.GlobalSpawn( p, x, y, z, rotx, roty );

            p.SendMessage( "&cSpawn: &eTeleported to spawn." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Sends you to the spawn point." );
        }

    }
}
