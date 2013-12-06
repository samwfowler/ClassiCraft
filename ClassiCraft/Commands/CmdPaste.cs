using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdPaste : Command {
        public override string Name {
            get { return "Paste"; }
        }

        public override string Syntax {
            get { return "/paste"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Builder; }
        }

        public override void Use( Player p, string args ) {
            if ( p.Level.BuildPermission > p.Rank.Permission ) {
                p.SendMessage( "&cYou aren't permitted to build here." );
                return;
            }

            p.SendMessage( "&cPaste: &ePlace a block to paste." );
            p.OnBlockChange += Blockchange1;
        }

        void Blockchange1( Player p, ushort x, ushort y, ushort z, byte type ) {
            p.ClearBlockChange();
            byte b = p.Level.GetBlock( x, y, z );
            p.SendSetBlock( x, y, z, b );

            foreach ( BufferPos bp in p.BufferCopy.copyBuffer ) {
                p.Level.Blockchange( (ushort)( x + bp.X ), (ushort)( y + bp.Y ), (ushort)( z + bp.Z ), bp.Type );
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "Pastes copied blocks" );
        }
    }
}
