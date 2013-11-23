using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class Config {
        public static string ClassiCraftVersion = "ClassiCraft 0.1";
        public static string ConfigFile = "config.txt";

        public static string Name = "ClassiCraft";
        public static string MOTD = "Welcome to my server!";
        public static string MainLevel = "Main";
        public static int Port = 25565;
        public static int maxPlayers = 32;
        public static bool isPublic = true;
        public static bool verifyPlayers = false;

        public static void LoadConfig() {
            if ( File.Exists( ConfigFile ) ) {
                foreach ( string line in File.ReadAllLines( ConfigFile ) ) {
                    if ( line != "" && line[0] != '#' ) {
                        if ( line.Split( '=' ).Length == 2 ) {
                            string key = line.Split( '=' )[0].Trim();
                            string value = line.Split( '=' )[1].Trim();

                            switch ( key.ToLower() ) {
                                case "name":
                                    Name = value;
                                    break;
                                case "motd":
                                    MOTD = value;
                                    break;
                                case "mainlevel":
                                    MainLevel = value;
                                    break;
                                case "port":
                                    Port = int.Parse( value );
                                    break;
                                case "maxplayers":
                                    maxPlayers = int.Parse( value );
                                    break;
                                case "ispublic":
                                    isPublic = bool.Parse( value );
                                    break;
                                case "verifyplayers":
                                    verifyPlayers = bool.Parse( value );
                                    break;
                            }
                        } else {
                            Server.Log( "Invalid line in config: " + line );
                        }
                    }
                }
            } else {
                SaveConfig();
            }

            Server.Log( "Loaded config..." );
        }

        public static void SaveConfig() {
            StreamWriter sw = new StreamWriter( File.Create( ConfigFile ) );
            sw.WriteLine( "Name = " + Name );
            sw.WriteLine( "MOTD = " + MOTD );
            sw.WriteLine( "MainLevel = " + MainLevel );
            sw.WriteLine( "Port = " + Port );
            sw.WriteLine( "maxPlayers = " + maxPlayers );
            sw.WriteLine( "isPublic = " + isPublic.ToString().ToLower() );
            sw.WriteLine( "verifyPlayers = " + verifyPlayers.ToString().ToLower() );
            sw.Flush();
            sw.Close();
            sw.Dispose();

            Server.Log( "Saved config..." );
        }
    }
}
