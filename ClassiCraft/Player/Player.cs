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

        public System.Timers.Timer pingTimer = new System.Timers.Timer();
        public System.Timers.Timer drownTimer = new System.Timers.Timer();
        byte Version;
        byte MagicNumber;
        string Key;

        public string Name;
        public string NamePrefix = "";
        public string NameSuffix = "";
        public string IP;
        public byte ID;
        public Rank Rank;
        public Level Level = Server.mainLevel;

        Socket Socket;
        byte[] buffer = new byte[0];
        byte[] tempBuffer = new byte[255];
        byte[] Bindings = new byte[128];

        public bool hasDisconnected;
        public bool isLoggedIn;
        public bool isCPECapable;

        public ushort[] Pos = new ushort[3] { 0, 0, 0 };
        public ushort[] OldPos = new ushort[3] { 0, 0, 0 };
        public ushort[] BasePos = new ushort[3] { 0, 0, 0 };
        public byte[] Rot = new byte[2] { 0, 0 };
        public byte[] OldRot = new byte[2] { 0, 0 };

        public delegate void BlockchangeEventHandler( Player p, ushort x, ushort y, ushort z, byte type );
        public event BlockchangeEventHandler OnBlockChange = null;
        public void ClearBlockChange() { OnBlockChange = null; }

        public Player( Socket socket ) {
            Socket = socket;
            IP = socket.RemoteEndPoint.ToString().Split( ':' )[0];
            Server.Log( IP + " connected...");

            for ( byte i = 0; i < 128; ++i ) {
                Bindings[i] = i;
            }

            socket.BeginReceive( tempBuffer, 0, tempBuffer.Length, SocketFlags.None, new AsyncCallback( recievePlayer ), this );

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
                    default:
                        SendKick( "Unhandled message ID" );
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
                NameSuffix = "+";
            }

            ID = GetID();
            PlayerDB.Load( this );
            PlayerList.Add( this );
            isLoggedIn = true;
            SendLevel();

            if ( Name.ToLower() == "marvy" || Name.ToLower() == "herocane" ) {
                Rank = Rank.Find( PermissionLevel.Owner );
            }

            Server.Log( "--> " + Name + " has joined (IP: " + IP + ")..." );
            GlobalMessage( this, "Player '" + Rank.Color + Name + "&e' logged in." );
            
            foreach ( string line in Server.welcomeMessage ) {
                SendMessage( line );
            }

            ushort x = (ushort)( ( 0.5 + Level.SpawnX ) * 32 );
            ushort y = (ushort)( ( 1 + Level.SpawnY ) * 32 );
            ushort z = (ushort)( ( 0.5 + Level.SpawnZ ) * 32 );
            Pos = new ushort[3] { x, y, z }; 
            Rot = new byte[2] { Level.SpawnRX, Level.SpawnRY };

            GlobalSpawn( this, x, y, z, Rot[0], Rot[1] );

            PlayerList.ForEach( delegate( Player p ) {
                if ( p.Level == Level && p != this ) {
                    SendSpawnPlayer( p.ID, p.Rank.Color + p.Name, p.Pos[0], p.Pos[1], p.Pos[2], p.Rot[0], p.Rot[1] );
                }
            } );

            drownTimer.Start();
        }

        void HandleBlockchange( byte[] message ) {
            ushort x = NetworkToHostOrder( message, 0 );
            ushort y = NetworkToHostOrder( message, 2 );
            ushort z = NetworkToHostOrder( message, 4 );
            byte mode = message[6];
            byte type = message[7];

            if ( Level.BuildPermission > Rank.Permission ) {
                byte b = Level.GetBlock( x, y, z );
                SendSetBlock( x, y, z, b );
                SendMessage( "&cThis level is reserved for " + Rank.Find(Level.BuildPermission).Color + Rank.Find(Level.BuildPermission).Name + "&c." );
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

            if ( mode == 0 ) {
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
                Server.Log( "# " + Name + ": " + text );
                GlobalMessage( this, text, true );
                return;
            }
                
            Server.Log( "<" + Level.Name + "> " + Name + ": " + text );
            Message( this, text, true );
        }

        void HandleCommand( string cmd, string args ) {
            Command comm = Command.Find( cmd );

            if ( comm != null ) {
                if ( comm.DefaultPerm <= Rank.Permission ) {
                    comm.Use( this, args );
                    Server.Log( Name + " has used the command /" + cmd + " " + args + "..." );
                } else {
                    SendMessage( "&cYou are not permitted to use this command." );
                }
            } else {
                SendMessage( "&cCommand &f/" + cmd + "&c was not found." );
            }
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
            SendLevelInitialize();
            SendLevelDataChunk();
            SendLevelFinalize();
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

            foreach ( string line in WordWrap( message ) ) {
                StringFormat( line, 64 ).CopyTo( messageData, 1 );
                SendPacket( Packet.Message, messageData );
            }
        }

        public void SendKick( string reason ) {
            SendPacket( Packet.DisconnectPlayer, StringFormat(reason, 64) );
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
                    p.SendMessage( "&c# " + from.Rank.Color + from.NamePrefix + from.Name + from.NameSuffix + "&f: " + message );
                } );
            } else {
                PlayerList.ForEach( delegate( Player p ) {
                    p.SendMessage( message );
                } );
            }
        }

        public static void Message( Player from, string message, bool isChat = false ) {
            if ( isChat ) {
                PlayerList.ForEach( delegate( Player p ) {
                    if ( p.Level == from.Level ) {
                        p.SendMessage( from.Rank.Color + from.NamePrefix + from.Name + from.NameSuffix + "&f: " + message );
                    }
                } );
            } else {
                PlayerList.ForEach( delegate( Player p ) {
                    if ( p.Level == from.Level ) {
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

        public static List<string> WordWrap( string message ) {
            List<string> lines = new List<string>();
            string validColorChars = "0123456789abcdef";
            string lastColor = "";

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

            for ( int i = 0; i <= 64; i++ ) {
                if ( message[i] == '&' && validColorChars.Contains( message[i + 1].ToString() ) ) {
                    lastColor = message.Substring( i, 2 );
                }

                if ( i == 64 ) {
                    if ( message[i] == ' ' ) {
                        lines.Add( message.Substring( 0, 64 ) );
                        message = ">" + lastColor + " " + message.Remove( 0, 64 ).TrimStart();
                    } else {
                        int breakIndex = message.Substring( 0, 64 ).LastIndexOf( ' ' );
                        lines.Add( message.Substring( 0, breakIndex ) );
                        message = ">" + lastColor + " " + message.Remove( 0, breakIndex ).TrimStart();
                    }
                }
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
            byte[] y = BitConverter.GetBytes( x ); Array.Reverse( y ); return y;
        }

        ushort NetworkToHostOrder( byte[] x, int offset ) {
            byte[] y = new byte[2];
            Buffer.BlockCopy( x, offset, y, 0, 2 ); Array.Reverse( y );
            return BitConverter.ToUInt16( y, 0 );
        }

        byte[] HostToNetworkOrder( short x ) {
            byte[] y = BitConverter.GetBytes( x ); Array.Reverse( y ); return y;
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
