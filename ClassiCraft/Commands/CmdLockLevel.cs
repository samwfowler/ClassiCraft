using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdLockLevel : Command {
        public override string Name {
            get { return "LockLevel"; }
        }

        public override string Syntax {
            get { return "/locklevel"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            p.Level.enableEditing = false;
            Player.GlobalMessage( "Level '" + Rank.Find(p.Level.BuildPermission).Color + p.Level.Name + "&e' was &clocked&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Locks a level." );
        }

    }
}
