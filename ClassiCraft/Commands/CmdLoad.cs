using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdLoad : Command {
        public override string Name {
            get { return "Load"; }
        }

        public override string Syntax {
            get { return "/load map"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Architect; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                return;
            }

            if ( Level.Find( args ) != null ) {
                p.SendMessage( "&cLevel \"&f" + args + "&c\" is already loaded." );
                return;
            }

            Level targetLevel = Level.Load( args );

            if ( targetLevel == null ) {
                p.SendMessage( "&cLevel \"&f" + args + "&c\" was not loaded." );
                return;
            }

            Player.GlobalMessage( "Level '" + Rank.GetColor( targetLevel.BuildPermission ) + targetLevel.Name + "&e' was loaded." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Loads a specified level." );
        }

    }
}
