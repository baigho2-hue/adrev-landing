using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace AdRev.Desktop.Views.Shared
{
    public partial class SimpleMessageDialog : UserControl
    {
        public SimpleMessageDialog()
        {
            InitializeComponent();
        }

        public void Configure(string message, string title, PackIconKind icon, bool isConfirmation = false)
        {
            MessageText.Text = message;
            TitleText.Text = title;
            IconDisplay.Kind = icon;
            
            if (isConfirmation)
            {
                CancelButton.Visibility = Visibility.Visible;
                IconDisplay.Foreground = System.Windows.Media.Brushes.Orange;
            }
            else if (icon == PackIconKind.Error)
            {
                IconDisplay.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(true, this);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, this);
        }
    }
}
