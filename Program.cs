using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CS.BO;
using CS.MongoDB.CSDataManager;

namespace CannedSoftware
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Discriminator.Instance.Register();

            //TODO:  create login screen for username/password/server/database
            var init = DataManager.Instance;

            Application.Run(new Form1());
        }
    }
}
