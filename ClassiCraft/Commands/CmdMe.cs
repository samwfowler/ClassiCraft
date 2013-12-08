using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdMe : Command {
        public override string Name {
            get { return "Me"; }
        }

        public override string Syntax {
            get { return "/me message"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Builder; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                p.SendMessage( "&cIncorrect syntax, refer to &f/help say &cfor more info." );
                return;
            }

            Player.GlobalMessage( p.Rank.Color + "*" + p.Name + " " + args + "*" );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Just try it out eh?." );
        }

    }
}
