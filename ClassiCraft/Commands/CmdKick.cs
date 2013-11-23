using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdKick : Command {
        public override string Name {
            get { return "Kick"; }
        }

        public override string Syntax {
            get { return "/kick player [reason]"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            string target;
            string reason;

            if ( args.IndexOf( ' ' ) != -1 && args.Split(' ').Length > 1 ) {
                target = args.Split( ' ' )[0];
                reason = args.Substring( target.Length ).Trim();
            } else {
                target = args.Trim();
                reason = "You've been kicked.";
            }

            Player who = Player.Find( target );

            if ( who == null ) {
                p.SendMessage( "&cPlayer \"&f" + target + "&c\" was not found." );
                return;
            } else if ( who.Rank.Permission >= p.Rank.Permission ) {
                p.SendMessage( "&cYou cannot kick this player." );
                return;
            }

            who.Kick( reason );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Kicks player specified with optional reason." );
        }

    }
}
