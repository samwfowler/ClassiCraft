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
                        byte currBlock = p.Level.GetBlock( (ushort)( p.Pos[0] / 32 ), (ushort)( ( p.Pos[1] / 32 ) - 1 ), (ushort)( p.Pos[2] / 32 ) );
                        if ( currBlock == Block.Lava ) {
                            p.Die();
                            Thread.Sleep( 500 );
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
