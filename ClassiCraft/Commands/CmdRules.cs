using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class CmdRules : Command {
        public override string Name {
            get { return "Rules"; }
        }

        public override string Syntax {
            get { return "/rules"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            retry:
            if ( File.Exists( "documentation/rules.txt" ) ) {
                foreach ( string line in File.ReadAllLines( "documentation/rules.txt" ) ) {
                    p.SendMessage( line );
                }
            } else {
                StreamWriter sw = new StreamWriter( File.Create( "documentation/rules.txt" ) );
                sw.WriteLine( "&aRules File: " );
                sw.WriteLine( " * No griefing" );
                sw.WriteLine( " * No asking for ranks" );
                sw.WriteLine( " * Guests please type &f/goto guest" );
                sw.WriteLine( " &c* Have fun!" );
                sw.Flush();
                sw.Close();
                sw.Dispose();
                goto retry;
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of rules." );
        }
    }
}
