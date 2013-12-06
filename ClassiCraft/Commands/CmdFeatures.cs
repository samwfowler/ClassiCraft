using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdFeatures : Command {
        public override string Name {
            get { return "Features"; }
        }

        public override string Syntax {
            get { return "/features [submenu]"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            p.SendMessage( "------------------------" );
            p.SendMessage( "&fClassiCube &efeatures" );
            p.SendMessage( "------------------------" );
            p.SendMessage( "&f> &eMultiple gamemodes." );
            p.SendMessage( "&f> &eEfficient building commands." );
            p.SendMessage( "&f> &ePersonal maps (if enabled)." );
            p.SendMessage( "&f> &eOver " + (Command.CommandList.Count - 1) + " commands." );
            p.SendMessage( "&f> &eAlmost limitless number of ranks." );
            p.SendMessage( "&f> &eEasy-to-use admin controls." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of features." );
        }

    }
}
