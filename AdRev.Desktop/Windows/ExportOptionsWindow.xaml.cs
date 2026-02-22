using System.Windows;

namespace AdRev.Desktop.Windows
{
    public partial class ExportOptionsWindow : Window
    {
        public string SelectedFormat { get; private set; } = "Excel";
        public bool Anonymize { get; private set; } = true;

        public ExportOptionsWindow()
        {
            InitializeComponent();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SelectedFormat = FormatCombo.SelectedIndex == 0 ? "Excel" : "CSV";
            Anonymize = CheckAnonymize.IsChecked ?? false;
            DialogResult = true;
            Close();
        }
    }
}
