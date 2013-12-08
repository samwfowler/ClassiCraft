using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdSetMain : Command {
        public override string Name {
            get { return "SetMain"; }
        }

        public override string Syntax {
            get { return "/setmain"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.SuperOp; }
        }

        public override void Use( Player p, string args ) {
            Config.MainLevel = p.Level.Name;
            Server.mainLevel = p.Level;
            Config.SaveConfig();

            Player.GlobalMessage( "Server's main level set to " + Rank.GetColor( p.Level.BuildPermission ) + p.Level.Name + "&e." );
            p.SendMessage( "&cSetMain: &eSet main to your current level." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Sets main to current level." );
        }

    }
}
