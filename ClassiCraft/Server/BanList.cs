using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class BanList {
        public static List<string> bans = new List<string>();
        static string bansFile = "bans.txt";

        public static void LoadBans() {
            if ( !File.Exists( bansFile ) ) {
                return;
            }

            foreach ( string ban in File.ReadAllLines( bansFile ) ) {
                bans.Add( ban );
            }
        }

        public static void SaveBans() {
            StreamWriter sw = new StreamWriter( File.Create( bansFile ) );
            foreach ( string ban in bans ) {
                sw.WriteLine( ban );
            }
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }
        
        public static void Add( string name ) {
            if ( !bans.Contains( name.ToLower() ) ) {
                bans.Add( name.ToLower() );
                SaveBans();
            }
        }

        public static void Remove( string name ) {
            if ( bans.Contains( name.ToLower() ) ) {
                bans.Remove( name.ToLower() );
                SaveBans();
            }
        }
    }
}
