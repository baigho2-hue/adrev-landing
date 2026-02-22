using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AdRev.Desktop
{
    public class ProjectViewModel
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? LastModified { get; set; }
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<ProjectViewModel> RecentProjects { get; set; }
        private AdRev.Domain.Models.ResearchProject? _currentProject;

        // Track open independent windows
        private List<Window> _openProtocolWindows = new List<Window>();
        private readonly AdRev.Core.Services.ResearcherProfileService _profileService = new AdRev.Core.Services.ResearcherProfileService();

        // Embedded Views (Keep others embedded for now)
        private AdRev.Desktop.Views.Project.DataEntryView? _dataEntryView;
        private AdRev.Desktop.Views.Project.AnalysisView? _analysisView;
        private AdRev.Desktop.Views.Project.DiscussionView? _discussionView;
        private AdRev.Desktop.Views.Project.FinalReportView? _finalReportView;

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this; // Ensure specific reference if needed

            RecentProjects = new ObservableCollection<ProjectViewModel>();
            
            DataContext = this;
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            var profile = _profileService.GetProfile();
            UserProfileName.Text = profile.FullName;
            UserProfileTitle.Text = profile.Title;
        }

        private void EditProfile_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var win = new AdRev.Desktop.Windows.ProfileWindow();
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                LoadUserProfile();
            }
        }

        private void EnsureProjectViews()
        {
             if (_currentProject == null) return;

             // ProtocolView removed from here as it is now a Window
             
             if (_dataEntryView == null) _dataEntryView = new AdRev.Desktop.Views.Project.DataEntryView();
             _dataEntryView.LoadProject(_currentProject);

             if (_analysisView == null) _analysisView = new AdRev.Desktop.Views.Project.AnalysisView();
             _analysisView.LoadProject(_currentProject);

             if (_discussionView == null) _discussionView = new AdRev.Desktop.Views.Project.DiscussionView();
             _discussionView.LoadProject(_currentProject);

             if (_finalReportView == null) _finalReportView = new AdRev.Desktop.Views.Project.FinalReportView();
             _finalReportView.LoadProject(_currentProject);
        }

        private void SwitchView_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string tag)
            {
                // Reset UI for embedded views
                if (tag != "Protocols" && tag != "Help") // Don't hide dashboard if we just open a window
                {
                    DashboardContent.Visibility = Visibility.Collapsed;
                    ActiveViewContent.Visibility = Visibility.Collapsed;
                    PageTitle.Text = tag;
                }

                UpdateActiveMenu(btn);

                switch (tag)
                {
                    case "Dashboard":
                        DashboardContent.Visibility = Visibility.Visible;
                        PageTitle.Text = "Tableau de bord";
                        break;
                    case "Protocols":
                        // No active view change for separate window, but we keep highlighting or reset?
                        // User request implies highlighting the active menu.
                        // If separate window, maybe we shouldn't highlight or should we?
                        // "active la surbrillance du menu active" -> Let's highlight it.
                        if (_currentProject == null)
                        {
                             MessageBox.Show("Veuillez d'abord créer ou ouvrir un projet.", "Aucun Projet");
                             return;
                        }
                        OpenProjectWindow(_currentProject, 0);
                        break;
                    case "DataEntry":
                        PageTitle.Text = "Saisie de Données";
                         if (_currentProject == null)
                        {
                             MessageBox.Show("Veuillez d'abord créer ou ouvrir un projet.", "Aucun Projet");
                             return;
                        }
                        OpenProjectWindow(_currentProject, 2);
                        break;
                    case "Analysis":
                        // 1. Ask for Title / Author
                        var introWin = new AdRev.Desktop.Windows.QuickAnalysisIntroWindow();
                        introWin.Owner = this;
                        if (introWin.ShowDialog() == true)
                        {
                            // 2. Create Project
                            var quickProject = new AdRev.Domain.Models.ResearchProject
                            {
                                Title = introWin.ProjectTitle,
                                Authors = introWin.AuthorName,
                                CreatedOn = DateTime.Now,
                                Status = AdRev.Domain.Models.ProjectStatus.Ongoing
                            };
                            // Add PI to Team
                            quickProject.Team.Add(new AdRev.Domain.Models.Author 
                            { 
                                LastName = introWin.AuthorName, 
                                Role = AdRev.Domain.Models.FunctionalRole.PrincipalInvestigator 
                            });

                            _currentProject = quickProject;

                            // 3. Open Project Window at Analysis Tab (Index 3)
                            OpenProjectWindow(_currentProject, 3);
                        }
                        break;
                    case "Discussion":
                        PageTitle.Text = "Discussion";
                         if (_currentProject == null)
                        {
                             MessageBox.Show("Veuillez d'abord créer ou ouvrir un projet.", "Aucun Projet");
                             return;
                        }
                        OpenProjectWindow(_currentProject, 6);
                        break;
                    case "Report":
                        PageTitle.Text = "Rapport Final";
                         if (_currentProject == null)
                        {
                             MessageBox.Show("Veuillez d'abord créer ou ouvrir un projet.", "Aucun Projet");
                             return;
                        }
                        OpenProjectWindow(_currentProject, 7);
                        break;
                    case "Help":
                        var helpWin = new HelpWindow();
                        helpWin.Owner = this;
                        helpWin.Show();
                        break;
                    case "Projects":
                        PageTitle.Text = "Liste des Projets";
                        ActiveViewContent.Content = new AdRev.Desktop.Views.Project.ProjectListView();
                        ActiveViewContent.Visibility = Visibility.Visible;
                        break;
                    case "ExportFolder":
                        PageTitle.Text = "Exporter un Dossier";
                        ActiveViewContent.Content = new System.Windows.Controls.TextBlock { Text = "Fonctionnalité d'exportation de dossier de recherche (À venir)", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                        ActiveViewContent.Visibility = Visibility.Visible;
                        break;
                    case "MobileSync":
                        var syncWin = new MobileSyncWindow();
                        syncWin.Owner = this;
                        syncWin.ShowDialog();
                        break;
                }
            }
        }

        private void UpdateActiveMenu(System.Windows.Controls.Button activeBtn)
        {
            // Reset all buttons to default style
            var buttons = new List<System.Windows.Controls.Button> { BtnDashboard, BtnProjects, BtnAnalysis, BtnExportFolder, BtnMobileSync }; // Add others if they were re-added
            
            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                // Default Style
                btn.Background = System.Windows.Media.Brushes.Transparent;
                btn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#64748B"));
            }

            // Highlight active
            if (activeBtn != null)
            {
                activeBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EEF2FF"));
                activeBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#6366F1"));
            }
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var win = new NewProjectWindow();
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                _currentProject = win.CreatedProject;
                
                // Launch Project Window immediately (Tab 0: Protocol)
                OpenProjectWindow(_currentProject, 0);
            }
        }

        private void OpenProjectWindow(AdRev.Domain.Models.ResearchProject project, int initialTabIndex = 0)
        {
            // Check if already open
            foreach (var win in _openProtocolWindows)
            {
                if (win is AdRev.Desktop.Windows.ProjectWindow pw && pw.Title.Contains(project.Title)) // Simple check
                {
                    pw.Activate();
                    return;
                }
            }

            var pWin = new AdRev.Desktop.Windows.ProjectWindow(project, initialTabIndex);
            pWin.Closed += (s, args) => _openProtocolWindows.Remove(pWin);
            _openProtocolWindows.Add(pWin);
            pWin.Show();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_openProtocolWindows.Count > 0)
            {
                var result = MessageBox.Show(
                    "Des fenêtres de protocole sont ouvertes. Voulez-vous tout fermer ?", 
                    "Fermeture de l'application", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Close all children first
                    // Create a copy to iterate because closing modifies the list via event handler
                    var windows = new List<Window>(_openProtocolWindows);
                    foreach (var win in windows)
                    {
                        win.Close();
                    }
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnClosing(e);
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ProjectViewModel vm)
            {
                MessageBox.Show($"Ouverture de {vm.Name}...", "Ouverture");
                // In real app, load from DB
                // Then call OpenProtocolWindow(loadedProject);
            }
        }

        private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string lang)
            {
                if (Application.Current is App app)
                {
                    app.SetLanguage(lang);
                    // Refresh current view title if applicable
                    // This is a simple way, for full localized UI we usually use Bindings with DynamicResource
                }
            }
        }
    }
}
