using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdPerJoin : Command {
        public override string Name {
            get { return "PerJoin"; }
        }

        public override string Syntax {
            get { return "/perjoin map rank"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.SuperOp; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                return;
            }

            if ( args.Split( ' ' )[0].Length < 2 ) {
                return;
            }

            Level targetLevel = Level.Find( args.Split( ' ' )[0] );

            if ( targetLevel == null ) {
                return;
            }

            Rank targetRank = Rank.Find( args.Split( ' ' )[1] );

            if ( targetRank == null ) {
                return;
            }

            targetLevel.JoinPermission = targetRank.Permission;
            Player.GlobalMessage( "Level '" + Rank.GetColor( targetLevel.BuildPermission ) + targetLevel.Name + "&e's joinperm was set to " + targetRank.Color + targetRank.Name + "&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Changes the join permission of a specified level." );
        }

    }
}
