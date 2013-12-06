using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdZone : Command {
        public override string Name {
            get { return "Zone"; }
        }

        public override string Syntax {
            get { return "/zone rank"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            if ( args == "" ) {
                p.SendMessage( "&cPlease specify a rank." );
                return;
            }

            targetRank = Rank.Find( args );
            if ( targetRank == null ) {
                p.SendMessage( "&cRank \"&f" + args + "&c\" was not found." );
                return;
            }

            if ( p.Level.BuildPermission > p.Rank.Permission ) {
                p.SendMessage( "&cYou aren't permitted to build here." );
                return;
            }

            p.SendMessage( "&cZone: &ePlace a block at the first corner." );
            p.OnBlockChange += Blockchange1;
        }

        ushort x1;
        ushort y1;
        ushort z1;
        Rank targetRank;

        void Blockchange1( Player p, ushort x, ushort y, ushort z, byte type ) {
            p.ClearBlockChange();
            byte b = p.Level.GetBlock( x, y, z );
            p.SendSetBlock( x, y, z, b );

            x1 = x;
            y1 = y;
            z1 = z;

            p.SendMessage( "&cZone: &ePlace a block at the second corner." );
            p.OnBlockChange += Blockchange2;
        }

        void Blockchange2( Player p, ushort x2, ushort y2, ushort z2, byte type ) {
            p.ClearBlockChange();
            byte b = p.Level.GetBlock( x2, y2, z2 );
            p.SendSetBlock( x2, y2, z2, b );

            Zone newZone = new Zone( p.Level.Name, x1, x2, y1, y2, z1, z2, targetRank.Permission );
            ZoneDB.SaveZones();

            p.SendMessage( "&cZone: &eSuccessfully created zone for " + targetRank.Color + targetRank.Name + "&e." );
        }

        public override void Help( Player p ) {
            p.SendMessage( "Creates a rank limited zone." );
        }
    }
}
