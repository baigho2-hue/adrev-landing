using System;
using System.Windows;
using AdRev.Domain.Models;
using AdRev.Domain.Enums;

namespace AdRev.Desktop.Windows
{
    public partial class QuickAnalysisIntroWindow : Window
    {
        public string ProjectTitle { get; private set; } = string.Empty;
        public string AuthorName { get; private set; } = string.Empty;

        public QuickAnalysisIntroWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text) || string.IsNullOrWhiteSpace(AuthorBox.Text))
            {
                MessageBox.Show("Veuillez renseigner un titre et un auteur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProjectTitle = TitleBox.Text;
            AuthorName = AuthorBox.Text;
            
            DialogResult = true;
        }
    }
}
