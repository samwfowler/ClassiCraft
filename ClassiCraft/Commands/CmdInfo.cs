using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdInfo : Command {
        public override string Name {
            get { return "Info"; }
        }

        public override string Syntax {
            get { return "/info"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            p.SendMessage( "&f" + Config.Name + "&e running on ClassiCraft 1.0." );
            if ( Player.PlayerList.Count == 1 ) {
                p.SendMessage( "There is &a1 &eplayer online." );
            } else {
                p.SendMessage( "There are &a" + Player.PlayerList.Count + " &eplayers online." );
            }
            p.SendMessage( "Thankyou for playing on this server!" );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays server information." );
        }

    }
}
