using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AdRev.Domain.Models;
using AdRev.Domain.Enums;
using AdRev.Core.Services;
using System.Collections.Generic;

namespace AdRev.Desktop
{
    public partial class NewProjectWindow : Window
    {
        public ResearchProject? CreatedProject { get; private set; }
        private readonly CloudSyncService _cloudService = new CloudSyncService();

        public NewProjectWindow()
        {
            InitializeComponent();
            PathTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AdRev", "Projects");
            LoadCloudShortcuts();
        }

        private void LoadCloudShortcuts()
        {
            var providers = _cloudService.GetAvailableCloudProviders();
            if (providers.Count > 0)
            {
                CloudProvidersItems.ItemsSource = providers;
                CloudShortcuts.Visibility = Visibility.Visible;
            }
            else
            {
                CloudShortcuts.Visibility = Visibility.Collapsed;
            }
        }

        private void BrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            dialog.Title = "Sélectionnez l'emplacement des projets AdRev";
            dialog.InitialDirectory = PathTextBox.Text;
            
            if (dialog.ShowDialog() == true)
            {
                PathTextBox.Text = dialog.FolderName;
            }
        }

        private void CloudShortcut_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CloudProviderInfo info)
            {
                PathTextBox.Text = Path.Combine(info.Path, "AdRev_Projects");
            }
        }

        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Veuillez saisir un titre de projet.");
                return;
            }

            string projectPath = Path.Combine(PathTextBox.Text, TitleTextBox.Text.Replace(" ", "_"));
            if (!Directory.Exists(projectPath)) Directory.CreateDirectory(projectPath);

            CreatedProject = new ResearchProject
            {
                Title = TitleTextBox.Text,
                Authors = AuthorTextBox.Text,
                Institution = InstitutionTextBox.Text,
                StudyType = (StudyType)TypeComboBox.SelectedIndex,
                StoragePath = projectPath,
                CreatedOn = DateTime.Now
            };

            if (!string.IsNullOrEmpty(ProjectPasswordField.Password))
            {
                CreatedProject.PasswordHash = SecurityService.HashPassword(ProjectPasswordField.Password);
            }

            DialogResult = true;
            Close();
        }
    }
}
