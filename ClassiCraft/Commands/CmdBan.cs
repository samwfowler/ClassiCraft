using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdBan : Command {
        public override string Name {
            get { return "Ban"; }
        }

        public override string Syntax {
            get { return "/ban player"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            string who = args;
            Player target = Player.Find( args );

            if ( BanList.bans.Contains(who.ToLower()) ) {
                p.SendMessage( "&cPlayer \"&f" + args + "&c\" is already banned." );
                return;
            }

            BanList.Add( who );

            if ( target != null ) {
                Player.GlobalMessage( "'" + target.Rank.Color + target.Name + "&e' was &8banned&e." );
                target.Kick( "You've been banned!" );
            } else {
                Player.GlobalMessage( "'&f" + who + " (offline)&e' was &8banned&e." );
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "Bans a player." );
        }

    }
}
