using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class CmdHelp : Command {
        public override string Name {
            get { return "Help"; }
        }

        public override string Syntax {
            get { return "/help [command]"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            switch ( args ) {
                case "":
            retry:
                    if ( File.Exists( "documentation/helpfile.txt" ) ) {
                        foreach ( string line in File.ReadAllLines( "documentation/helpfile.txt" ) ) {
                            p.SendMessage( line );
                        }
                    } else {
                        StreamWriter sw = new StreamWriter( File.Create( "documentation/helpfile.txt" ) );
                        sw.WriteLine( "For a list of available commands, type &a/commands&e." );
                        sw.WriteLine( "For help on a specific command, type &a/help [command]&e." );
                        sw.WriteLine( "For a list of players online, type &a/players&e." );
                        sw.WriteLine( "For a list of available ranks, type &a/ranks&e." );
                        sw.WriteLine( "To speak globally, put a &c# &ebefore your message." );
                        sw.Flush();
                        sw.Close();
                        sw.Dispose();
                        goto retry;
                    }
                    break;
                default:
                    Command cmd = Command.Find( args );
                    if ( cmd != null ) {
                        p.SendMessage( "&f> " + Rank.GetColor(cmd.DefaultPerm) + cmd.Name + " &ehelp:" );
                        p.SendMessage( "Syntax: &f" + cmd.Syntax );
                        cmd.Help( p );
                        p.SendMessage( "Rank needed: " + Rank.GetColor( cmd.DefaultPerm ) + Rank.Find( cmd.DefaultPerm ).Name );
                    } else {
                        p.SendMessage( "&cCommand &f/" + args + "&c was not found." );
                        return;
                    }
                    break;
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "No, just no." );
        }

    }
}
