using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdCommands : Command {
        public override string Name {
            get { return "Commands"; }
        }

        public override string Syntax {
            get { return "/commands"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Guest; }
        }

        public override void Use( Player p, string args ) {
            string cmdList = "";
            int cmdCount = 0;

            CommandAllowance.CommandList.ForEach( delegate( CommandAllowance cmd ) {
                if ( cmd.perm <= p.Rank.Permission ) {
                    cmdList += " &f| " + Rank.GetColor( cmd.perm ) + cmd.cmd.Name.ToLower();
                    cmdCount++;
                }
            } );

            if ( cmdList != "" ) {
                p.SendMessage( "Available commands (&a" + cmdCount + "&e):" );
                p.SendMessage( cmdList.Remove(0, 5) );
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "Displays a list of available commands." );
        }
    }
}
