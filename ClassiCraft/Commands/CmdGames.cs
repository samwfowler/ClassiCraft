using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdGames : Command {
        public override string Name {
            get { return "Games"; }
        }

        public override string Syntax {
            get { return "/games"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            p.SendMessage( "Available games:" );
            p.SendMessage( "&cBlockrun - &fRun across an icey flatgrass as you melt the floor with your feet, try and trick other players into falling through down into the lava pit that lies beneath!" );
            p.SendMessage( "&cSpleef - &fUnstable blocks are randomly changing state, wholes are being burned through the ice, can you evade the 1000 degree lavapit beneath?" );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of available commands." );
        }
    }
}
