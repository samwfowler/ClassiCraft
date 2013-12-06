using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdUnmute : Command {
        public override string Name {
            get { return "Unmute"; }
        }

        public override string Syntax {
            get { return "/unmute player"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            string who = args.Split( ' ' )[0].Trim();
            Player targetPlayer = Player.Find( who );

            if ( targetPlayer == null ) {
                p.SendMessage( "&cPlayer \"&f" + who + "&c\" was not found." );
                return;
            }

            targetPlayer.isMuted = false;
            Player.GlobalMessage( targetPlayer.Rank.Color + targetPlayer.Name + " &ewas &aunmuted&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Unmutes a player." );
        }

    }
}
