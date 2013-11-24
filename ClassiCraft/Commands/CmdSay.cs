using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdSay : Command {
        public override string Name {
            get { return "say"; }
        }

        public override string Syntax {
            get { return "/say message"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Member; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                p.SendMessage( "&cIncorrect syntax, refer to &f/help say &cfor more info." );
                return;
            }

            Player.GlobalMessage( "&c>> &d" + args );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Sends a message to the whole server." );
        }

    }
}
