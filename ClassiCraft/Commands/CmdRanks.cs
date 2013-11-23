using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdRanks : Command {
        public override string Name {
            get { return "Ranks"; }
        }

        public override string Syntax {
            get { return "/ranks"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            p.SendMessage("Available ranks:");

            Rank.RankList.ForEach( delegate( Rank r ) {
                p.SendMessage( r.Color + r.Name + " &c- " + r.Permission.GetHashCode().ToString() );
            } );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of available ranks." );
        }

    }
}
