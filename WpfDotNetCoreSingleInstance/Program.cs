using System;
using System.Windows;

namespace WpfDotNetCoreSingleInstance
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var singleInstanceManager = new SingleInstanceManager("{E2BF9293-7630-4262-A2FA-56B6304682FC}");

            if (singleInstanceManager.IsOnlyInstance)
            {
                var app = new App();
                app.Run(new MainWindow());
            }
            else
            {
                MessageBox.Show("Another Instance is already running");
                Environment.Exit(0);
            }
        }
    }
}