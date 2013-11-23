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
            p.SendLevel();
        }

        public override void Help( Player p ) {
            p.SendMessage( "Hides all revealed zones." );
        }
    }
}
