using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class Directories {
        public static void LoadDirectories() {
            if ( !System.IO.Directory.Exists( "levels" ) ) {
                System.IO.Directory.CreateDirectory( "levels" );
            }

            if ( !System.IO.Directory.Exists( "players" ) ) {
                System.IO.Directory.CreateDirectory( "players" );
            }

            if ( !System.IO.Directory.Exists( "ranks" ) ) {
                System.IO.Directory.CreateDirectory( "ranks" );
            }

            if ( !System.IO.Directory.Exists( "documentation" ) ) {
                System.IO.Directory.CreateDirectory( "documentation" );
            }
        }
    }
}
