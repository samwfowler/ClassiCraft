using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdPerBuild : Command {
        public override string Name {
            get { return "PerBuild"; }
        }

        public override string Syntax {
            get { return "/perbuild map rank"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Administrator; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                return;
            }

            if ( args.Split( ' ' )[0].Length < 2 ) {
                p.SendMessage( "&cInvalid arguments, see &f/help " + Name + " &cfor more detail." );
                return;
            }

            Level targetLevel = Level.Find( args.Split( ' ' )[0] );

            if ( targetLevel == null ) {
                p.SendMessage( "&cLevel \"&f" + args.Split( ' ' )[0] + "&c\" was not found." );
                return;
            }

            Rank targetRank = Rank.Find( args.Split( ' ' )[1] );

            if ( targetRank == null ) {
                p.SendMessage( "&cRank \"&f" + args.Split( ' ' )[1] + "&c\" was not found." );
                return;
            }

            targetLevel.BuildPermission = targetRank.Permission;
            Player.GlobalMessage( "Level '" + Rank.GetColor( targetLevel.BuildPermission ) + targetLevel.Name + "&e's buildperm was set to " + targetRank.Color + targetRank.Name + "&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Changes the build permission of a specified level." );
        }

    }
}
