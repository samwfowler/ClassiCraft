using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class Rank {
        public static List<Rank> RankList = new List<Rank>();
        public static string ConfigFile = "ranks/config.txt";

        public string Name;
        public string Color;
        public string CommandsFile;
        public bool CanBreakBedrock;
        public int DrawLimit;
        public PermissionLevel Permission;
        public CommandList Commands;

        public Rank() { }

        public Rank(string name, string color, PermissionLevel permission, bool canbreakbedrock = false) {
            Name = name;
            Color = color;
            CanBreakBedrock = canbreakbedrock;
            DrawLimit = permission.GetHashCode() * 100;
            Permission = permission;
            CommandsFile = Name + "_commands.txt";
            Commands = new CommandList();
        }

        public static void LoadRanks() {
            Rank rank = new Rank();
            int stage = 0;

            if ( File.Exists( ConfigFile ) ) {
                foreach ( string line in File.ReadAllLines( ConfigFile ) ) {
                    if ( line != "" && line[0] != '#' ) {
                        try {
                            string property = line.Split( '=' )[0].Trim();
                            string value = line.Split( '=' )[1].Trim();

                            switch ( property.ToLower() ) {
                                case "name":
                                    rank.Name = value;
                                    stage = 1;
                                    break;
                                case "color":
                                    rank.Color = value;
                                    stage++;
                                    break;
                                case "canbreakbedrock":
                                    rank.CanBreakBedrock = ( value == "true" ) ? true : false;
                                    stage++;
                                    break;
                                case "drawlimit":
                                    rank.DrawLimit = int.Parse( value );
                                    stage++;
                                    break;
                                case "permission":
                                    rank.Permission = (PermissionLevel)int.Parse( value );
                                    stage++;
                                    break;
                                case "commandsfile":
                                    rank.CommandsFile = value;
                                    stage++;
                                    break;
                            }

                            if ( stage >= 6 ) {
                                Rank newRank = new Rank( rank.Name, rank.Color, rank.Permission, rank.CanBreakBedrock );
                                RankList.Add( newRank );
                                newRank.LoadCommands();
                            }
                        } catch(Exception e) {
                            Server.Log( "Invalid line: " + line );
                            Server.Log( e.ToString() );
                        }
                    }
                }
            }

            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Guest ) == null ) { RankList.Add( new Rank( "Guest", "&7", PermissionLevel.Guest ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Builder ) == null ) { RankList.Add( new Rank( "Builder", "&2", PermissionLevel.Builder ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.AdvBuilder ) == null ) { RankList.Add( new Rank( "AdvBuilder", "&3", PermissionLevel.AdvBuilder ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Operator ) == null ) { RankList.Add( new Rank( "Operator", "&c", PermissionLevel.Operator ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.SuperOp ) == null ) { RankList.Add( new Rank( "SuperOp", "&e", PermissionLevel.SuperOp, true ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Owner ) == null ) { RankList.Add( new Rank( "Owner", "&4", PermissionLevel.Owner, true ) ); }

            SaveRanks();
            Server.Log( "Loaded ranks..." );
        }

        public static void SaveRanks() {
            StreamWriter sw = new StreamWriter( File.Create( ConfigFile ) );
            foreach ( Rank rnk in RankList ) {
                sw.WriteLine( "Name = " + rnk.Name );
                sw.WriteLine( "Color = " + rnk.Color );
                sw.WriteLine( "CanBreakBedrock = " + rnk.CanBreakBedrock.ToString().ToLower() );
                sw.WriteLine( "DrawLimit = " + rnk.DrawLimit );
                sw.WriteLine( "Permission = " + rnk.Permission.GetHashCode() );
                sw.WriteLine( "CommandsFile = " + rnk.CommandsFile );
                sw.WriteLine();
            }
            sw.Flush();
            sw.Close();
            sw.Dispose();

            Server.Log( "Saved ranks..." );
        }

        public void LoadCommands() {
            foreach ( CommandAllowance ca in CommandAllowance.CommandList ) {
                if ( ca.perm <= Permission ) {
                    Commands.Add( ca.cmd );
                }
            }
        }

        public static Rank Find( string name ) {
            foreach ( Rank r in RankList ) {
                if ( r.Name.ToLower().Contains( name.ToLower() ) ) {
                    return r;
                }
            }
            return null;
        }

        public static Rank Find( PermissionLevel perm ) {
            foreach ( Rank r in RankList ) {
                if ( r.Permission == perm ) {
                    return r;
                }
            }
            return null;
        }

        public static Rank RoundDown( PermissionLevel perm ) {
            int permy = perm.GetHashCode();
            retry:
            foreach ( Rank r in RankList ) {
                if ( r.Permission == (PermissionLevel)permy ) {
                    return r;
                }
            }

            if ( RankList[0].Permission < (PermissionLevel)permy ) {
                permy = permy - 1;
                goto retry;
            } else {
                return null;
            }
        }

        public static Rank RoundUp( PermissionLevel perm ) {
            int permy = perm.GetHashCode();
        retry:
            foreach ( Rank r in RankList ) {
                if ( r.Permission == (PermissionLevel)permy ) {
                    return r;
                }
            }

            if ( RankList[RankList.Count - 1].Permission > (PermissionLevel)permy ) {
                permy = permy + 1;
                goto retry;
            } else {
                return null;
            }
        }

        public static string GetColor( PermissionLevel perm ) {
            foreach ( Rank rnk in Rank.RankList ) {
                if ( rnk.Permission == perm ) {
                    return rnk.Color;
                }
            }

            if ( RoundDown( perm ) != null ) {
                return RoundDown( perm ).Color;
            } else if ( RoundUp( perm ) != null ) {
                return RoundUp( perm ).Color;
            }

            return null;
        }
    }
}
