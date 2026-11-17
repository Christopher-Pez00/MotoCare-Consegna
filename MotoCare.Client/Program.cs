using System;
using System.Windows.Forms;

namespace MotoCare.Client
{
   
    internal static class Program
    {
        
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();

           
            Application.SetCompatibleTextRenderingDefault(false);

            // Avvia il programma aprendo la finestra di Login
            // Il programma rimane attivo finché la finestra non viene chiusa
            Application.Run(new LoginForm());
        }
    }
}