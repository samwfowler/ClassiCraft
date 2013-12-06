using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace ClassiCraft {
    public class Server {
        public delegate void LogHandler( string message );
        public static event LogHandler OnLog;
        
        public static string salt = "";
        public static string version = "1.0";
        public static Socket socket;
        public static Level mainLevel;
        public static List<string> welcomeMessage;
        public static System.Timers.Timer messageTimer = new System.Timers.Timer( 300000 );

        public static void Start() {
            Log( "ClassiCraft 1.0 started..." );

            Directories.LoadDirectories();
            Config.LoadConfig();
            Rank.LoadRanks();
            BanList.LoadBans();
            Command.LoadCommands();
            ZoneDB.LoadZones();
            PortalDB.LoadPortals();
            Autosaver.Setup();

            mainLevel = Level.Load( Config.MainLevel );
            if ( mainLevel == null ) {
                mainLevel = new Level( Config.MainLevel, 128, 64, 128 );
                mainLevel.BuildPermission = PermissionLevel.Operator;
                mainLevel.Save();
            }

            if ( !Setup() ) {
                Log( "Failed to setup listening socket..." );
            } else {
                Log( "Set up socket on port " + Config.Port + "..." );
            }

            welcomeMessage = new List<string>();

            retry:
            if ( File.Exists( "documentation/welcome.txt" ) ) {
                welcomeMessage.AddRange( File.ReadAllLines( "documentation/welcome.txt" ) );
            } else {
                StreamWriter sw = new StreamWriter( File.Create( "documentation/welcome.txt" ) );
                sw.WriteLine( "Change this message in documentation/welcome.txt" );
                sw.Flush();
                sw.Close();
                sw.Dispose();
                goto retry;
            }

            messageTimer.Elapsed += delegate {
                Player.GlobalMessage( "Running ClassiCraft " + version );
            };
            messageTimer.Start();

            Thread posCheck = new Thread( new ThreadStart( delegate {
                while ( true ) {
                    Player.PlayerList.ForEach( delegate( Player p ) {
                        try {
                            if ( p.isLoggedIn ) {
                                ushort x = (ushort)(p.Pos[0] / 32);
                                ushort y = (ushort)(p.Pos[1] / 32);
                                ushort z = (ushort)(p.Pos[2] / 32);
                                byte currHeadBlock = p.Level.GetBlock( x, y, z );
                                byte currFootBlock = p.Level.GetBlock( x, (ushort)( y - 1 ), z );

                                if ( currHeadBlock == Block.Lava || currFootBlock == Block.Lava ) {
                                    p.Die();
                                } else if ( currHeadBlock == Block.PortalAir || currHeadBlock == Block.PortalLava || currHeadBlock == Block.PortalWater ) {
                                    PortalDB.PortalList.ForEach( delegate( Portal po ) {
                                        if ( po.x1 == x && po.y1 == y && po.z1 == z && po.Destination == p.Level ) {
                                            ushort xx = po.x2;
                                            ushort yy = po.y2;
                                            ushort zz = po.z2;

                                            xx *= 32; xx += 16;
                                            yy *= 32; yy += 32;
                                            zz *= 32; zz += 16;

                                            unchecked {
                                                p.SendSpawnPlayer( (byte)-1, p.Rank.Color + p.Name, xx, yy, zz, p.Rot[0], p.Rot[1] );
                                            }
                                        }
                                    } );
                                } else if ( currFootBlock == Block.PortalAir || currFootBlock == Block.PortalLava || currFootBlock == Block.PortalWater ) {
                                    PortalDB.PortalList.ForEach( delegate( Portal po ) {
                                        if ( po.x1 == x && po.y1 == (ushort)( y - 1 ) && po.z1 == z && po.Destination == p.Level ) {
                                            ushort xx = po.x2;
                                            ushort yy = po.y2;
                                            ushort zz = po.z2;

                                            xx *= 32; xx += 16;
                                            yy *= 32; yy += 32;
                                            zz *= 32; zz += 16;

                                            unchecked {
                                                p.SendSpawnPlayer( (byte)-1, p.Rank.Color + p.Name, xx, yy, zz, p.Rot[0], p.Rot[1] );
                                            }
                                        }
                                    } );
                                }

                                Thread.Sleep( 500 );
                            }
                        } catch {

                        }
                    } );
                }
            } ) );
            posCheck.Start();
        }

        public static bool Setup() {
            try {
                IPEndPoint endpoint = new IPEndPoint( IPAddress.Any, Config.Port );
                socket = new Socket( endpoint.Address.AddressFamily,
                                    SocketType.Stream, ProtocolType.Tcp );
                socket.Bind( endpoint );
                socket.Listen( (int)SocketOptionName.MaxConnections );
                socket.BeginAccept( new AsyncCallback( AcceptConnection ), null );
                return true;
            } catch {
                return false;
            }
        }

        public static void AcceptConnection( IAsyncResult result ) {
            try {
                new Player( socket.EndAccept( result ) );
                socket.BeginAccept( new AsyncCallback( AcceptConnection ), null );
            } catch {
                Log( "ERROR" );
            }
        }

        public static void Log( string message ) {
            OnLog( message );
        }
    }
}
