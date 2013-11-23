using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdDelLevel : Command {
        public override string Name {
            get { return "DelLevel"; }
        }

        public override string Syntax {
            get { return "/dellevel map"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            p.SendMessage( "&cNot implemented yet." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Deletes a specified level." );
        }

    }
}
