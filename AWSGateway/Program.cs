using System.Diagnostics;
using AWSGateway.Helpers;
using AWSGateway.Services;

namespace AWSGateway
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnUiThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnBackgroundThreadException;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Forms.LoginForm());

            // gasi TCP server samo ako ga je pokrenula ova aplikacija (Job Object u TcpClientService pokriva rušenje/prisilni prekid)
            TcpClientService.StopStartedServer();
        }

        private static void OnUiThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogAndShow(e.Exception);
        }

        private static void OnBackgroundThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogAndShow(ex);
            }
        }

        // ne mijenja postojeći JSON log sustav - samo osigurava da neobrađena iznimka ne sruši app bez traga
        private static void LogAndShow(Exception ex)
        {
            Debug.WriteLine("Neobrađena iznimka: " + ex);

            ActivityLogger.LogError("Neobrađena iznimka", "Program", ex.Message);

            MessageBox.Show(
                "Dogodila se neočekivana greška:" + Environment.NewLine + Environment.NewLine + ex.Message,
                "Greška",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
