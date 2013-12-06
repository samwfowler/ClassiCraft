using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdWhois : Command {
        public override string Name {
            get { return "Whois"; }
        }

        public override string Syntax {
            get { return "/whois name"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                p.SendMessage( "&cPlease enter a player's name." );
                return;
            }

            string who = args.Split(' ')[0].Trim();
            Player targetPlayer = Player.Find( who );

            if ( targetPlayer == null ) {
                p.SendMessage( "&cPlayer \"&f" + who + "&c\" was not found, use &f/whowas&c." );
                return;
            }

            p.SendMessage( "Information on " + targetPlayer.Rank.Color + targetPlayer.Name + "&e." );
            p.SendMessage( "&f> &eIP Address: &8" + targetPlayer.IP + "&e." );
            p.SendMessage( "&f> &eCurrent level: " + Rank.GetColor( targetPlayer.Level.BuildPermission ) + targetPlayer.Level.Name + "&e." );
            p.SendMessage( "&f> &eCoins: &b" + targetPlayer.Coins + "&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays informantion about a specified player." );
        }
    }
}
