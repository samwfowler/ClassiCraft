using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClassiCraft {
    public class BlockrunGame {
        public List<Player> players;
        List<ActiveBlock> activeBlocks = new List<ActiveBlock>();
        Level blockrunLevel;
        Player lastSurvivor;
        Thread gameThread;
        public bool autoRun;
        public bool isRunning;

        public void GameStart( Level level, bool autorun = true ) {
            blockrunLevel = level;
            blockrunLevel.isHostingGame = true;
            blockrunLevel.enableEditing = false;
            autoRun = autorun;
            BlockrunifyLevel();

            gameThread = new Thread( new ThreadStart( delegate {
                Player.GlobalMessage( "Blockrun was started on " + Rank.GetColor( blockrunLevel.JoinPermission ) + blockrunLevel.Name + "&e, type &f/goto " + blockrunLevel.Name + " &eto join." );
                Thread.Sleep( 5000 );
                Player.Message( blockrunLevel, "Blockrun starting in &a15 seconds" );
                Thread.Sleep( 10000 );
                Player.Message( blockrunLevel, "Blockrun starting in &c5" );
                Thread.Sleep( 1000 );
                Player.Message( blockrunLevel, "Blockrun starting in &c4" );
                Thread.Sleep( 1000 );
                Player.Message( blockrunLevel, "Blockrun starting in &c3" );
                Thread.Sleep( 1000 );
                Player.Message( blockrunLevel, "Blockrun starting in &c2" );
                Thread.Sleep( 1000 );
                Player.Message( blockrunLevel, "Blockrun starting in &c1" );
                Thread.Sleep( 1000 );
                Player.Message( blockrunLevel, "Blockrun game &astarted" );

                players = new List<Player>();
                Player.PlayerList.ForEach( delegate( Player pl ) {
                    if ( pl.Level == blockrunLevel ) {
                        players.Add( pl );
                        pl.OnDeath += PlayerDeath;
                        pl.OnPosChange += PosChangeHandler;
                    }
                } );

                isRunning = true;

                Thread.Sleep( 2000 );
                Player.Message( blockrunLevel, "&cYou have 3 minutes to play." );

                DateTime start = DateTime.Now;
                DateTime end = DateTime.Now.AddSeconds( 10 );
                while ( end > DateTime.Now ) {
                    while ( players.Count > 0 ) {
                        // do nothing
                    }
                    goto gameend;
                }
                
                gameend:
                GameEnd();
            } ) );
            gameThread.Start();
        }

        public void PlayerDeath( Player p ) {
            if ( !players.Contains( p ) ) {
                return;
            }

            if ( players.Count == 1 ) {
                lastSurvivor = p;
            }

            players.Remove( p );
            p.ClearPosChange();
            p.ClearDeath();
            Player.Message( blockrunLevel, p.Rank.Color + p.Name + " &cfell into the lava!" );
        }

        public void GameEnd() {
            isRunning = false;
            Player.GlobalMessage( "Blockrun game &cended&e..." );

            string survivors = "";
            foreach ( Player p in players ) {
                survivors += " &f| " + p.Rank.Color + p.Name;
                p.Reward( 50, "Survived a whole game of blockrun." );
                p.ClearPosChange();
                p.ClearDeath();
            }

            if ( survivors != "" ) {
                Player.Message( blockrunLevel, "&aRound Winners: " + survivors.Remove( 0, 5 ) );
            } else {
                Player.Message( blockrunLevel, "&aRound Winner: " + lastSurvivor.Rank.Color + lastSurvivor.Name );
                lastSurvivor.Reward( 25, "Last survivor in blockrun." );
            }

            activeBlocks.Clear();
            BlockrunifyLevel();

            if ( autoRun ) {
                GameStart( blockrunLevel );
            } else {
                blockrunLevel.isHostingGame = false;
                blockrunLevel.enableEditing = true;
            }
        }

        public void BlockrunifyLevel() {
            for ( ushort xx = 0; xx < blockrunLevel.Width; xx++ ) {
                ushort yy = (ushort)( blockrunLevel.Height / 2 );
                for ( ushort zz = 0; zz < blockrunLevel.Depth; zz++ ) {
                    blockrunLevel.Blockchange( xx, yy, zz, Block.White );
                    blockrunLevel.Blockchange( xx, (ushort)( yy - 1 ), zz, Block.Lava );
                    blockrunLevel.Blockchange( xx, (ushort)( yy - 2 ), zz, Block.Lava );
                }
            }
        }

        public void PosChangeHandler( Player p ) {
            if ( p.Level.GetBlock( (ushort)( p.OldPos[0] / 32 ), (ushort)( ( p.OldPos[1] / 32 ) - 2 ), (ushort)( p.OldPos[2] / 32 ) ) == Block.White ) {
                activeBlocks.Add( new ActiveBlock( p.Level, (ushort)( p.OldPos[0] / 32 ), (ushort)( ( p.OldPos[1] / 32 ) - 2 ), (ushort)( p.OldPos[2] / 32 ) ) );
            }
        }

        public struct ActiveBlock {
            static Level thisLevel;
            static ushort X;
            static ushort Y;
            static ushort Z;
            static bool isActive;

            public ActiveBlock( Level lvl, ushort x, ushort y, ushort z ) {
                X = x;
                Y = y;
                Z = z;
                thisLevel = lvl;
                isActive = true;

                Thread changeThread = new Thread( new ThreadStart( delegate {
                    while ( isActive ) {
                        thisLevel.Blockchange( x, y, z, Block.Red );
                        Thread.Sleep( 500 );
                        thisLevel.Blockchange( x, y, z, Block.Air );

                        isActive = false;
                    }
                } ) );
                changeThread.Start();
            }
        }
    }
}
