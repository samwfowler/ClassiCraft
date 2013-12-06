using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdGoto : Command {
        public override string Name {
            get { return "Goto"; }
        }

        public override string Syntax {
            get { return "/goto map"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                p.SendMessage( "&cYou must enter a level name." );
                return;
            }

            Level targetLevel = Level.Find( args );

            if ( targetLevel == null ) {
                p.SendMessage( "&cLevel \"&f" + args + "&c\" isn't loaded." );
                return;
            }

            Player.GlobalDespawn( p );

            p.Level = targetLevel;
            p.SendLevel();

            Player.GlobalMessage( p.Rank.Color + p.Name + " &echanged levels to " + Rank.GetColor( targetLevel.BuildPermission ) + targetLevel.Name + "&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Changes to a specified level." );
        }

    }
}
