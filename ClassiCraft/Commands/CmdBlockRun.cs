using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClassiCraft {
    public class CmdBlockRun : Command {
        public override string Name {
            get { return "blockrun"; }
        }

        public override string Syntax {
            get { return "/blockrun"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Member; }
        }

        List<ActiveBlock> activeBlocks = new List<ActiveBlock>();

        public override void Use( Player p, string args ) {
            Level blockRunLevel = p.Level;
            List<Player> players = new List<Player>();

            bool Intense = false;
            bool isRunning = true;
            
            if ( blockRunLevel.Width > 64 || blockRunLevel.Depth > 64 ) {
                p.SendMessage( "&cBlockrun works better in a smaller map!" );
                return;
            }

            for ( ushort xx = 0; xx < blockRunLevel.Width; xx++ ) {
                for ( ushort yy = 0; yy < blockRunLevel.Height; yy++ ) {
                    for ( ushort zz = 0; zz < blockRunLevel.Depth; zz++ ) {
                        ushort half = (ushort)( blockRunLevel.Height / 2 );
                        if ( yy == half ) {
                            blockRunLevel.Blockchange( xx, yy, zz, Block.White );
                        } else if ( yy < half ) {
                            if ( yy == 0 ) {
                                blockRunLevel.Blockchange( xx, yy, zz, Block.Lava );
                            } else {
                                blockRunLevel.Blockchange( xx, yy, zz, Block.Air );
                            }
                        } else {
                            blockRunLevel.Blockchange( xx, yy, zz, Block.Air );
                        }
                    }
                }
            }

            Thread checkDeaths = new Thread( new ThreadStart( delegate {
                while ( isRunning ) {
                    players.ForEach( delegate( Player pl ) {
                        byte currBlock = blockRunLevel.GetBlock( (ushort)( pl.Pos[0] / 32 ), (ushort)( ( pl.Pos[1] / 32 ) - 1 ), (ushort)( pl.Pos[2] / 32 ) );
                        if ( currBlock == Block.Lava ) {
                            if ( players.Count == 1 ) {
                                Player.GlobalMessage( "Blockrun game &cover&e..." );
                            }

                            players.Remove( pl );
                            pl.SendMessage( "&cYou have died." );
                            Player.GlobalMessage( pl.Rank.Color + pl.Name + " &cfell into the lava!" );
                            pl.ClearPosChange();
                        }
                    } );
                }
            } ) );

            Thread blockRun = new Thread( new ThreadStart( delegate {
                Player.GlobalMessage( "&bBlockrun &ewas started on &b" + blockRunLevel.Name + "&e, type &b/goto " + blockRunLevel.Name + " &eto join." );
                Thread.Sleep( 5000 );
                Player.GlobalMessage( "Blockrun starting in &a15 seconds&e..." );
                Thread.Sleep( 10000 );
                Player.GlobalMessage( "Blockrun starting in &c5&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Blockrun starting in &c4&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Blockrun starting in &c3&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Blockrun starting in &c2&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Blockrun starting in &c1&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Blockrun game &astarted&e..." );

                checkDeaths.Start();

                Player.PlayerList.ForEach( delegate( Player pl ) {
                    //pl.SendLevel();
                    players.Add( pl );
                    pl.OnPosChange += PosChangeHandler;
                } );
            } ) );
            blockRun.Start();
            
        }

        public override void Help( Player p ) {
            p.SendMessage( "Sends a message to the whole server." );
        }

        public void PosChangeHandler(Player p) {
            if ( p.Level.GetBlock( (ushort)( p.OldPos[0] / 32 ), (ushort)( ( p.OldPos[1] / 32 ) - 2 ), (ushort)( p.OldPos[2] / 32 ) ) == Block.White ) {
                activeBlocks.Add( new ActiveBlock( p.Level, (ushort)( p.OldPos[0] / 32 ), (ushort)( ( p.OldPos[1] / 32 ) - 2 ), (ushort)( p.OldPos[2] / 32 ) ) );
            }
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
