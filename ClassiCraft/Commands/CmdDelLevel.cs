using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace ClassiCraft {
    public class CmdDelLevel : Command {
        public override string Name {
            get { return "DelLevel"; }
        }

        public override string Syntax {
            get { return "/dellevel map"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                p.SendMessage( "&cYou must enter a level name." );
                return;
            }

            string level = args.Split( ' ' )[0].Trim();
            Level targetLevel = Level.Find( level );

            if ( targetLevel != null ) {
                level = targetLevel.Name;

                Player.PlayerList.ForEach( delegate( Player pl ) {
                    if ( pl.Level == targetLevel ) {
                        Player.GlobalDespawn( p );
                        pl.Level = Server.mainLevel;
                        pl.SendLevel();
                    }
                } );

                Level.LevelList.Remove(targetLevel);
            }

            if ( File.Exists( "levels/" + level.ToLower() + ".lvl" ) ) {
                File.Delete( "levels/" + level.ToLower() + ".lvl" );
            } else {
                p.SendMessage( "&cFailed to delete level." );
            }

            Player.GlobalMessage( "Level \"&f" + level + "&e\" was deleted." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Deletes a specified level." );
        }

    }
}
