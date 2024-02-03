using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace B1_Test_Task.Services
{
    public static class ExceptionNotifier
    {
        public static void NotifyAboutException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(((Exception)e.ExceptionObject).Message);
        }

        public static void NotifyAboutException(string message)
        {
            MessageBox.Show(message);
        }
    }
}
