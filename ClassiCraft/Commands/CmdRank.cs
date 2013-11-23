﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class CmdRank : Command {
        public override string Name {
            get { return "Rank"; }
        }

        public override string Syntax {
            get { return "/rank player rank"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Administrator; }
        }

        public override void Use( Player p, string args ) {
            string tp = args.Split( ' ' )[0];
            string tr = args.Split( ' ' )[1];
            Player targetPlayer = Player.Find( tp );
            Rank targetRank = Rank.Find( tr );

            RankChangeType rankchangetype = RankChangeType.Null;

            if ( targetPlayer == null ) {
                p.SendMessage( "&cPlayer \"&f" + tp + "&c\" was not found." );
                p.SendMessage( "&cUse &f/players &cto check the name was correct." );
                return;
            } else if ( targetRank == null ) {
                p.SendMessage( "&cRank \"&f" + tr + "&c\" was not found." );
                p.SendMessage( "&cUse &f/ranks &cto check the rank's name was correct." );
                return;
            }

            if ( targetPlayer == p ) {
                p.SendMessage( "&cYou cannot change your own rank." );
                return;
            } else if ( targetRank.Permission >= p.Rank.Permission && p.Rank.Permission < PermissionLevel.Owner ) {
                p.SendMessage( "&cYou must specify a rank with a lower permission to yours." );
                return;
            }

            if ( targetRank.Permission > targetPlayer.Rank.Permission ) {
                rankchangetype = RankChangeType.Promotion;
            } else if ( targetRank.Permission < targetPlayer.Rank.Permission ) {
                rankchangetype = RankChangeType.Demotion;
            } else {
                p.SendMessage( "&cPlayer \"&f" + targetPlayer.Name + "&c\" is already a " + targetRank.Color + targetRank.Name + "&c." );
                return;
            }

            if ( rankchangetype == RankChangeType.Promotion ) {
                Player.GlobalMessage( targetPlayer.Rank.Color + targetPlayer.Name + " &ewas promoted from " + targetPlayer.Rank.Color + targetPlayer.Rank.Name + " &eto " + targetRank.Color + targetRank.Name + "&e." );
                p.SendMessage( "&6You've been promoted!" );
                targetPlayer.Rank = targetRank;
                PlayerDB.Save( targetPlayer );
            } else if ( rankchangetype == RankChangeType.Demotion ) {
                Player.GlobalMessage( targetPlayer.Rank.Color + targetPlayer.Name + " &cwas demoted from " + targetPlayer.Rank.Color + targetPlayer.Rank.Name + " &cto " + targetRank.Color + targetRank.Name + "&c." );
                p.SendMessage( "&4You've been demoted." );
                targetPlayer.Rank = targetRank;
                PlayerDB.Save( targetPlayer );
            }
        }

        public override void Help( Player p ) {
            p.SendMessage( "Sets a players rank." );
        }

        public enum RankChangeType {
            Promotion,
            Demotion,
            Null
        }
    }
}
