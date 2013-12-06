using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdTitle : Command {
        public override string Name {
            get { return "Title"; }
        }

        public override string Syntax {
            get { return "/title title"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Architect; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                p.NamePrefix = "";
                Player.GlobalMessage( p.Rank.Color + p.Name + "&e's title was &cremoved&e." );
                return;
            }

            p.NamePrefix = "[" + args + "]";
            Player.GlobalMessage( p.Rank.Color + p.Name + "&e's title was changed to &b" + p.NamePrefix + "&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Changes your title (name prefix)." );
            p.SendMessage( "Use just /title to remove title." );
        }
    }
}
