using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public abstract class Command {
        public static List<Command> CommandList = new List<Command>();

        public abstract string Name { get; }
        public abstract string Syntax { get; }
        public abstract PermissionLevel DefaultPerm { get; }
        public abstract void Use( Player p, string args );
        public abstract void Help( Player p );

        public static void LoadCommands() {
            CommandList.Add( new CmdAddLevel() );
            CommandList.Add( new CmdBan() );
            CommandList.Add( new CmdBanned() );
            CommandList.Add( new CmdBlockRun() );
            CommandList.Add( new CmdBox() );
            CommandList.Add( new CmdCommands() );
            CommandList.Add( new CmdCopy() );
            CommandList.Add( new CmdDelLevel() );
            CommandList.Add( new CmdFeatures() );
            CommandList.Add( new CmdGames() );
            CommandList.Add( new CmdGetLoc() );
            CommandList.Add( new CmdGoto() );
            CommandList.Add( new CmdHBox() );
            CommandList.Add( new CmdHelp() );
            CommandList.Add( new CmdInfo() );
            CommandList.Add( new CmdKick() );
            CommandList.Add( new CmdLevels() );
            CommandList.Add( new CmdLoad() );
            CommandList.Add( new CmdMaterials() );
            CommandList.Add( new CmdMute() );
            CommandList.Add( new CmdPaint() );
            CommandList.Add( new CmdPaste() );
            CommandList.Add( new CmdPlayers() );
            CommandList.Add( new CmdPortal() );
            CommandList.Add( new CmdRank() );
            CommandList.Add( new CmdReplace() );
            CommandList.Add( new CmdReplaceAll() );
            CommandList.Add( new CmdRanks() );
            CommandList.Add( new CmdRecordBlocks() );
            CommandList.Add( new CmdHideZones() );
            CommandList.Add( new CmdShowZones() );
            CommandList.Add( new CmdRules() );
            CommandList.Add( new CmdSave() );
            CommandList.Add( new CmdSay() );
            CommandList.Add( new CmdPerBuild() );
            CommandList.Add( new CmdPerJoin() );
            CommandList.Add( new CmdSetMain() );
            CommandList.Add( new CmdSetSpawn() );
            CommandList.Add( new CmdSpawn() );
            CommandList.Add( new CmdSpleef() );
            CommandList.Add( new CmdTitle() );
            CommandList.Add( new CmdUnban() );
            CommandList.Add( new CmdUnload() );
            CommandList.Add( new CmdUnmute() );
            CommandList.Add( new CmdWhois() );
            CommandList.Add( new CmdZone() );
        }

        public static Command Find( string name ) {
            foreach ( Command cmd in CommandList ) {
                if ( cmd.Name.ToLower() == name.ToLower() ) {
                    return cmd;
                }
            }
            return null;
        }
    }

    public class CommandList {
        public List<Command> Commands = new List<Command>();

        public CommandList() { }

        public void Add( Command cmd ) {
            if ( !Commands.Contains( cmd ) ) {
                Commands.Add( cmd );
            }
        }

        public void Delete( Command cmd ) {
            if ( Commands.Contains( cmd ) ) {
                Commands.Remove( cmd );
            }
        }

        public bool Contains( Command cmd ) {
            if ( Commands.Contains( cmd ) ) {
                return true;
            } else {
                return false;
            }
        }
    }
}
