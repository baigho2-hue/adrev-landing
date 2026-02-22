using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using AdRev.Desktop.Views.Shared;

namespace AdRev.Desktop.Helpers
{
    public static class DialogHelper
    {
        /// <summary>
        /// Shows a simple information message.
        /// </summary>
        public static async Task ShowMessage(string message, string title = "Information")
        {
            var view = new SimpleMessageDialog();
            view.Configure(message, title, PackIconKind.Information);
            await DialogHost.Show(view, "RootDialog");
        }

        /// <summary>
        /// Shows an error message.
        /// </summary>
        public static async Task ShowError(string message, string title = "Erreur")
        {
            var view = new SimpleMessageDialog();
            view.Configure(message, title, PackIconKind.Error);
            await DialogHost.Show(view, "RootDialog");
        }

        /// <summary>
        /// Shows a warning message.
        /// </summary>
        public static async Task ShowWarning(string message, string title = "Attention")
        {
            var view = new SimpleMessageDialog();
            view.Configure(message, title, PackIconKind.Warning);
            await DialogHost.Show(view, "RootDialog");
        }

        /// <summary>
        /// Shows a confirmation dialog (Yes/No). Returns true if confirmed.
        /// </summary>
        public static async Task<bool> ShowConfirmation(string message, string title = "Confirmation")
        {
            var view = new SimpleMessageDialog();
            view.Configure(message, title, PackIconKind.QuestionMark, isConfirmation: true);
            var result = await DialogHost.Show(view, "RootDialog");
            return result is bool b && b;
        }
    }
}
