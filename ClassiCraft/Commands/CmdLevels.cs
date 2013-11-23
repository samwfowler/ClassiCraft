using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdLevels : Command {
        public override string Name {
            get { return "Levels"; }
        }

        public override string Syntax {
            get { return "/levels"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            string levels = "";

            Level.LevelList.ForEach( delegate( Level lvl ) {
                levels += Rank.GetColor( lvl.BuildPermission ) + lvl.Name + ", ";
            } );

            if ( levels != "" ) {
                p.SendMessage( "Levels online:" );
                p.SendMessage( levels.Remove( levels.Length - 2 ) );
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of levels loaded." );
        }

    }
}
