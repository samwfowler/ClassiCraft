using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdBanned : Command {
        public override string Name {
            get { return "Banned"; }
        }

        public override string Syntax {
            get { return "/banned"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            string bans = "";

            BanList.bans.ForEach( delegate( string ban ) {
                string banstring = String.Format( "{1}{2}{3}", "&8", ban, ", " );
                bans += banstring;
            } );

            p.SendMessage( bans );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of banned players." );
        }

    }
}
