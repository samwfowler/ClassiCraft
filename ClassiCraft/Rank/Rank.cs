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
        public PermissionLevel Permission;
        public CommandList Commands;

        public Rank() { }

        public Rank(string name, string color, PermissionLevel permission, bool canbreakbedrock = false) {
            Name = name;
            Color = color;
            CanBreakBedrock = canbreakbedrock;
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
                                case "permission":
                                    rank.Permission = (PermissionLevel)int.Parse( value );
                                    stage++;
                                    break;
                                case "commandsfile":
                                    rank.CommandsFile = value;
                                    stage++;
                                    break;
                            }

                            if ( stage >= 5 ) {
                                RankList.Add( new Rank( rank.Name, rank.Color, rank.Permission, rank.CanBreakBedrock ) );
                            }
                        } catch {
                            Server.Log( "Invalid line: " + line );
                        }
                    }
                }
            }

            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Guest ) == null ) { RankList.Add( new Rank( "Guest", "&f", PermissionLevel.Guest ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Member ) == null ) { RankList.Add( new Rank( "Member", "&b", PermissionLevel.Member ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Builder ) == null ) { RankList.Add( new Rank( "Builder", "&e", PermissionLevel.Builder ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Architect ) == null ) { RankList.Add( new Rank( "Architect", "&6", PermissionLevel.Architect ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Operator ) == null ) { RankList.Add( new Rank( "Operator", "&c", PermissionLevel.Operator ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Administrator ) == null ) { RankList.Add( new Rank( "Administrator", "&4", PermissionLevel.Administrator, true ) ); }
            if ( RankList.Find( grp => grp.Permission == PermissionLevel.Owner ) == null ) { RankList.Add( new Rank( "Owner", "&8", PermissionLevel.Owner, true ) ); }

            SaveRanks();
            Server.Log( "Loaded ranks..." );
        }

        public static void SaveRanks() {
            StreamWriter sw = new StreamWriter( File.Create( ConfigFile ) );
            foreach ( Rank rnk in RankList ) {
                sw.WriteLine( "Name = " + rnk.Name );
                sw.WriteLine( "Color = " + rnk.Color );
                sw.WriteLine( "CanBreakBedrock = " + rnk.CanBreakBedrock.ToString().ToLower() );
                sw.WriteLine( "Permission = " + rnk.Permission.GetHashCode().ToString() );
                sw.WriteLine( "CommandsFile = " + rnk.CommandsFile );
                sw.WriteLine();
            }
            sw.Flush();
            sw.Close();
            sw.Dispose();

            Server.Log( "Saved ranks..." );
        }

        public void FillCommands() {
            Commands = new CommandList();

            foreach ( Command cmd in Command.CommandList ) {
                if ( cmd.DefaultPerm <= Permission ) {
                    Commands.Add( cmd );
                }
            }

            SaveCommands();
        }

        public void SaveCommands() {
            StreamWriter sw = new StreamWriter( File.Create( CommandsFile ) );
            foreach ( Command cmd in Commands.Commands ) {
                sw.WriteLine( cmd.Name );
            }
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }

        public static Rank Find( string name ) {
            foreach ( Rank r in RankList ) {
                if ( r.Name.ToLower() == name.ToLower() ) {
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

        public static string GetColor( PermissionLevel perm ) {
            foreach ( Rank rnk in Rank.RankList ) {
                if ( rnk.Permission == perm ) {
                    return rnk.Color;
                }
            }
            return "";
        }
    }
}
