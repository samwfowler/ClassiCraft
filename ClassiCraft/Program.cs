using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassiCraft {
    public class Program {
        static void Main( string[] args ) {
            Server.OnLog += Console.WriteLine;
            Server.Start();

            while ( true ) {
                Console.ReadLine();
            }
        }
    }
}
