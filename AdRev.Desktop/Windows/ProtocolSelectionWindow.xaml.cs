using System;
using System.Collections.Generic;
using System.Windows;
using AdRev.Domain.Protocols;

namespace AdRev.Desktop.Windows
{
    public partial class ProtocolSelectionWindow : Window
    {
        public ResearchProtocol? SelectedProtocol { get; private set; }

        public ProtocolSelectionWindow(IEnumerable<ResearchProtocol> protocols)
        {
            InitializeComponent();
            ProtocolsList.ItemsSource = protocols;
        }

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            if (ProtocolsList.SelectedItem is ResearchProtocol protocol)
            {
                SelectedProtocol = protocol;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un protocole.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
