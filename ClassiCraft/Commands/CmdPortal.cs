using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdPortal : Command {
        public override string Name {
            get { return "Portal"; }
        }

        public override string Syntax {
            get { return "/portal [material]"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            if ( args != "" ) {
                if ( args.Split( ' ' ).Length < 2 ) {
                    material = Block.Byte( args );
                } else {
                    material = Block.Byte( args.Split( ' ' )[0] );
                }

                if ( material == Block.Air ) {
                    material = Block.PortalAir;
                } else if ( material == Block.Water || material == Block.Waterstill ) {
                    material = Block.PortalWater;
                } else if ( material == Block.Lava || material == Block.Lavastill ) {
                    material = Block.PortalLava;
                }

                if ( material < 0 || material > 149 ) {
                    p.SendMessage( "&cMaterial \"&f" + args + "&c\" was not found." );
                    return;
                }
            } else {
                usecurrentmaterial = true;
            }

            if ( p.Level.BuildPermission > p.Rank.Permission ) {
                p.SendMessage( "&cYou aren't permitted to build here." );
                return;
            }


            if ( multi ) {

            } else {
                p.SendMessage( "&cPortal: &ePlace a block at the entrance." );
            }
            p.OnBlockChange += Blockchange1;
        }

        ushort x1;
        ushort y1;
        ushort z1;
        Level previous;
        byte material;
        bool usecurrentmaterial = false;
        bool multi = false;

        void Blockchange1( Player p, ushort x, ushort y, ushort z, byte type ) {
            p.ClearBlockChange();

            if ( usecurrentmaterial ) {
                material = type;
            }

            p.SendSetBlock( x, y, z, material );
            p.Level.Blockchange( p, x, y, z, material );

            x1 = x;
            y1 = y;
            z1 = z;
            previous = p.Level;

            if ( type == Block.Red || !multi ) {
                p.SendMessage( "&cPortal: &ePlace a block at the exit." );
                p.OnBlockChange += Blockchange2;
            }
        }

        void Blockchange2( Player p, ushort x2, ushort y2, ushort z2, byte type ) {
            p.ClearBlockChange();
            byte b = p.Level.GetBlock( x2, y2, z2 );
            p.SendSetBlock( x2, y2, z2, b );

            Portal newPortal = new Portal( previous.Name, p.Level.Name, x1, x2, y1, y2, z1, z2 );
            PortalDB.SavePortals();
        }

        public override void Help( Player p ) {
            p.SendMessage( "Creates a portal." );
        }
    }
}
