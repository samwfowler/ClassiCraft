using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClassiCraft {
    public class CmdSpleef : Command {
        public override string Name {
            get { return "spleef"; }
        }

        public override string Syntax {
            get { return "/spleef"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Member; }
        }

        public override void Use( Player p, string args ) {
            Level oldLevel = p.Level;
            Level spleefLevel = new Level( p.Level.Name + "(spleef)", p.Level.Width, p.Level.Height, p.Level.Depth );
                  spleefLevel.BuildPermission = PermissionLevel.Administrator;
            List<Player> players = new List<Player>();
            Player lastSurvivor;

            for ( ushort xx = 0; xx < spleefLevel.Width; xx++ ) {
                for ( ushort zz = 0; zz < spleefLevel.Depth; zz++ ) {
                    spleefLevel.Blockchange( xx, (ushort)( spleefLevel.Height / 2 ), zz, Block.White );
                    spleefLevel.Blockchange( xx, (ushort)( ( spleefLevel.Height / 2 ) - 1 ), zz, Block.Lava );
                }
            }

            Player.PlayerList.ForEach( delegate( Player pl ) {
                if ( pl.Level == p.Level ) {
                    pl.Level = spleefLevel;
                    players.Add( pl );
                    pl.SendLevel();
                }
            } );

            Thread spleefGame = new Thread( new ThreadStart( delegate {
                Thread.Sleep( 5000 );

                Player.GlobalMessage( "Spleef starting in &c5&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Spleef starting in &c4&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Spleef starting in &c3&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Spleef starting in &c2&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Spleef starting in &c1&e..." );
                Thread.Sleep( 1000 );
                Player.GlobalMessage( "Spleef game &astarted&e!" );

                int rounds = 12;
                int roundCount = 0;

            newRound:
                List<BufferPos> randomBlocks = new List<BufferPos>();
                int blockNum = ( p.Level.Width * p.Level.Depth ) / 3;
                Random randPos = new Random();
                ushort half = (ushort)( p.Level.Height / 2 );

                for ( int i = 0; i < blockNum; i++ ) {
                    randomBlocks.Add( new BufferPos( (ushort)randPos.Next( 0, p.Level.Width ), half, (ushort)randPos.Next( 0, p.Level.Depth ), Block.Red ) );
                }

                foreach ( BufferPos bp in randomBlocks ) {
                    p.Level.Blockchange( bp.X, bp.Y, bp.Z, bp.Type );
                }

                Thread.Sleep( 1000 );

                foreach ( BufferPos bp in randomBlocks ) {
                    p.Level.Blockchange( bp.X, bp.Y, bp.Z, Block.Air );
                }

                Thread.Sleep( 500 );

                players.ForEach( delegate( Player pl ) {
                    byte currBlock = spleefLevel.GetBlock( (ushort)( pl.Pos[0] / 32 ), (ushort)( ( pl.Pos[1] / 32 ) - 1 ), (ushort)( pl.Pos[2] / 32 ) );
                    if ( currBlock == Block.Lava ) {
                        if ( players.Count == 1 ) {
                            lastSurvivor = pl;
                        }

                        players.Remove( pl );
                        pl.SendMessage( "&cYou have died." );
                        Player.GlobalMessage( pl.Rank.Color + pl.Rank.Name + " &cdied." );
                        Command.Find( "goto" ).Use( pl, oldLevel.Name );
                    }
                } );

                Thread.Sleep( 500 );

                roundCount++;

                if ( roundCount <= rounds ) {
                    goto newRound;
                } else {
                    Player.GlobalMessage( "Spleef &cover&e..." );
                    string survivors = "";
                    players.ForEach( delegate( Player survivor ) {
                        survivors += " &f| " + survivor.Rank.Color + survivor.Name;
                    } );
                    Player.GlobalMessage( "&aSpleef winners:" );
                    Player.GlobalMessage( survivors.Remove( 0, 5 ) );
                }
            } ) );
            spleefGame.Start();
        }

        public override void Help( Player p ) {
            p.SendMessage( "Sends a message to the whole server." );
        }

    }
}
