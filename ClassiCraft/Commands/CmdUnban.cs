using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdUnban : Command {
        public override string Name {
            get { return "Unban"; }
        }

        public override string Syntax {
            get { return "/unban player"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            string who = args;

            if ( !BanList.bans.Contains( who.ToLower() ) ) {
                p.SendMessage( "&cPlayer \"&f" + who + "&c\" isn't banned." );
                return;
            }

            BanList.Remove( who );
            Player.GlobalMessage( "'&8" + who + "&e' was &aunbanned&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Unbans a player." );
        }

    }
}
