using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ClassiCraft {
    public class CmdSpleef : Command {
        public override string Name {
            get { return "spleef"; }
        }

        public override string Syntax {
            get { return "/spleef [autorun]"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Member; }
        }

        public override void Use( Player p, string args ) {
            SpleefGame newGame = new SpleefGame();
            bool autorun = false;

            if ( args == "autorun" ) {
                autorun = true;
            }

            if ( p.Level.isHostingGame ) {
                p.SendMessage( "&cA game is already running on this level." );
                return;
            }

            newGame.GameStart(p.Level, autorun);
        }

        public override void Help( Player p ) {
            p.SendMessage( "Starts a game of spleef." );
        }

    }
}
