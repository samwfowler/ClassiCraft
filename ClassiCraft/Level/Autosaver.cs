using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class Autosaver {
        public static System.Timers.Timer backupTimer = new System.Timers.Timer( 15000 );

        public static void Setup() {
            backupTimer.Elapsed += delegate {
                foreach ( Level lvl in Level.LevelList ) {
                    if ( lvl.hasChanged ) {
                        lvl.Save();
                    }
                }
            };
            backupTimer.Start();
        }
    }
}
