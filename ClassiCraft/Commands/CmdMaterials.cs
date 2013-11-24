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
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            string blockList = "";
            for ( int i = 0; i <= 49; i++ ) {
                blockList += "&e" + Block.Name((byte)i) + " &f| ";
            }
            Server.Log( blockList );
            p.SendMessage( "Available materials: " );
            p.SendMessage( blockList.Substring( 0, blockList.Length - 5 ) );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of available materials." );
        }

    }
}
