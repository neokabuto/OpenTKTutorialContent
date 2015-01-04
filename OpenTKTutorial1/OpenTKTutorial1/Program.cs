using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTKTutorial1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {
                /*The Run method of the GameWindow has multiple overloads.
                 * With a single float parameter, Run will give your window 30 UpdateFrame events a second, and as many RenderFrame events per second as the computer will process.*/
                game.Run(30.0);
            }
        }
    }
}
