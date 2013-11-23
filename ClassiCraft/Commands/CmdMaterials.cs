using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdMaterials : Command {
        public override string Name {
            get { return "Materials"; }
        }

        public override string Syntax {
            get { return "/materials"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            string blockList = "";
            for ( int i = 0; i <= 49; i++ ) {
                blockList += Block.Name((byte)i) + " &f|&e ";
            }
            p.SendMessage( "Available materials: " );
            p.SendMessage( blockList );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of available materials." );
        }

    }
}
