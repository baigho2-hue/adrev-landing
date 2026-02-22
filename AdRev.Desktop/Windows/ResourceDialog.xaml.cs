using System.Windows;
using System.Windows.Controls;
using AdRev.Domain.Models;

namespace AdRev.Desktop.Windows
{
    public partial class ResourceDialog : Window
    {
        public ProtocolResource? ResultResource { get; private set; }

        public ResourceDialog()
        {
            InitializeComponent();
            TitleBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Veuillez entrer un titre.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var typeTag = ((ComboBoxItem)TypeComboBox.SelectedItem)?.Tag?.ToString();
            var resourceType = typeTag == "Figure" ? ResourceType.Figure : ResourceType.Table;

            ResultResource = new ProtocolResource
            {
                Title = TitleBox.Text,
                Type = resourceType,
                Description = DescriptionBox.Text
                // Number will be assigned by parent
            };

            DialogResult = true;
            Close();
        }
    }
}
