using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public sealed partial class Level {
        public static List<Level> LevelList = new List<Level>();

        public string Name;
        public ushort Width;
        public ushort Height;
        public ushort Depth;
        public byte[] Blocks;

        public ushort SpawnX;
        public ushort SpawnY;
        public ushort SpawnZ;
        public byte SpawnRX;
        public byte SpawnRY;

        public PermissionLevel JoinPermission;
        public PermissionLevel BuildPermission;

        public List<BufferPos> TempBuffers;

        public bool hasChanged = false;
        public bool isHostingGame = false;
        public bool enableEditing = true;

        public Level( string name, ushort x, ushort y, ushort z ) {
            Name = name;
            Width = x;
            Height = y;
            Depth = z;
            Blocks = new byte[Width * Height * Depth];

            for ( ushort xx = 0; xx < Width; xx++ ) {
                for ( ushort yy = 0; yy < Height; yy++ ) {
                    for ( ushort zz = 0; zz < Depth; zz++ ) {
                        ushort half = (ushort)( Height / 2 );
                        if ( yy < half ) {
                            if ( yy <= 1 ) {
                                SetBlock( xx, yy, zz, Block.Lava );
                            } else {
                                SetBlock( xx, yy, zz, Block.Dirt );
                            }
                        } else if ( yy == half ) {
                            SetBlock( xx, yy, zz, Block.Grass );
                        } else {
                            SetBlock( xx, yy, zz, Block.Air );
                        }
                    }
                }
            }

            SpawnX = (ushort)( Width / 2 );
            SpawnY = (ushort)( Height / 0.65f );
            SpawnZ = (ushort)( Depth / 2 );
            SpawnRX = 0; 
            SpawnRY = 0;

            BufferPos bpos = new BufferPos();

            for ( ushort yy = 0; yy < Height; yy++ ) {
                if ( GetBlock( SpawnX, yy, SpawnZ ) != Block.Air ) {
                    bpos = new BufferPos( SpawnX, yy, SpawnZ, GetBlock( SpawnX, yy, SpawnZ ) );
                }
            }

            SpawnY = (ushort)(bpos.Y + 2);

            LevelList.Add( this );
        }

        public void SetBlock( ushort x, ushort y, ushort z, byte type ) {
            try {
                Blocks[x + Width * z + Width * Depth * y] = type;
            } catch {
                // do nothing.
            }
        }

        public byte GetBlock( ushort x, ushort y, ushort z ) {
            try {
                return Blocks[x + ( z * Width ) + ( y * Width * Depth )];
            } catch {
                return 0;
            }
        }

        public int PosToInt( ushort x, ushort y, ushort z ) {
            return x + ( z * Width ) + ( y * Width * Depth );
        }

        public void Blockchange( ushort x, ushort y, ushort z, byte type ) {
            if ( GetBlock( x, y, z ) != type ) {
                Player.PlayerList.ForEach( delegate( Player p ) {
                    if ( p.Level == this ) {
                        p.SendSetBlock( x, y, z, type );
                    }
                } );

                SetBlock( x, y, z, type );
                hasChanged = true;
            }
        }

        public void Blockchange( Player player, ushort x, ushort y, ushort z, byte type ) {
            foreach ( Player p in Player.PlayerList ) {
                if ( p.Level == this ) {
                    p.SendSetBlock( x, y, z, type );
                }
            }
            SetBlock( x, y, z, type );
            hasChanged = true;
        }

        public static Level Load( string name ) {
            if ( !System.IO.File.Exists( "levels/" + name.ToLower() + ".lvl" ) ) {
                return null;
            }

            Level level;
            System.IO.FileStream fs = System.IO.File.OpenRead( "levels/" + name.ToLower() + ".lvl" );
            System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream( fs, System.IO.Compression.CompressionMode.Decompress );

            try {
                byte[] header = new byte[81];
                gs.Read( header, 0, header.Length );

                string lvlname = Encoding.ASCII.GetString( header, 0, 64 ).Trim('\0');
                ushort width = BitConverter.ToUInt16( header, 65 );
                ushort height = BitConverter.ToUInt16( header, 67 );
                ushort depth = BitConverter.ToUInt16( header, 69 );
                ushort spawnx = BitConverter.ToUInt16( header, 71 );
                ushort spawny = BitConverter.ToUInt16( header, 73 );
                ushort spawnz = BitConverter.ToUInt16( header, 75 );
                byte spawnrx = header[77];
                byte spawnry = header[78];
                PermissionLevel joinpermission = (PermissionLevel)header[79];
                PermissionLevel buildpermission = (PermissionLevel)header[80];

                level = new Level( name, width, height, depth );
                level.Name = lvlname;
                level.SpawnX = spawnx;
                level.SpawnY = spawny;
                level.SpawnZ = spawnz;
                level.SpawnRX = spawnrx;
                level.SpawnRY = spawnry;
                level.JoinPermission = joinpermission;
                level.BuildPermission = buildpermission;

                byte[] blocks = new byte[level.Width * level.Depth * level.Height];
                gs.Read( blocks, 0, blocks.Length );
                level.Blocks = blocks;
                gs.Close();

                Server.Log( "Loaded level \"" + name + "\"..." );
                return level;
            } catch ( Exception e ) {
                Server.Log( "Error loading level \"" + name + "\" (" + e.ToString() + ")..." );
                return null;
            }
        }

        public bool Save() {
            if ( !System.IO.Directory.Exists( "levels" ) ) {
                System.IO.Directory.CreateDirectory( "levels" );
            }

            System.IO.FileStream fs = System.IO.File.Create( "levels/" + Name.ToLower() + ".lvl" );
            System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream( fs, System.IO.Compression.CompressionMode.Compress );

            try {
                byte[] header = new byte[81];
                Encoding.ASCII.GetBytes( Name ).CopyTo( header, 0 );
                BitConverter.GetBytes( Width ).CopyTo( header, 65 );
                BitConverter.GetBytes( Height ).CopyTo( header, 67 );
                BitConverter.GetBytes( Depth ).CopyTo( header, 69 );
                BitConverter.GetBytes( SpawnX ).CopyTo( header, 71 );
                BitConverter.GetBytes( SpawnY ).CopyTo( header, 73 );
                BitConverter.GetBytes( SpawnZ ).CopyTo( header, 75 );
                header[77] = SpawnRX;
                header[78] = SpawnRY;
                header[79] = (byte)JoinPermission;
                header[80] = (byte)BuildPermission;

                gs.Write( header, 0, header.Length );
                gs.Write( Blocks, 0, Blocks.Length );
                gs.Close();
                fs.Close();

                Server.Log( "Saved level \"" + Name + "\" to " + Name.ToLower() + ".lvl..." );
                hasChanged = false;
                return true;
            } catch ( Exception e ) {
                Server.Log( "Failed to save level \"" + Name + "\" (" + e.ToString() + ")..." );
                return false;
            }
        }

        public static Level Find( string name ) {
            foreach ( Level l in LevelList ) {
                if ( l.Name.ToLower().Contains(name.ToLower()) ) {
                    return l;
                }
            }
            return null;
        }
    }

    public enum PermissionLevel {
        Guest = 0,
        Builder = 20,
        AdvBuilder = 40,
        Operator = 80,
        SuperOp = 160,
        Owner = 320
    }
}
