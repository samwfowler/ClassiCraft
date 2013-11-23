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
                return;
            }

            Level targetLevel = Level.Find( args );

            if ( targetLevel == null ) {
                return;
            }

            Player.GlobalDespawn( p );
            p.Level = targetLevel;
            p.SendLevel();
            Player.GlobalSpawn( p,
                                (ushort)( ( 0.5 + targetLevel.SpawnX ) * 32 ),
                                (ushort)( ( 1 + targetLevel.SpawnY ) * 32 ),
                                (ushort)( ( 0.5 + targetLevel.SpawnZ ) * 32 ), 
                                targetLevel.SpawnRX, 
                                targetLevel.SpawnRY );

            Player.PlayerList.ForEach( delegate( Player pl ) {
                if ( pl.Level == p.Level && pl != p ) {
                    p.SendSpawnPlayer( pl.ID, pl.Rank.Color + pl.Name, pl.Pos[0], pl.Pos[1], pl.Pos[2], pl.Rot[0], pl.Rot[1] );
                }
            } );

            Player.GlobalMessage( p.Rank.Color + p.Name + " &echanged levels to " + Rank.GetColor( targetLevel.BuildPermission ) + targetLevel.Name + "&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Changes to a specified level." );
        }

    }
}
