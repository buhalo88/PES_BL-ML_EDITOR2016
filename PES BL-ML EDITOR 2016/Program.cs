using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PES_BL_ML_EDITOR_2016
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
