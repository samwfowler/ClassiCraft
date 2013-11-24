using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class PlayerDB {
        public static void Load( Player p ) {
            if ( !File.Exists( "players/" + p.Name + ".db" ) ) {
                p.Rank = Rank.RankList[0];
                Save( p );
                return;
            }

            foreach ( string line in File.ReadAllLines( "players/" + p.Name + ".db" ) ) {
                if ( !string.IsNullOrEmpty( line ) && line[0] != '#' ) {
                    string key = line.Split( '=' )[0].Trim();
                    string value = line.Split( '=' )[1].Trim();

                    switch ( key.ToLower() ) {
                        case "rank":
                            Rank targetRank = Rank.Find( value );

                            if ( targetRank == null ) {
                                targetRank = Rank.RankList[0];
                            }

                            p.Rank = targetRank;
                            break;
                        case "coins":
                            p.Coins = int.Parse( value );
                            break;
                    }
                }
            }
        }

        public static void Save( Player p ) {
            try {
                StreamWriter sw = new StreamWriter( File.Create( "players/" + p.Name + ".db" ) );
                sw.WriteLine( "Rank = " + p.Rank.Name );
                sw.WriteLine( "Coins = " + p.Coins );
                sw.Flush();
                sw.Close();
                sw.Dispose();
            } catch {

            }
        }
    }
}
