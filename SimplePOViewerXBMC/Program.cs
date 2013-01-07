using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SimplePOViewerXBMC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string xbmc = Environment.GetEnvironmentVariable("XBMC_ROOT");
            if (xbmc == null)
            {
                if (args.Length == 0)
                {
                    xbmc = Environment.CurrentDirectory;
                }
                else
                    xbmc = args[0];
            }

            System.IO.DirectoryInfo root = new System.IO.DirectoryInfo(xbmc);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain(root));
        }
    }
}
