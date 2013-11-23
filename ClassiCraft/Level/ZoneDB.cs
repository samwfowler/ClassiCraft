using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class ZoneDB {
        public static List<Zone> ZoneList = new List<Zone>();
        public static string DBFile = "ZoneDB.db";

        public static void LoadZones() {
            if ( File.Exists( DBFile ) ) {
                Zone loadedZone;
                foreach ( string line in File.ReadAllLines( DBFile ) ) {
                    if ( !string.IsNullOrEmpty( line ) && line[0] != '#' ) {
                        try {
                            string level = line.Split( ':' )[0].Trim();
                            ushort x1 = (ushort)int.Parse( line.Split( ':' )[1].Trim() );
                            ushort x2 = (ushort)int.Parse( line.Split( ':' )[2].Trim() );
                            ushort y1 = (ushort)int.Parse( line.Split( ':' )[3].Trim() );
                            ushort y2 = (ushort)int.Parse( line.Split( ':' )[4].Trim() );
                            ushort z1 = (ushort)int.Parse( line.Split( ':' )[5].Trim() );
                            ushort z2 = (ushort)int.Parse( line.Split( ':' )[6].Trim() );
                            PermissionLevel perm = (PermissionLevel)int.Parse( line.Split( ':' )[7].Trim() );
                            loadedZone = new Zone( level, x1, x2, y1, y2, z1, z2, perm );
                        } catch { }
                    }
                }
            } else {
                SaveZones();
            }

            Server.Log( "Loaded ZoneDB..." );
        }

        public static void SaveZones() {
            StreamWriter sw = new StreamWriter( File.Create( DBFile ) );
            foreach ( Zone z in ZoneList ) {
                sw.WriteLine( z.Level.Name + " : " + 
                    z.x1 + " : " + 
                    z.x2 + " : " + 
                    z.y1 + " : " + 
                    z.y2 + " : " + 
                    z.z1 + " : " + 
                    z.z2 + " : " + 
                    z.Permission.GetHashCode() );
            }
            sw.Flush();
            sw.Close();
            sw.Dispose();

            Server.Log( "Saved ZoneDB..." );
        }
    }

    public struct Zone {
        public Level Level;
        public ushort x1, x2, y1, y2, z1, z2;
        public PermissionLevel Permission;

        public Zone( string level, ushort x, ushort xx, ushort y, ushort yy, ushort z, ushort zz, PermissionLevel perm ) {
            Level = Level.Find(level);
            x1 = x;
            x2 = xx;
            y1 = y;
            y2 = yy;
            z1 = z;
            z2 = zz;
            Permission = perm;

            ZoneDB.ZoneList.Add( this );
        }
    }
}
