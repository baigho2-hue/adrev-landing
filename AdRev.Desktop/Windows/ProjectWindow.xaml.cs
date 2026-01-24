using System;
using System.Windows;
using System.Windows.Controls;
using AdRev.Domain.Models;
using AdRev.Domain.Enums;
using AdRev.Core.Common;

namespace AdRev.Desktop.Windows
{
    public partial class ProjectWindow : Window
    {
        private ResearchProject _project;
        private readonly ResearchProjectService _projectService = new ResearchProjectService();
        private readonly FeatureManager _featureManager;

        public ProjectWindow(ResearchProject project)
        {
            _featureManager = new FeatureManager(new AdRev.Core.Services.LicensingService());
            InitializeComponent();
            _project = project;
            ApplyGating();
            LoadProject();
        }

        private void ApplyGating()
        {
            // Quali Features (Elite)
            bool hasQuali = _featureManager.IsFeatureAvailable(AppFeature.QualitativeAnalysis);
            RibbonStep5.IsEnabled = hasQuali;
            RibbonStep5b.IsEnabled = hasQuali;
            if (!hasQuali)
            {
                RibbonStep5.ToolTip = "🔒 Atelier Qualitatif (Réservé à l'édition Elite)";
                RibbonStep5b.ToolTip = "🔒 Codage Thématique (Réservé à l'édition Elite)";
            }

            // Quality Check (Pro)
            bool hasQuality = _featureManager.IsFeatureAvailable(AppFeature.QualityValidation);
            RibbonStep9.IsEnabled = hasQuality;
            if (!hasQuality)
            {
                RibbonStep9.ToolTip = "🔒 Grilles de Qualité CONSORT/STROBE (Réservé à l'édition Pro/Elite)";
            }
        }

        private void LoadProject()
        {
            if (_project == null) return;
            TxtProjectTitleHeader.Text = _project.Title;
            StatusBarProjectStatus.Text = _project.Status.ToString();
            
            // Pass project to all views
            ViewProtocol.LoadProject(_project);
            ViewVariableDesign.LoadProject(_project);
            ViewDataEntry.LoadProject(_project);
            ViewAnalysis.LoadProject(_project);
            ViewQualitativeAnalysis.LoadProject(_project);
            ViewQualitativeCoding.LoadProject(_project);
            ViewDiscussion.LoadProject(_project);
            ViewFinalReport.LoadProject(_project);
            ViewJournalArticle.LoadProject(_project);
            ViewQualityCheck.LoadProject(_project);

            // Initial progress update
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            // Reset all
            StepProtocol.Opacity = 0.4;
            StepVariables.Opacity = 0.4;
            StepData.Opacity = 0.4;
            StepAnalysis.Opacity = 0.4;
            StepReport.Opacity = 0.4;

            // Simple logic for progress visualization
            StepProtocol.Opacity = 1.0; 
            if (_project.Variables != null && _project.Variables.Count > 0) StepVariables.Opacity = 1.0;
            // Add more conditions as needed...
        }

        private void Ribbon_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (sender is RadioButton rb && ProjectTabControl != null && rb.Tag != null)
            {
                if (int.TryParse(rb.Tag.ToString(), out int index))
                {
                    ProjectTabControl.SelectedIndex = index;
                }
            }
        }

        private void QuickSave_Click(object sender, RoutedEventArgs e)
        {
            _projectService.SaveProject(_project);
            StatusBarLastSaved.Text = $"Dernière sauvegarde : {DateTime.Now:HH:mm:ss}";
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
