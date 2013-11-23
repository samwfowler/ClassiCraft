using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdSave : Command {
        public override string Name {
            get { return "Save"; }
        }

        public override string Syntax {
            get { return "/save map"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Architect; }
        }

        public override void Use( Player p, string args ) {
            bool all = false;

            if ( args == "" ) {
                all = true;
                return;
            }

            if ( all ) {
                foreach ( Level lvl in Level.LevelList ) {
                    lvl.Save();
                }

                Player.GlobalMessage( "&cAll &elevels were saved." );
            } else {
                Level targetLevel = Level.Find( args );

                if ( targetLevel == null ) {
                    p.SendMessage( "&cLevel \"&f" + args + "&c\" was not found." );
                    return;
                }

                targetLevel.Save();

                Player.GlobalMessage( "Level '" + Rank.GetColor( targetLevel.BuildPermission ) + targetLevel.Name + "&e' was saved." );
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "Saves a specified level." );
        }

    }
}
