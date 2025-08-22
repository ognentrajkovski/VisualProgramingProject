using System;
using System.Windows.Forms;

namespace VisualProgramingProject
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GameForm());
        }
    }
}
