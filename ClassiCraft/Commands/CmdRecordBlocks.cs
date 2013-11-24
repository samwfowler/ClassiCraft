using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ClassiCraft {
    public class CmdRecordBlocks : Command {
        public override string Name {
            get { return "RecBlocks"; }
        }

        public override string Syntax {
            get { return "/recblocs"; }
        }

        public override PermissionLevel DefaultPerm {
            get { return PermissionLevel.Operator; }
        }

        public override void Use( Player p, string args ) {
            StreamWriter sw = new StreamWriter( File.Create( "treecoordinates.txt" ) );
                p.OnBlockChange += delegate( Player pl, ushort x, ushort y, ushort z, byte type ) {
                    if ( type == Block.Red ) {
                        sw.Flush();
                        sw.Close();
                        sw.Dispose();
                        p.ClearBlockChange();
                    } else {
                        sw.WriteLine( "level.SetBlock(" + x + ", " + y + ", " + z + ", " + type + ");" );
                    }
                };
        }

        public override void Help( Player p ) {
            p.SendMessage( "Records blockchanges to a file." );
        }

    }
}
