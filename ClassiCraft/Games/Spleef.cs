using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClassiCraft {
    public class SpleefGame {
        public List<Player> players;
        Level spleefLevel;
        Player lastSurvivor;
        Thread gameThread;
        public bool autoRun;
        public bool isRunning;

        public void GameStart(Level level, bool autorun = true) {
            spleefLevel = level;
            spleefLevel.isHostingGame = true;
            spleefLevel.enableEditing = false;
            autoRun = autorun;
            SpleefifyLevel();
            
            gameThread = new Thread( new ThreadStart( delegate {
                Player.GlobalMessage( "Spleef was started on " + Rank.GetColor(spleefLevel.JoinPermission) +  spleefLevel.Name + "&e, type &f/goto " + spleefLevel.Name + " &eto join." );
                Thread.Sleep( 5000 );
                Player.Message( spleefLevel, "Spleef starting in &a15 seconds" );
                Thread.Sleep( 10000 );
                Player.Message( spleefLevel, "Spleef starting in &c5" );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef starting in &c4" );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef starting in &c3" );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef starting in &c2" );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef starting in &c1" );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef game &astarted" );

                players = new List<Player>();
                //Player.PlayerList.ForEach( delegate( Player pl ) {
                foreach ( Player pl in Player.PlayerList ) {
                    if ( pl.Level == spleefLevel ) {
                        players.Add( pl );
                        //pl.SendLevel();
                        pl.OnDeath += PlayerDeath;
                    }
                }
                //} );

                isRunning = true;

                ushort half = (ushort)( spleefLevel.Height / 2 );
                List<BufferPos> planeBlocks = new List<BufferPos>();
                for ( ushort xx = 0; xx < spleefLevel.Width; xx++ ) {
                    for ( ushort zz = 0; zz < spleefLevel.Depth; zz++ ) {
                        planeBlocks.Add( new BufferPos( xx, half, zz, Block.White ) );
                    }
                }

                while ( planeBlocks.Count > 0 ) {
                    List<BufferPos> randomBlocks = new List<BufferPos>();
                    int blockNum = ( spleefLevel.Width * spleefLevel.Depth ) / 8;
                    Random randPos = new Random();

                    for ( int i = 0; i < blockNum; i++ ) {
                        randomBlocks.Add( planeBlocks[randPos.Next( 0, planeBlocks.Count - 1 )] );
                    }

                    foreach ( BufferPos bp in randomBlocks ) {
                        if ( spleefLevel.GetBlock( bp.X, bp.Y, bp.Z ) == Block.White ) {
                            spleefLevel.Blockchange( bp.X, bp.Y, bp.Z, Block.Random() );
                        }
                    } Thread.Sleep( 1000 );

                    foreach ( BufferPos bp in randomBlocks ) {
                        spleefLevel.Blockchange( bp.X, bp.Y, bp.Z, Block.Air );
                        planeBlocks.Remove( bp );
                    } Thread.Sleep( 1000 );

                    if ( players.Count <= 0 ) {
                        goto gameend;
                    }
                }

                gameend:
                GameEnd();
            } ) );
            gameThread.Start();
        }

        public void PlayerDeath(Player p) {
            if ( !players.Contains( p ) ) {
                return;
            }

            if ( players.Count == 1 ) {
                lastSurvivor = p;
            }

            players.Remove( p );
            p.ClearPosChange();
            p.ClearDeath();
            Player.Message( spleefLevel, p.Rank.Color + p.Name + " &cfell into the lava!" );
        }

        public void GameEnd() {
            isRunning = false;
            Player.GlobalMessage( "Spleef game &cended" );

            string survivors = "";
            foreach ( Player p in players ) {
                survivors += " &f| " + p.Rank.Color + p.Name;
                p.Reward( 50, "Survived a whole game of spleef." );
                p.ClearBlockChange();
                p.ClearDeath();
            }

            if ( survivors != "" ) {
                Player.Message( spleefLevel, "&aRound Winners: " + survivors.Remove( 0, 5 ) );
            } else {
                Player.Message( spleefLevel, "&aRound Winner: " + lastSurvivor.Rank.Color + lastSurvivor.Name );
                lastSurvivor.Reward( 25, "Last survivor in spleef." );
            }

            SpleefifyLevel();

            if ( autoRun ) {
                GameStart( spleefLevel );
            } else {
                spleefLevel.isHostingGame = false;
                spleefLevel.enableEditing = true;
            }
        }

        public void SpleefifyLevel() {
            for ( ushort xx = 0; xx < spleefLevel.Width; xx++ ) {
                ushort yy = (ushort)(spleefLevel.Height / 2);
                for ( ushort zz = 0; zz < spleefLevel.Depth; zz++ ) {
                    spleefLevel.Blockchange( xx, yy, zz, Block.White );
                    spleefLevel.Blockchange( xx, (ushort)( yy - 1 ), zz, Block.Lava );
                    spleefLevel.Blockchange( xx, (ushort)( yy - 2 ), zz, Block.Lava );
                }
            }
        }
    }
}
