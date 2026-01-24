using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdRev.Core.Common;
using AdRev.Domain.Enums;
using AdRev.Domain.Variables;
using AdRev.Domain.Models;
using System.Windows.Documents;
using System.Windows.Media;
using AdRev.Domain.Quality;
using AdRev.Core.Services;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.Json;
using AdRev.Desktop.Services;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using AdRev.Core.Extensions;
using AdRev.Desktop.Windows;

namespace AdRev.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly ResearchProjectService _projectService = new ResearchProjectService();
        private ResearchProject? _currentProject;
        private readonly AdRev.Core.Services.LicensingService _licensingService = new AdRev.Core.Services.LicensingService();
        private readonly FeatureManager _featureManager;
        private readonly AdRev.Core.Protocols.ProtocolService _protocolService = new AdRev.Core.Protocols.ProtocolService();

        // Fields for legacy features removed - clean state

        public MainWindow()
        {
            _featureManager = new FeatureManager(_licensingService);
            InitializeComponent();

            // Sizing and positioning: 10px margin top and bottom
            double workHeight = SystemParameters.WorkArea.Height;
            double workWidth = SystemParameters.WorkArea.Width;
            this.Height = workHeight - 20;
            this.Width = Math.Min(1280, workWidth - 40); // Maintain a reasonable maximum width
            this.Top = 10 + SystemParameters.WorkArea.Top;
            this.Left = (workWidth - this.Width) / 2 + SystemParameters.WorkArea.Left;

            StudyTypeComboBox.ItemsSource = Enum.GetValues(typeof(StudyType));
            StudyTypeComboBox.SelectedIndex = 0;

            ProjectContextComboBox.ItemsSource = System.Enum.GetValues(typeof(ProjectContext));
            ProjectContextComboBox.SelectedIndex = 0;

            DomainComboBox.ItemsSource = System.Enum.GetValues(typeof(ScientificDomain));
            DomainComboBox.SelectedIndex = 0;

            LoadProjects();
            
            // Force Startup View to Dashboard
            MainViewTabControl.SelectedItem = ViewDash;

            Loaded += MainWindow_Loaded;
            UpdateLicenseDisplay();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUserProfile();
            CheckLicense();
        }

        private void CheckLicense()
        {
            if (!_licensingService.IsActivated(out string status))
            {
                // Prompt verification
                WelcomeWindow prompt = new WelcomeWindow();
                // Optionally Pre-fill if profile exists? For now, clean slate for simplicity or re-registration.
                
                bool? result = prompt.ShowDialog();
                if (result != true || !_licensingService.IsActivated(out _))
                {
                    MessageBox.Show("Une licence valide est requise pour utiliser AdRev Desktop.\nL'application va se fermer.", "Accès Refusé", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
                else
                {
                    // Update Profile if changed in prompt
                    if (prompt.Profile != null) CheckUserProfile(); // Helper to reload or ensure persistence
                }
            }
            UpdateLicenseDisplay();        }

        private void CheckUserProfile()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdRev");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);

            string profilePath = Path.Combine(appData, "user_profile.json");
            UserProfile? profile = null;

            if (File.Exists(profilePath))
            {
                try {
                    string json = File.ReadAllText(profilePath);
                    profile = JsonSerializer.Deserialize<UserProfile>(json);
                } catch { /* corrupted file */ }
            }

            if (profile == null || !profile.IsSet)
            {
                // First Run
                WelcomeWindow welcome = new WelcomeWindow();
                if (welcome.ShowDialog() == true)
                {
                    profile = welcome.Profile;
                    string json = JsonSerializer.Serialize(profile);
                    File.WriteAllText(profilePath, json);
                }
                else
                {
                    // If user cancels, maybe default?
                    profile = new UserProfile { Title = "", LastName = "Utilisateur" };
                }
            }

            // Update Greeting
            if (profile != null)
                UpdateGreeting(profile);
        }

        private void UpdateGreeting(UserProfile profile)
        {
            if (DashboardTitle == null) return;

            int hour = DateTime.Now.Hour;
            string greeting = (hour >= 18 || hour < 5) ? "Bonsoir" : "Bonjour";

            string namePart = "";
            if (!string.IsNullOrWhiteSpace(profile.Title)) namePart += profile.Title + " ";
            if (!string.IsNullOrWhiteSpace(profile.LastName)) namePart += profile.LastName;

            // Updated greeting to be more personal as requested
            DashboardTitle.Text = $"👋 {greeting}, {namePart.Trim()}";
            DashboardTitle.FontSize = 32; 
        }

        private void LoadProjects()
        {
            var projects = _projectService.GetAllProjects().OrderByDescending(p => p.CreatedOn).ToList();
            
            // To dashboard recent items
            if (RecentProjectsListBox != null)
            {
                RecentProjectsListBox.ItemsSource = projects.Take(4);
            }
            
        
            // Dashboard Lists by Status/Team
            if (ProjectsListTeam != null)
            {
                ProjectsListTeam.ItemsSource = projects.Where(p => 
                    (p.Status != ProjectStatus.Completed && p.Status != ProjectStatus.Archived) && 
                    (p.Team != null && p.Team.Any())
                ).Take(6).ToList();
            }

            if (ProjectsListIndividual != null)
            {
                ProjectsListIndividual.ItemsSource = projects.Where(p => 
                    (p.Status != ProjectStatus.Completed && p.Status != ProjectStatus.Archived) && 
                    (p.Team == null || !p.Team.Any())
                ).Take(6).ToList();
            }
            
            if (ProjectsListCompleted != null)
            {
                ProjectsListCompleted.ItemsSource = projects.Where(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Archived).Take(6).ToList();
            }

            // Dashboard Quick Stats
            if (StatOngoing != null) StatOngoing.Text = projects.Count(p => p.Status == ProjectStatus.Ongoing).ToString();
            if (StatValidated != null) StatValidated.Text = projects.Count(p => p.Status == ProjectStatus.Completed).ToString();


            // REFRESH LIBRARY
            RefreshLibrary(projects);
        }

        private void RefreshLibrary(List<ResearchProject>? allProjects = null)
        {
            if (LibraryGrid == null) return;
            var projects = allProjects ?? _projectService.GetAllProjects();

            // Filter logic
            if (LibraryStatusFilter != null && LibraryStatusFilter.SelectedIndex > 0)
            {
                if (LibraryStatusFilter.SelectedIndex == 1) // Publié
                    projects = projects.Where(p => p.Status == ProjectStatus.Published).ToList();
                else if (LibraryStatusFilter.SelectedIndex == 2) // Accepté
                    projects = projects.Where(p => p.Status == ProjectStatus.Accepted).ToList();
                else if (LibraryStatusFilter.SelectedIndex == 3) // Terminé
                    projects = projects.Where(p => p.Status == ProjectStatus.Completed).ToList();
            }
            else
            {
                // Default Library view: Show Published, Accepted, Completed
                projects = projects.Where(p => 
                    p.Status == ProjectStatus.Published || 
                    p.Status == ProjectStatus.Accepted || 
                    p.Status == ProjectStatus.Completed).ToList();
            }

            LibraryGrid.ItemsSource = projects.OrderByDescending(p => p.CreatedOn).ToList();
        }

        private void BtnRefreshLibrary_Click(object sender, RoutedEventArgs e)
        {
            LoadProjects(); // Will trigger RefreshLibrary
        }

        private void LibraryFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            LoadProjects();
        }

        private void BtnOpenProjectLibrary_Click(object sender, RoutedEventArgs e)
        {
             if (sender is Button btn && btn.DataContext is ResearchProject project)
             {
                 OpenProjectWindow(project);
             }
        }

        private void OpenProjectWindow(ResearchProject project)
        {
            // Optional: Check if a window is already open for this project
            ProjectWindow projectWin = new ProjectWindow(project);
            projectWin.Show();
        }

        private void LoadProjectIntoUI(ResearchProject project)
        {
            if (project == null) return;
            _currentProject = project;

            // Project-specific UI state is now managed by ProjectWindow
        }

        private void Ribbon_Checked(object sender, RoutedEventArgs e)
        {
            if (MainViewTabControl == null) return;

            // Navigation Logic: Switch Main View based on Ribbon Tab (Home Screen views only)
            if (RibbonFile != null && RibbonFile.IsChecked == true)
            {
                MainViewTabControl.SelectedItem = ViewDash;
            }
            else if (RibbonHelp != null && RibbonHelp.IsChecked == true)
            {
                MainViewTabControl.SelectedItem = ViewHelp;
            }
        }


        private void UpdateLicenseDisplay()
        {
            if (LicenseStatusText != null)
            {
                _licensingService.IsActivated(out string status);
                string tier = _featureManager.GetTierName();
                LicenseStatusText.Text = $"{status} - {tier}";
            }

            if (BtnCloudSync != null)
            {
                BtnCloudSync.IsEnabled = _featureManager.IsFeatureAvailable(AppFeature.CloudSync);
                if (!BtnCloudSync.IsEnabled) 
                    BtnCloudSync.ToolTip = "☁️ Synchronisation Cloud (Réservé aux versions Institutionnelle/Elite)";
            }

            if (BtnUpgrade != null)
            {
                var license = _licensingService.GetCurrentLicense();
                BtnUpgrade.Visibility = (license != null && (license.Type == LicenseType.Student || license.Type == LicenseType.Annual)) 
                                        ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OpenUpgrade_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Rendez-vous sur www.adrev-science.com pour mettre à jour votre licence et débloquer toutes les fonctionnalités (Analyses de Régression, Atelier Qualitatif, etc.) !", "Mise à Niveau");
        }

        private void OpenActivation_Click(object sender, RoutedEventArgs e)
        {
            ActivationWindow activation = new ActivationWindow();
            if (activation.ShowDialog() == true)
            {
                UpdateLicenseDisplay();
            }
        }


        

        private void HelpAction_Click(object sender, RoutedEventArgs e)
        {
             if (sender is Button btn && btn.Tag is string action)
             {
                 if (action == "Documentation")
                 {
                     var helpWin = new HelpWindow();
                     helpWin.Owner = this;
                     helpWin.Show();
                 }
                 else if (action == "About")
                 {
                     var helpWin = new HelpWindow();
                     helpWin.Owner = this;
                     helpWin.Show();
                     helpWin.LoadTopic("About");
                 }
                 else
                 {
                     MessageBox.Show($"Action Support: {action}\n\n(Veuillez contacter le support informatique à support@adrev.org)", "Support AdRev", MessageBoxButton.OK, MessageBoxImage.Information);
                 }
             }
        }

        // --- Quick Access Toolbar Handlers ---
        private void QuickSave_Click(object sender, RoutedEventArgs e) 
        {
             SaveFullProject();
             MessageBox.Show("Projet et données sauvegardés.", "AdRev");
        }
        private void QuickOpen_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Ouvrir un projet.", "AdRev");
        private void QuickShare_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Partage du projet.", "AdRev");
        private void QuickUndo_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Annuler la dernière action.", "AdRev");


        private void OpenTeamWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProject == null)
            {
                MessageBox.Show("Veuillez d'abord ouvrir ou créer un projet pour gérer l'équipe.", "Aucun projet ouvert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var win = new TeamWindow(_currentProject);
            win.Owner = this;
            win.ShowDialog();
        }

        private void TabHome_Click(object sender, RoutedEventArgs e) => MainViewTabControl.SelectedItem = ViewDash;
        private void TabNewProject_Click(object sender, RoutedEventArgs e) => MainViewTabControl.SelectedItem = ViewNewProject;


        // --- Utilities ---
        
        private TextBlock CreateHeader(string text) => new TextBlock { Text = text, FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 20), Foreground = (System.Windows.Media.Brush)Application.Current.Resources["PrimaryBrush"] };
        
        private TextBlock CreateError(string text) => new TextBlock { Text = "❌ " + text, Foreground = Brushes.Red, Margin = new Thickness(0, 10, 0, 0) };

        private Grid CreateInfoRow(string label, string value)
        {
            var g = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold });
            var v = new TextBlock { Text = value };
            Grid.SetColumn(v, 1);
            g.Children.Add(v);
            return g;
        }

        private static bool IsNumeric(VariableType type) => type == VariableType.QuantitativeDiscrete || type == VariableType.QuantitativeContinuous;



        








        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();











        private void BrowseProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Sélectionner le dossier du projet";
            dialog.Multiselect = false;
            
            if (dialog.ShowDialog() == true)
            {
                ProjectPathTextBox.Text = dialog.FolderName;
            }
        }

        private void ImportProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Importer un dossier projet existant";
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var project = _projectService.ImportFromFolder(dialog.FolderName);
                    LoadProjects();
                    MessageBox.Show($"Le projet '{project.Title}' a été importé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'importation: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            var title = TitleTextBox.Text;

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Veuillez saisir un titre.");
                return;
            }

            var selectedStudyType = (StudyType)StudyTypeComboBox.SelectedItem;
            var selectedContext = (ProjectContext)ProjectContextComboBox.SelectedItem;
            var selectedDomain = (ScientificDomain)DomainComboBox.SelectedItem;
            var authors = AuthorsTextBox.Text;
            var institution = InstitutionTextBox.Text;
            
            string? customPath = null;
            if (ProjectPathTextBox.Text != "Documents/AdRev/Projects (Défaut)" && Directory.Exists(ProjectPathTextBox.Text))
            {
                customPath = ProjectPathTextBox.Text;
            }

            var project = _projectService.CreateNew(
                title,
                selectedStudyType,
                selectedContext,
                selectedDomain,
                authors,
                institution,
                customPath
            );

            _currentProject = project;

            ResultTextBlock.Text =
                $"Projet créé : {project.Title} ({project.StudyType})\nOuverture du projet...";
            
            LoadProjects();

            // Open Project Window
            OpenProjectWindow(project);

            // Switch to dashboard
            MainViewTabControl.SelectedItem = ViewDash;
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ResearchProject project)
            {
                OpenProjectWindow(project);
            }
        }



        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ResearchProject project)
            {
                var result = MessageBox.Show($"Voulez-vous vraiment supprimer le projet '{project.Title}' ?", 
                    "Confirmation de suppression", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    _projectService.DeleteProject(project);
                    LoadProjects();
                    MessageBox.Show("Projet supprimé avec succès.");
                }
            }
        }

        private void SaveFullProject()
        {
            if (_currentProject == null || string.IsNullOrEmpty(_currentProject.FilePath)) return;

            try
            {
                _projectService.SaveProject(_currentProject);
                // Folder structure is now handled by ProjectWindow or during creation
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde: {ex.Message}", "Erreur Sauvegarde", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Project-specific views have been moved to ProjectWindow





        // --- Custom Window Chrome Handlers ---
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var openProjectWindows = Application.Current.Windows.OfType<AdRev.Desktop.Windows.ProjectWindow>().ToList();
            if (openProjectWindows.Any())
            {
                var result = MessageBox.Show(
                    "Des fenêtres de projet sont encore ouvertes. Voulez-vous vraiment fermer l'application principale ?\n(Toutes les fenêtres de projet seront fermées)",
                    "Confirmation de Fermeture",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }




        private void LibraryMarkAccepted_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is ResearchProject project)
            {
                project.Status = ProjectStatus.Accepted;
                _projectService.SaveProject(project);
                LoadProjects();
                MessageBox.Show($"Le projet '{project.Title}' a été marqué comme Accepté.", "Statut Mis à Jour", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LibraryMarkPublished_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is ResearchProject project)
            {
                project.Status = ProjectStatus.Published;
                if (!project.PublicationDate.HasValue) project.PublicationDate = DateTime.Now;
                
                _projectService.SaveProject(project);
                LoadProjects();
                MessageBox.Show($"Le projet '{project.Title}' a été marqué comme Publié.", "Statut Mis à Jour", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenProfile_Click(object sender, RoutedEventArgs e)
        {
            WelcomeWindow welcome = new WelcomeWindow();
            if (welcome.ShowDialog() == true)
            {
                var profile = welcome.Profile;
                if (profile != null)
                {
                    string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdRev");
                    string profilePath = Path.Combine(appData, "user_profile.json");
                    string json = JsonSerializer.Serialize(profile);
                    File.WriteAllText(profilePath, json);
                    UpdateGreeting(profile);
                }
            }
        }
    }
}
