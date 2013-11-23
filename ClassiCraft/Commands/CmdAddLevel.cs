using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdAddLevel : Command {
        public override string Name {
            get { return "AddLevel"; }
        }

        public override string Syntax {
            get { return "/addlevel name width height depth"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                return;
            }

            if ( args.Split( ' ' ).Length < 4 ) {
                return;
            }

            string name = args.Split( ' ' )[0];
            ushort width = (ushort)int.Parse( args.Split( ' ' )[1] );
            ushort height = (ushort)int.Parse( args.Split( ' ' )[2] );
            ushort depth = (ushort)int.Parse( args.Split( ' ' )[3] );

            Level newLevel = new Level( name, width, height, depth );
            newLevel.Save();

            Player.GlobalMessage( "Level '&f" + name + "&e' was created." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Creates a new level." );
        }

    }
}
