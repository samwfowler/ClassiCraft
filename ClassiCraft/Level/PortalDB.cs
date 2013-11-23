using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class PortalDB {
        public static List<Portal> PortalList = new List<Portal>();
        public static string DBFile = "PortalDB.db";

        public static void LoadPortals() {
            if ( File.Exists( DBFile ) ) {
                Portal loadedPortal;
                foreach ( string line in File.ReadAllLines( DBFile ) ) {
                    if ( !string.IsNullOrEmpty( line ) && line[0] != '#' ) {
                        try {
                            string level = line.Split( ':' )[0].Trim();
                            string destination = line.Split( ':' )[1].Trim();
                            ushort x1 = (ushort)int.Parse( line.Split( ':' )[2].Trim() );
                            ushort x2 = (ushort)int.Parse( line.Split( ':' )[3].Trim() );
                            ushort y1 = (ushort)int.Parse( line.Split( ':' )[4].Trim() );
                            ushort y2 = (ushort)int.Parse( line.Split( ':' )[5].Trim() );
                            ushort z1 = (ushort)int.Parse( line.Split( ':' )[6].Trim() );
                            ushort z2 = (ushort)int.Parse( line.Split( ':' )[7].Trim() );
                            loadedPortal = new Portal( level, destination, x1, x2, y1, y2, z1, z2 );
                        } catch {

                        }
                    }
                }
            } else {
                SavePortals();
            }

            Server.Log( "Loaded PortalDB..." );
        }

        public static void SavePortals() {
            try {
                StreamWriter sw = new StreamWriter( File.Create( DBFile ) );
                foreach ( Portal p in PortalList ) {
                    sw.WriteLine( p.Level.Name + " : " + 
                        p.Destination.Name + " : " + 
                        p.x1 + " : " + 
                        p.x2 + " : " + 
                        p.y1 + " : " + 
                        p.y2 + " : " + 
                        p.z1 + " : " + 
                        p.z2 );
                }
                sw.Flush();
                sw.Close();
                sw.Dispose();

                Server.Log( "Saved PortalDB..." );
            } catch ( Exception e ) {
                Server.Log( "ERROR: " + e.ToString() );
            }
        }
    }

    public struct Portal {
        public Level Level;
        public Level Destination;
        public ushort x1, x2, y1, y2, z1, z2;

        public Portal( string level, string destination, ushort x, ushort xx, ushort y, ushort yy, ushort z, ushort zz ) {
            Level = Level.Find( level );
            Destination = Level.Find( destination );
            x1 = x;
            x2 = xx;
            y1 = y;
            y2 = yy;
            z1 = z;
            z2 = zz;

            PortalDB.PortalList.Add( this );
        }
    }
}
