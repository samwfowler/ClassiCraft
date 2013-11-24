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
            autoRun = autorun;
            SpleefifyLevel();
            
            gameThread = new Thread( new ThreadStart( delegate {
                Player.GlobalMessage( "Spleef was started on " + Rank.GetColor(spleefLevel.JoinPermission) +  spleefLevel.Name + "&e, type &f/goto " + spleefLevel.Name + " &eto join." );
                Thread.Sleep( 5000 );
                Player.Message( spleefLevel, "Spleef starting in &a15 seconds&e..." );
                Thread.Sleep( 10000 );
                Player.Message( spleefLevel, "Spleef starting in &c5&e..." );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef starting in &c4&e..." );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef starting in &c3&e..." );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef starting in &c2&e..." );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef starting in &c1&e..." );
                Thread.Sleep( 1000 );
                Player.Message( spleefLevel, "Spleef game &astarted&e..." );

                players = new List<Player>();
                Player.PlayerList.ForEach( delegate( Player pl ) {
                    if ( pl.Level == spleefLevel ) {
                        players.Add( pl );
                        pl.SendServerIdentification();
                        pl.OnDeath += PlayerDeath;
                    }
                } );

                isRunning = true;

                ushort half = (ushort)( spleefLevel.Height / 2 );
                List<BufferPos> planeBlocks = new List<BufferPos>();
                for ( ushort xx = 0; xx < spleefLevel.Width; xx++ ) {
                    for ( ushort zz = 0; zz < spleefLevel.Depth; zz++ ) {
                        planeBlocks.Add( new BufferPos( xx, half, zz, Block.White ) );
                    }
                }

            flushStart:

                while ( planeBlocks.Count > 0 ) {
                    List<BufferPos> randomBlocks = new List<BufferPos>();
                    int blockNum = ( spleefLevel.Width * spleefLevel.Depth ) / 12;
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

                    if ( players.Count > 0 ) {
                        goto flushStart;
                    }
                }

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
            Player.Message( spleefLevel, p.Rank.Color + p.Name + " &cfell into the lava!" );
        }

        public void GameEnd() {
            isRunning = false;
            Player.GlobalMessage( "Spleef game &cover&e..." );

            string survivors = "";
            foreach ( Player p in players ) {
                survivors += " &f| " + p.Rank.Color + p.Name;
                lastSurvivor.Reward( 50, "Survived a whole game of spleef." );
            }

            if ( survivors != "" ) {
                Player.Message( spleefLevel, "&aRound Winners: " + survivors.Remove( 0, 5 ) );
            } else {
                Player.Message( spleefLevel, "&aRound Winner: " + lastSurvivor.Rank.Color + lastSurvivor.Rank.Name );
                lastSurvivor.Reward( 25, "Last survivor in spleef." );
            }

            if ( autoRun ) {
                GameStart( spleefLevel );
            } else {
                spleefLevel.isHostingGame = false;
            }
        }

        public void SpleefifyLevel() {
            for ( ushort xx = 0; xx < spleefLevel.Width; xx++ ) {
                for ( ushort yy = 0; yy < spleefLevel.Height; yy++ ) {
                    for ( ushort zz = 0; zz < spleefLevel.Depth; zz++ ) {
                        ushort half = (ushort)( spleefLevel.Height / 2 );
                        if ( yy == half ) {
                            spleefLevel.Blockchange( xx, yy, zz, Block.White );
                        } else if ( yy < half ) {
                            if ( yy == 0 ) {
                                spleefLevel.Blockchange( xx, yy, zz, Block.Lava );
                            } else {
                                spleefLevel.Blockchange( xx, yy, zz, Block.Air );
                            }
                        } else {
                            spleefLevel.Blockchange( xx, yy, zz, Block.Air );
                        }
                    }
                }
            }
        }
    }
}
