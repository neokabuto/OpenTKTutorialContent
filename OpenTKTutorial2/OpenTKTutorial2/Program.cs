using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTKTutorial2
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {

                game.Run(30, 30);

            }
        }
    }
}
