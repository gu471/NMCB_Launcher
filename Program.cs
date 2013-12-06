using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;    

namespace NMCB_Launcher
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //AppDomain.CurrentDomain.AppendPrivatePath(@"lib\");
            AppDomain.CurrentDomain.AppendPrivatePath(@"lib\");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1( args));
        }
    }
}
