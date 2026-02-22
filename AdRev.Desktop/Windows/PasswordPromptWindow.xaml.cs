using System.Windows;
using AdRev.Core.Services;

namespace AdRev.Desktop.Windows
{
    public partial class PasswordPromptWindow : Window
    {
        private readonly string _targetHash;

        public PasswordPromptWindow(string hash)
        {
            InitializeComponent();
            _targetHash = hash;
            PasswordField.Focus();
        }

        private void Unlock_Click(object sender, RoutedEventArgs e)
        {
            if (SecurityService.VerifyPassword(PasswordField.Password, _targetHash))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorText.Visibility = Visibility.Visible;
                PasswordField.Password = string.Empty;
                PasswordField.Focus();
            }
        }
    }
}
