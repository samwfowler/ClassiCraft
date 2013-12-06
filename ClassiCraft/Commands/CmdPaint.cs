using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class CmdPaint : Command {
        public override string Name {
            get { return "Paint"; }
        }

        public override string Syntax {
            get { return "/paint"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Builder; }
        }

        public override void Use( Player p, string args ) {
            if ( p.isPainting ) {
                p.isPainting = false;
                p.SendMessage( "&cPaint: &ePaint mode &4disabled&e." );
            } else {
                p.isPainting = true;
                p.SendMessage( "&cPaint: &ePaint mode &2enabled&e." );
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "Enables paint mode." );
        }
    }
}
