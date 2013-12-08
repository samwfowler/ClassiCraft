using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdUnlockLevel : Command {
        public override string Name {
            get { return "UnlockLevel"; }
        }

        public override string Syntax {
            get { return "/unlocklevel"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            p.Level.enableEditing = true;
            Player.GlobalMessage( "Level '" + Rank.Find(p.Level.BuildPermission).Color + p.Level.Name + "&e' was &aunlocked&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Unlocks a level." );
        }

    }
}
