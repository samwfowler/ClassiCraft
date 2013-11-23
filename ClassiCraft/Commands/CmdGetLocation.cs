using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class CmdGetLoc : Command {
        public override string Name {
            get { return "getloc"; }
        }

        public override string Syntax {
            get { return "/getloc"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            p.SendMessage( "Location: " + (p.Pos[0] / 32) + ", " + (p.Pos[1] / 32) + ", " + (p.Pos[2] / 32) );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Gets your position in terms of X-Y-Z co-ordinates." );
        }

    }
}
