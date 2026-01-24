using System.Configuration;
using System.Data;
using System.Windows;

namespace AdRev.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly AdRev.Core.Services.LicensingService _licensingService = new AdRev.Core.Services.LicensingService();

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Check if License is Valid
            if (!_licensingService.IsActivated(out string _))
            {
                // Show Activation Window
                var activationWindow = new ActivationWindow();
                bool? result = activationWindow.ShowDialog();

                if (result != true)
                {
                    // User closed activation or it failed
                    Shutdown();
                    return;
                }
            }

            // 2. License OK, show MainWindow
            MainWindow main = new MainWindow();
            main.Show();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            string message = ex.Message;
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                message += "\n\nInner Exception: " + ex.Message;
            }

            MessageBox.Show($"Une erreur inattendue est survenue : {message}\n\n{ex.StackTrace}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
