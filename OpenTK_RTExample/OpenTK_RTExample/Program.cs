using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenTK_RTExample
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
