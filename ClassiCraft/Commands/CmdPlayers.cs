using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdPlayers : Command {
        public override string Name {
            get { return "Players"; }
        }

        public override string Syntax {
            get { return "/players"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            List<string> levels = new List<string>();

            Level.LevelList.ForEach( delegate( Level l ) {
                string playerList = l.Name + " - ";

                Player.PlayerList.ForEach( delegate( Player pl ) {
                    if ( pl.Level == l ) {
                        playerList += pl.Rank.Color + pl.Name + " &f| ";
                    }
                } );

                levels.Add( playerList.Substring( 0, playerList.Length - 5 ) );
            } );

            if ( levels.Count > 0 ) {
                p.SendMessage( "Players online:" );
            }

            levels.ForEach( delegate( string level ) {
                p.SendMessage( level );
            } );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of players on each level." );
        }

    }
}
