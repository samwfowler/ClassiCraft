using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdUnload : Command {
        public override string Name {
            get { return "Unload"; }
        }

        public override string Syntax {
            get { return "/unload map"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.AdvBuilder; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                p.SendMessage( "&cYou must enter a level name." );
                return;
            }

            Level targetLevel = Level.Find( args );

            if ( targetLevel == null ) {
                p.SendMessage( "&cLevel \"&f" + args + "&c\" was not found." );
                return;
            }

            Player.PlayerList.ForEach( delegate( Player pl ) {
                if ( pl.Level == targetLevel ) {
                    pl.Level = Server.mainLevel;
                    pl.SendLevel();
                }
            } );

            Level.LevelList.Remove( targetLevel );

            Player.GlobalMessage( "Level '" + Rank.GetColor( targetLevel.BuildPermission ) + targetLevel.Name + "&e' was unloaded." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Unloads a specified level." );
        }

    }
}
