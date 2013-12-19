using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace ClassiCraft {
    public sealed partial class Player {
        public static List<Player> PlayerList = new List<Player>();
        public static List<Player> ConnectionList = new List<Player>();
        static ASCIIEncoding enc = new ASCIIEncoding();
        static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

        public delegate void BlockchangeEventHandler( Player p, ushort x, ushort y, ushort z, byte type );
        public event BlockchangeEventHandler OnBlockChange = null;
        public void ClearBlockChange() { OnBlockChange = null; }

        public delegate void DeathHandler(Player p);
        public event DeathHandler OnDeath;
        public void ClearDeath() { OnDeath = null; }

        public delegate void PosChangeHandler(Player p);
        public event PosChangeHandler OnPosChange;
        public void ClearPosChange() { OnPosChange = null; }

        public System.Timers.Timer pingTimer = new System.Timers.Timer();
        byte Version;
        byte MagicNumber;
        string Key;

        public string Name;
        public string NamePrefix = "";
        public string NameSuffix = "";
        public string IP;
        public int Coins;
        public byte ID;
        public Rank Rank;
        public Level Level = Server.mainLevel;
        public BufferCopy BufferCopy;

        Socket Socket;
        byte[] buffer = new byte[0];
        byte[] tempBuffer = new byte[255];
        byte[] Bindings = new byte[128];

        public bool hasDisconnected = false;
        public bool isLoggedIn = false;
        public bool isMuted = false;
        public bool isCPECapable = false;
        public bool isPainting = false;

        public ushort[] Pos = new ushort[3] { 0, 0, 0 };
        public ushort[] OldPos = new ushort[3] { 0, 0, 0 };
        public ushort[] BasePos = new ushort[3] { 0, 0, 0 };
        public byte[] Rot = new byte[2] { 0, 0 };
        public byte[] OldRot = new byte[2] { 0, 0 };

        public Player( Socket socket ) {
            Socket = socket;
            IP = socket.RemoteEndPoint.ToString().Split( ':' )[0];
            Server.Log( IP + " connected...");

            for ( byte i = 0; i < 128; ++i ) {
                Bindings[i] = i;
            }

            Socket.BeginReceive( tempBuffer, 0, tempBuffer.Length, SocketFlags.None, new AsyncCallback( recievePlayer ), this );

            pingTimer.Interval = 500;
            pingTimer.Elapsed += delegate {
                SendPing();
            };
            pingTimer.Start();

            ConnectionList.Add( this );
        }

        static void recievePlayer( IAsyncResult result ) {
            Player player = (Player)result.AsyncState;

            if ( player.hasDisconnected ) {
                return;
            }

            try {
                int length = player.Socket.EndReceive( result );
                if ( length == 0 ) {
                    player.Disconnect();
                    return;
                }

                byte[] b = new byte[player.buffer.Length + length];
                Buffer.BlockCopy( player.buffer, 0, b, 0, player.buffer.Length );
                Buffer.BlockCopy( player.tempBuffer, 0, b, player.buffer.Length, length );

                player.buffer = player.HandleMessage( b );
                player.Socket.BeginReceive( player.tempBuffer, 0, player.tempBuffer.Length, SocketFlags.None,
                                      new AsyncCallback( recievePlayer ), player );
            } catch ( SocketException ) {
                player.Disconnect();
            } catch ( Exception e ) {
                Server.Log( e.ToString() );
                player.SendKick("Error!");
            }
        }

        byte[] HandleMessage( byte[] buffer ) {
            try {
                int length = 0;
                byte msg = buffer[0];

                switch ( msg ) {
                    case 0:
                        length = 130;
                        break;
                    case 5:
                        if ( !isLoggedIn )
                            goto default;
                        length = 8;
                        break;
                    case 8:
                        if ( !isLoggedIn )
                            goto default;
                        length = 9;
                        break;
                    case 13:
                        if ( !isLoggedIn )
                            goto default;
                        length = 65;
                        break;
                    case 16:
                        length = 67;
                        break;
                    case 17:
                        length = 69;
                        break;
                    default:
                        Kick( "Unsupported packet ID: " + msg );
                        return new byte[0];
                }

                if ( buffer.Length > length ) {
                    byte[] message = new byte[length];
                    Buffer.BlockCopy( buffer, 1, message, 0, length );

                    byte[] tempbuffer = new byte[buffer.Length - length - 1];
                    Buffer.BlockCopy( buffer, length + 1, tempbuffer, 0, buffer.Length - length - 1 );

                    buffer = tempbuffer;

                    switch ( msg ) {
                        case 0:
                            HandleLogin( message );
                            break;
                        case 5:
                            if ( !isLoggedIn )
                                break;
                            HandleBlockchange( message );
                            break;
                        case 8:
                            if ( !isLoggedIn )
                                break;
                            HandlePosition( message );
                            break;
                        case 13:
                            if ( !isLoggedIn )
                                break;
                            HandleChat( message );
                            break;
                        case 16:
                            HandleExtInfo( message );
                            break;
                        case 17:
                            HandleExtEntry( message );
                            break;
                    }

                    if ( buffer.Length > 0 ) {
                        buffer = HandleMessage( buffer );
                    } else {
                        return new byte[0];
                    }
                }
            } catch(Exception e) {
                SendKick( "Error!" );
                Server.Log( e.ToString() );
            }

            return buffer;
        }

        void HandleLogin( byte[] message ) {
            Version = message[0];
            Name = enc.GetString( message, 1, 64 ).Trim();
            Key = enc.GetString( message, 65, 32 ).Trim();
            MagicNumber = message[129];

            if ( Version != 7 ) {
                Kick( "Wrong version..." );
                return;
            }

            foreach ( Player p in PlayerList ) {
                if ( p.Name.ToLower() == Name.ToLower() ) {
                    Kick( "Someone is already logged in with your username!" );
                    return;
                }
            }

            if ( Config.verifyPlayers ) {
                if ( Key == BitConverter.ToString( md5.ComputeHash( enc.GetBytes( Server.salt + Name ) ) ) ) {
                    Server.Log( "Signed in!" );
                } else {
                    Kick( "Failed to verify account, please try again..." );
                    return;
                }
            }

            if ( BanList.bans.Contains( Name.ToLower() ) ) {
                Kick( "You're banned!" );
                return;
            }

            if ( MagicNumber == 66 ) {
                isCPECapable = true;
                NameSuffix = "+";
                SendExtInfo(3);

                SendExtEntry( "ClickDistance", 1 );
                SendExtEntry( "CustomBlocks", 1 );
                SendExtEntry( "MessageTypes", 1 ); // don't think classicube supports this ext. yet
                short ClickDistance;
                
                unchecked {
                    ClickDistance = 0; // hmm this seems a good use for greifers
                }

                SendClickDistance( ClickDistance );
                SendCustomBlockSupportLevel( 1 );
            }

            ID = GetID();
            PlayerDB.Load( this );
            Rank = Rank.RankList[0];
            PlayerList.Add( this );
            isLoggedIn = true;
            SendLevel();

            Server.Log( "--> " + Name + " has joined (IP: " + IP + ")..." );
            GlobalMessage( this, "Player '" + Rank.Color + Name + "&e' logged in." );
            GlobalAnnouncement( Name + " joined!" );
            
            foreach ( string line in Server.welcomeMessage ) {
                SendMessage( line );
            }
        }

        void HandleBlockchange( byte[] message ) {
            ushort x = NetworkToHostOrder( message, 0 );
            ushort y = NetworkToHostOrder( message, 2 );
            ushort z = NetworkToHostOrder( message, 4 );
            byte mode = message[6];
            byte type = message[7];

            if ( !Level.enableEditing ) {
                byte b = Level.GetBlock( x, y, z );
                SendSetBlock( x, y, z, b );
                SendMessage( "&cBuilding on this map has been disabled." );
                return;
            }

            if ( Level.BuildPermission > Rank.Permission ) {
                byte b = Level.GetBlock( x, y, z );
                SendSetBlock( x, y, z, b );
                SendMessage( "&cThis level is reserved for " + Rank.GetColor(Level.BuildPermission) + Rank.Find(Level.BuildPermission).Name + "&c." );
                return;
            }

            foreach ( Zone zn in ZoneDB.ZoneList ) {
                if ( zn.x1 <= x && zn.x2 >= x && zn.y1 <= y && zn.y2 >= y && zn.z1 <= z && zn.z2 >= z && zn.Permission > Rank.Permission) {
                    byte b = Level.GetBlock( x, y, z );
                    SendSetBlock( x, y, z, b );
                    SendMessage( "&cThis zone is reserved for " + Rank.GetColor(zn.Permission) + Rank.Find(zn.Permission).Name + "&c." );
                    return;
                }
            }

            foreach ( Portal p in PortalDB.PortalList ) {
                if ( p.x1 == x && p.y1 == y && p.z1 == z ) {
                    byte b = Level.GetBlock( x, y, z );
                    SendSetBlock( x, y, z, b );

                    if ( b == Block.PortalWater || b == Block.PortalAir || b == Block.PortalLava ) {
                        return;
                    }

                    ushort xx = p.x2;
                    ushort yy = p.y2;
                    ushort zz = p.z2;

                    xx *= 32; xx += 16;
                    yy *= 32; yy += 32;
                    zz *= 32; zz += 16;

                    unchecked {
                        SendSpawnPlayer( (byte)-1, Rank.Color + Name, xx, yy, zz, Rot[0], Rot[1] );
                    }
                    return;
                }
            }

            if ( OnBlockChange != null ) {
                OnBlockChange( this, x, y, z, type );
                return;
            }

            if ( mode == 0 && !isPainting ) {
                HandleBlockDelete( x, y, z, type );
            } else {
                HandleBlockPlace( x, y, z, type );
            }
        }

        void HandleBlockDelete( ushort x, ushort y, ushort z, byte type ) {
            byte blockDeleted = Level.GetBlock( x, y, z );
            switch ( blockDeleted ) {
                case Block.Staircasefull:
                    Level.Blockchange( this, x, y, z, Block.Staircasestep );
                    break;
                default:
                    Level.Blockchange( this, x, y, z, Block.Air );
                    break;
            }
        }

        void HandleBlockPlace( ushort x, ushort y, ushort z, byte type ) {
            switch ( type ) {
                case Block.Dirt:
                    if ( Level.GetBlock( x, (ushort)( y + 1 ), z ) == Block.Air ) {
                        Level.Blockchange( this, x, y, z, Block.Grass );
                    } else {
                        goto default;
                    }
                    break;
                case Block.Staircasestep:
                    if ( Level.GetBlock( x, (ushort)( y - 1 ), z ) == Block.Staircasestep ) {
                        SendSetBlock( x, y, z, Block.Air );
                        Level.Blockchange( this, x, (ushort)( y - 1 ), z, Block.Staircasefull );
                    } else {
                        goto default;
                    }
                    break;
                default:
                    if ( Level.GetBlock( x, (ushort)( y - 1 ), z ) == Block.Grass ) {
                        Level.Blockchange( this, x, (ushort)( y - 1 ), z, Block.Dirt );
                    }
                    Level.Blockchange( this, x, y, z, type );
                    break;
            }
        }

        void HandlePosition( byte[] message ) {
            if ( !isLoggedIn ) {
                return;
            }

            ushort x = NetworkToHostOrder( message, 1 );
            ushort y = NetworkToHostOrder( message, 3 );
            ushort z = NetworkToHostOrder( message, 5 );
            byte rotx = message[7];
            byte roty = message[8];
            Pos = new ushort[3] { x, y, z };
            Rot = new byte[2] { rotx, roty };

            UpdatePosition();
            if ( OnPosChange != null ) {
                OnPosChange(this);
            }
        }

        void HandleChat( byte[] message ) {
            string text = enc.GetString( message, 1, 64 ).TrimEnd();

            if ( text.StartsWith( "/" ) ) {
                text = text.Remove( 0, 1 );
                string cmd = text.Split( ' ' )[0];

                HandleCommand(cmd, text.Substring(cmd.Length).Trim());
                return;
            } else if ( text.StartsWith( "#" ) ) {
                text = text.Remove( 0, 1 );
                if ( isMuted ) {
                    SendMessage( "&cYou're muted. You can't talk." );
                    return;
                }
                Server.Log( "# " + Name + ": " + text );
                GlobalMessage( this, text, true );
                return;
            }

            if ( isMuted ) {
                SendMessage( "&cYou're muted. You can't talk." );
                return;
            }

            Server.Log( "<" + Level.Name + "> " + Name + ": " + text ); 
            Message( this, Level, text, true );
        }

        void HandleCommand( string cmd, string args ) {
            Command comm = Command.Find( cmd );

            if ( comm != null ) {
                if ( Rank.Commands.Contains( comm ) ) {
                    Server.Log( Name + " has used the command /" + cmd + " " + args + "..." );
                    try {
                        comm.Use( this, args );
                    } catch ( Exception e ) {
                        Server.Log( e.ToString() );
                        SendMessage( "&cCommand failed, please contact an admin." );
                    }
                } else {
                    SendMessage( "&cYou are not permitted to use this command." );
                }
            } else {
                SendMessage( "&cCommand &f/" + cmd + "&c was not found." );
            }
        }

        void HandleExtInfo( byte[] message ) {
            string appName = enc.GetString( message, 0, 64 );
            byte extCount = message[65];

            Server.Log( appName.Trim() + "; extCount: " + extCount );
        }

        void HandleExtEntry( byte[] message ) {
            Server.Log( "RECIEVED EXTENTRY!!!" );
        }

        #region Outgoing

        public void SendPacket( Packet p ) {
            SendPacket( p, new byte[0] );
        }

        public void SendPacket( Packet p, byte[] message ) {
            byte[] buffer = new byte[message.Length + 1];
            buffer[0] = (byte)p.GetHashCode();
            Buffer.BlockCopy( message, 0, buffer, 1, message.Length );

            if ( !hasDisconnected ) {
                try {
                    this.Socket.BeginSend( buffer, 0, buffer.Length, SocketFlags.None, delegate( IAsyncResult result ) { }, null );
                } catch ( Exception e ) {
                    Disconnect();
                }
            }
        }

        public void SendServerIdentification() {
            byte[] serverData = new byte[130];
            serverData[0] = (byte)7;
            StringFormat( Config.Name, 64 ).CopyTo( serverData, 1 );
            StringFormat( Config.MOTD, 64 ).CopyTo( serverData, 65 );

            if ( Rank.CanBreakBedrock ) {
                serverData[129] = (byte)100;
            } else {
                serverData[129] = (byte)0;
            }

            SendPacket( Packet.ServerIdentification, serverData );
        }

        public void SendPing() {
            SendPacket( Packet.Ping );
        }

        public void SendLevel() {
            SendServerIdentification();
            Thread.Sleep( 100 );
            SendLevelInitialize();
            Thread.Sleep( 100 );
            SendLevelDataChunk();
            Thread.Sleep( 100 );
            SendLevelFinalize();

            Player.GlobalSpawn( this,
                                (ushort)( ( 0.5 + Level.SpawnX ) * 32 ),
                                (ushort)( ( 1 + Level.SpawnY ) * 32 ),
                                (ushort)( ( 0.5 + Level.SpawnZ ) * 32 ),
                                Level.SpawnRX,
                                Level.SpawnRY );

            Player.PlayerList.ForEach( delegate( Player pl ) {
                if ( pl.Level == Level && pl != this ) {
                    SendSpawnPlayer( pl.ID, pl.Rank.Color + pl.Name, pl.Pos[0], pl.Pos[1], pl.Pos[2], pl.Rot[0], pl.Rot[1] );
                }
            } );
        }

        public void SendLevelInitialize() {
            SendPacket( Packet.LevelInitialize );
        }

        public void SendLevelDataChunk() {
            byte[] buffer = new byte[this.Level.Blocks.Length + 4];
            BitConverter.GetBytes( IPAddress.HostToNetworkOrder( this.Level.Blocks.Length ) ).CopyTo( buffer, 0 );
            for ( int i = 0; i < this.Level.Blocks.Length; ++i ) {
                buffer[4 + i] = Block.Convert(this.Level.Blocks[i]);
            }
            buffer = GZip( buffer );
            int number = (int)Math.Ceiling( ( (double)buffer.Length ) / 1024 );
            for ( int i = 1; buffer.Length > 0; ++i ) {
                ushort length = (ushort)Math.Min( buffer.Length, 1024 );
                byte[] send = new byte[1027];
                HostToNetworkOrder( length ).CopyTo( send, 0 );
                Buffer.BlockCopy( buffer, 0, send, 2, length );
                byte[] tempbuffer = new byte[buffer.Length - length];
                Buffer.BlockCopy( buffer, length, tempbuffer, 0, buffer.Length - length );
                buffer = tempbuffer;
                send[1026] = (byte)( i * 100 / number );
                SendPacket( Packet.LevelDataChunk, send );
                Thread.Sleep( 10 );
            }
        }

        public void SendLevelFinalize() {
            buffer = new byte[6];
            HostToNetworkOrder( (ushort)this.Level.Width ).CopyTo( buffer, 0 );
            HostToNetworkOrder( (ushort)this.Level.Height ).CopyTo( buffer, 2 );
            HostToNetworkOrder( (ushort)this.Level.Depth ).CopyTo( buffer, 4 );
            SendPacket( Packet.LevelFinalize, buffer );
        }

        public void SendSetBlock(ushort x, ushort y, ushort z, byte type) {
            byte[] blockData = new byte[7];
            HostToNetworkOrder( x ).CopyTo( blockData, 0 );
            HostToNetworkOrder( y ).CopyTo( blockData, 2 );
            HostToNetworkOrder( z ).CopyTo( blockData, 4 );
            blockData[6] = Block.Convert( type );
            SendPacket( Packet.SetBlock, blockData );
        }
        
        public void SendSpawnPlayer(byte id, string name, ushort x, ushort y, ushort z, byte rx, byte ry) {
            byte[] playerData = new byte[73];
            playerData[0] = id;
            StringFormat( name, 64 ).CopyTo( playerData, 1 );
            HostToNetworkOrder( x ).CopyTo( playerData, 65 );
            HostToNetworkOrder( y ).CopyTo( playerData, 67 );
            HostToNetworkOrder( z ).CopyTo( playerData, 69 );
            playerData[71] = rx;
            playerData[72] = ry;
            SendPacket( Packet.SpawnPlayer, playerData );
        }

        public void SendDespawnPlayer( byte id ) {
            byte[] playerData = new byte[1];
            playerData[0] = id;
            SendPacket( Packet.DespawnPlayer, playerData );
        }

        public void SendMessage( string message ) {
            unchecked {
                SendMessage( ID, "&e" + message);
            }
        }

        public void SendMessage( byte id, string message ) {
            byte[] messageData = new byte[65];
            messageData[0] = id;

            message = message.Replace( '%', '&' );
            message = message.Trim();

            foreach ( string line in WordWrap( message ) ) {
                StringFormat( line, 64 ).CopyTo( messageData, 1 );
                SendPacket( Packet.Message, messageData );
            }
        }

        public void SendKick( string reason ) {
            SendPacket( Packet.DisconnectPlayer, StringFormat(reason, 64) );
        }

        public void SendExtInfo(short extCount) {
            byte[] extData = new byte[66];
            Encoding.ASCII.GetBytes( "ClassiCraft 1.0".PadRight( 64 ), 0, 64, extData, 0 );
            ToNetOrder( extCount, extData, 64 );
            SendPacket( Packet.ExtInfo, extData );
        }

        public void SendExtEntry(string extension, short version) {
            byte[] extData = new byte[68];
            Encoding.ASCII.GetBytes( extension.PadRight( 64 ), 0, 64, extData, 0 );
            ToNetOrder( version, extData, 64 );
            SendPacket( Packet.ExtEntry, extData );
        }

        public void SendClickDistance( short distance ) {
            byte[] clickData = new byte[2];
            ToNetOrder( distance, clickData, 0 );
            SendPacket( Packet.ClickDistance, clickData );
        }

        public void SendCustomBlockSupportLevel(byte level) {
            byte[] levelData = new byte[1];
            levelData[0] = level;
            SendPacket( Packet.CustomBlockSupportLevel, levelData );
        }

        public void SendEnvSetWeatherType( byte weatherType ) {
            byte[] weatherData = new byte[1];
            weatherData[0] = weatherType;
            SendPacket( Packet.EnvSetWeatherType, weatherData );
        }

        static void ToNetOrder( short number, byte[] arr, int offset ) {
            if ( arr == null ) throw new ArgumentNullException( "arr" );
            arr[offset] = (byte)( ( number & 0xff00 ) >> 8 );
            arr[offset + 1] = (byte)( number & 0x00ff );
        }


        static void ToNetOrder( int number, byte[] arr, int offset ) {
            if ( arr == null ) throw new ArgumentNullException( "arr" );
            arr[offset] = (byte)( ( number & 0xff000000 ) >> 24 );
            arr[offset + 1] = (byte)( ( number & 0x00ff0000 ) >> 16 );
            arr[offset + 2] = (byte)( ( number & 0x0000ff00 ) >> 8 );
            arr[offset + 3] = (byte)( number & 0x000000ff );
        }

        void UpdatePosition() {
            byte changed = 0; 

            if ( OldPos[0] != Pos[0] || OldPos[1] != Pos[1] || OldPos[2] != Pos[2] ) {
                changed |= 1;
            }
            if ( OldRot[0] != Rot[0] || OldRot[1] != Rot[1] ) {
                changed |= 2;
            }
            if ( Math.Abs( Pos[0] - BasePos[0] ) > 32 || Math.Abs( Pos[1] - BasePos[1] ) > 32 || Math.Abs( Pos[2] - BasePos[2] ) > 32 ) {
                changed |= 4;
            }
            if ( ( OldPos[0] == Pos[0] && OldPos[1] == Pos[1] && OldPos[2] == Pos[2] ) && ( BasePos[0] != Pos[0] || BasePos[1] != Pos[1] || BasePos[2] != Pos[2] ) ) {
                changed |= 4;
            }

            byte[] buffer = new byte[0]; 
            Packet msg = (Packet)0;

            if ( ( changed & 4 ) != 0 ) {
                msg = Packet.PositionOrientationTeleport; //Player teleport - used for spawning or moving too fast
                buffer = new byte[9];
                buffer[0] = ID;
                HostToNetworkOrder( Pos[0] ).CopyTo( buffer, 1 );
                HostToNetworkOrder( Pos[1] ).CopyTo( buffer, 3 );
                HostToNetworkOrder( Pos[2] ).CopyTo( buffer, 5 );
                buffer[7] = Rot[0]; 
                buffer[8] = Rot[1];
            } else if ( changed == 1 ) {
                try {
                    msg = Packet.PositionUpdate; //Position update
                    buffer = new byte[4]; 
                    buffer[0] = ID;
                    Buffer.BlockCopy( System.BitConverter.GetBytes( (sbyte)( Pos[0] - OldPos[0] ) ), 0, buffer, 1, 1 );
                    Buffer.BlockCopy( System.BitConverter.GetBytes( (sbyte)( Pos[1] - OldPos[1] ) ), 0, buffer, 2, 1 );
                    Buffer.BlockCopy( System.BitConverter.GetBytes( (sbyte)( Pos[2] - OldPos[2] ) ), 0, buffer, 3, 1 );
                } catch {

                }
            } else if ( changed == 2 ) {
                msg = Packet.OrientationUpdate; //Orientation update
                buffer = new byte[3]; 
                buffer[0] = ID;
                buffer[1] = Rot[0]; 
                buffer[2] = Rot[1];
            } else if ( changed == 3 ) {
                try {
                    msg = Packet.PositionOrientationUpdate; //Position and orientation update
                    buffer = new byte[6]; buffer[0] = ID;
                    Buffer.BlockCopy( System.BitConverter.GetBytes( (sbyte)( Pos[0] - OldPos[0] ) ), 0, buffer, 1, 1 );
                    Buffer.BlockCopy( System.BitConverter.GetBytes( (sbyte)( Pos[1] - OldPos[1] ) ), 0, buffer, 2, 1 );
                    Buffer.BlockCopy( System.BitConverter.GetBytes( (sbyte)( Pos[2] - OldPos[2] ) ), 0, buffer, 3, 1 );
                    buffer[4] = Rot[0]; 
                    buffer[5] = Rot[1];
                } catch {

                }
            }

            if ( changed != 0 ) {
                PlayerList.ForEach( delegate( Player p ) {
                    if ( p != this && p.Level == Level ) {
                        p.SendPacket( msg, buffer );
                    }
                } );
            }

            OldPos = Pos; 
            OldRot = Rot;
        }


        public static void GlobalSpawn( Player player, ushort x, ushort y, ushort z, byte rotx, byte roty ) {
            PlayerList.ForEach( delegate( Player p ) {
                if ( p != player && p.Level == player.Level ) {
                    p.SendSpawnPlayer( player.ID, player.Rank.Color + player.Name, x, y, z, rotx, roty );
                }
            } );

            player.Pos = new ushort[3] { x, y, z };
            player.Rot = new byte[2] { rotx, roty };
            player.OldPos = player.Pos;
            player.BasePos = player.Pos;
            player.OldRot = player.Rot;

            unchecked {
                player.SendSpawnPlayer( (byte)-1, player.Rank.Color + player.Name, x, y, z, rotx, roty );
            }
        }

        public static void GlobalDespawn( Player player ) {
            PlayerList.ForEach( delegate( Player p ) {
                if ( p != player ) {
                    p.SendDespawnPlayer( player.ID );
                }
            } );
        }

        public static void GlobalMessage( string message ) {
            GlobalMessage( null, message, false );
        }

        public static void GlobalMessage( Player from, string message, bool isChat = false ) {
            if ( isChat ) {
                PlayerList.ForEach( delegate( Player p ) {
                    p.SendMessage( "&c# " + from.Rank.Color + from.NamePrefix + " " + from.Name + from.NameSuffix + "&f: " + message );
                } );
            } else {
                PlayerList.ForEach( delegate( Player p ) {
                    p.SendMessage( message );
                } );
            }
        }

        public static void GlobalAnnouncement( string message ) {
            Player.PlayerList.ForEach( delegate( Player p ) {
                if ( p.isCPECapable ) {
                    p.SendMessage( 100, message );
                }
            } );
        }

        public static void Message( Level to, string message ) {
            Message( null, to, message, false );
        }

        public static void Message( Player from, Level to, string message, bool isChat = false ) {
            string prefix = "";

            if ( isChat ) {
                if ( from.NamePrefix != "" ) {
                    prefix = from.NamePrefix + " ";
                }

                PlayerList.ForEach( delegate( Player p ) {
                    if ( p.Level == to ) {
                        p.SendMessage( from.Rank.Color + prefix + from.Name + from.NameSuffix + "&f: " + message );
                    }
                } );
            } else {
                PlayerList.ForEach( delegate( Player p ) {
                    if ( p.Level == to ) {
                        p.SendMessage( message );
                    }
                } );
            }
        }

        #endregion

        #region Disconnecting

        public void Disconnect() {
            if ( hasDisconnected ) {
                if ( ConnectionList.Contains( this ) ) {
                    ConnectionList.Remove( this );
                    return;
                }
            }

            hasDisconnected = true;
            pingTimer.Stop();
            PlayerDB.Save( this );
            SendKick( "You've been disconnected." );

            if ( isLoggedIn ) {
                GlobalDespawn( this );
                Server.Log( "<-- " + Name + " disconnected (IP: " + IP + ")..." );
                GlobalMessage( this, "Player '" + Rank.Color + Name + "&e' logged out. (Disconnected)" );
                PlayerList.Remove( this );
            } else {
                Server.Log( IP + " disconnected..." );
                ConnectionList.Remove( this );
            }
        }

        public void Kick(string message = "You've been kicked.") {
            if ( hasDisconnected ) {
                if ( ConnectionList.Contains( this ) ) {
                    ConnectionList.Remove( this );
                    return;
                }
            }

            pingTimer.Stop();
            PlayerDB.Save( this );
            SendKick( message );

            if ( isLoggedIn ) {
                Server.Log( "<-- " + Name + " was kicked \"" + message + "\" (IP: " + IP + ")..." );
                GlobalMessage( this, "Player '" + Rank.Color + Name + "&e' was kicked. (" + message + ")" );
                GlobalDespawn( this );
                PlayerList.Remove( this );
            } else {
                Server.Log( IP + " was kicked \"" + message + "\"..." );
                ConnectionList.Remove( this );
            }

            hasDisconnected = true;
        }

        #endregion

        #region Other

        public void Reward( int coins, string reason = "" ) {
            Coins += coins;
            //SendMessage( "----------------------------------" );
            SendMessage( "&bYou were rewarded &e" + coins + "&b coins!" );
            //if(reason != "") {
            //SendMessage( "&bReason: &e" + reason );
            //}
            //SendMessage( "&bYou now have a total of: &e" + Coins + " &bcoins!" );
            //SendMessage( "----------------------------------" );
            Server.Log( Name + " was rewarded " + coins + " for: " + reason + "..." );
            PlayerDB.Save(this);
        }

        public void Die() {
            if ( OnDeath != null ) {
                OnDeath(this);
            }
        }

        public static List<string> WordWrap( string message ) {
            List<string> lines = new List<string>();
            string validColorChars = "0123456789abcdef";
            string lastColor = "";

            message = message.Trim();

            for ( int i = 0; i < message.Length; i++ ) {
                char thisChar = message[i];
                if ( thisChar == '&' ) {
                    try {
                        if ( message[i + 2] == '&' ) {
                            message = message.Remove( i, 2 );
                            i = i - 1;
                        }
                    } catch { }
                }
            }

        next:

            if ( message.Length <= 64 ) {
                lines.Add( message );
                return lines;
            }

            try {
                for ( int i = 0; i <= 64; i++ ) {
                    if ( message[i] == '&' && validColorChars.Contains( message[i + 1].ToString() ) ) {
                        lastColor = message.Substring( i, 2 );
                    }

                    if ( i == 64 ) {
                        if ( message[i] == ' ' ) {
                            lines.Add( message.Substring( 0, 64 ) );
                            message = ">" + lastColor + " " + message.Remove( 0, 64 ).Trim();
                        } else {
                            int breakIndex = message.Substring( 0, 64 ).LastIndexOf( ' ' );
                            lines.Add( message.Substring( 0, breakIndex ) );
                            message = ">" + lastColor + " " + message.Remove( 0, breakIndex ).Trim();
                        }
                    }
                }
            } catch {
                Server.Log( "ERROR!!!!!" );
            }

            goto next;
        }

        public static bool ValidName( string name ) {
            string allowedchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890._@";
            foreach ( char ch in name ) {
                if ( allowedchars.IndexOf( ch ) == -1 ) {
                    return false;
                }
            }
            return true;
        }

        public static Player Find( string name ) {
            foreach ( Player p in PlayerList ) {
                if ( p.Name.ToLower().Contains(name.ToLower()) ) {
                    return p;
                }
            }
            return null;
        }

        static byte[] StringFormat( string str, int size ) {
            byte[] bytes = new byte[size];
            bytes = enc.GetBytes( str.PadRight( size ).Substring( 0, size ) );
            return bytes;
        }

        static byte GetID() {
            for ( byte i = 0; i < 255; i++ ) {
                bool used = false;

                foreach ( Player p in PlayerList ) {
                    if ( p.ID == i ) {
                        used = true;
                    }
                }

                if ( !used ) {
                    return i;
                }
            }
            return (byte)1;
        }

        #endregion

        #region Host <> Network

        byte[] HostToNetworkOrder( ushort x ) {
            byte[] y = BitConverter.GetBytes( x ); 
            Array.Reverse( y ); 
            return y;
        }

        ushort NetworkToHostOrder( byte[] x, int offset ) {
            byte[] y = new byte[2];
            Buffer.BlockCopy( x, offset, y, 0, 2 ); Array.Reverse( y );
            return BitConverter.ToUInt16( y, 0 );
        }

        byte[] HostToNetworkOrder( short x ) {
            byte[] y = BitConverter.GetBytes( x ); 
            Array.Reverse( y ); 
            return y;
        }

        public static byte[] GZip( byte[] bytes ) {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            GZipStream gs = new GZipStream( ms, CompressionMode.Compress, true );
            gs.Write( bytes, 0, bytes.Length );
            gs.Close();
            ms.Position = 0;
            bytes = new byte[ms.Length];
            ms.Read( bytes, 0, (int)ms.Length );
            ms.Close();
            return bytes;
        }

        #endregion
    }
}
