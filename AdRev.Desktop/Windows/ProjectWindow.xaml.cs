using System;
using System.Windows;
using System.Windows.Controls;
using AdRev.Domain.Models;
using AdRev.Domain.Enums;
using AdRev.Core.Common;
<<<<<<< HEAD
using AdRev.Core.Services;
using System.Windows.Media;
using System.Collections.Generic;
using AdRev.Domain.Variables;
=======
>>>>>>> origin/main

namespace AdRev.Desktop.Windows
{
    public partial class ProjectWindow : Window
    {
        private ResearchProject _project;
        private readonly ResearchProjectService _projectService = new ResearchProjectService();
        private readonly FeatureManager _featureManager;

<<<<<<< HEAD
        public ProjectWindow(ResearchProject project, int initialTabIndex = 0)
=======
        public ProjectWindow(ResearchProject project)
>>>>>>> origin/main
        {
            _featureManager = new FeatureManager(new AdRev.Core.Services.LicensingService());
            InitializeComponent();
            _project = project;
<<<<<<< HEAD
            
            // Force initial tab selection
            ProjectTabControl.SelectedIndex = initialTabIndex;

=======
>>>>>>> origin/main
            ApplyGating();
            LoadProject();
        }

<<<<<<< HEAD
        public void ApplyGating()
        {
            if (RibbonStep5 == null || RibbonStep5b == null) return;

            bool hasQualiFeature = _featureManager.IsFeatureAvailable(AppFeature.QualitativeAnalysis);
            bool hasQualityFeature = _featureManager.IsFeatureAvailable(AppFeature.QualityValidation);

            bool isQualiOrMixed = _project.StudyType == StudyType.Qualitative || _project.StudyType == StudyType.Mixed;
            bool isQuantiOrMixed = _project.StudyType == StudyType.Quantitative || _project.StudyType == StudyType.Mixed;

            var qualiVisibility = (hasQualiFeature && isQualiOrMixed) ? Visibility.Visible : Visibility.Collapsed;
            RibbonStep5.Visibility = qualiVisibility;
            RibbonStep5b.Visibility = qualiVisibility;
            RibbonStep4.Visibility = isQuantiOrMixed ? Visibility.Visible : Visibility.Collapsed;

            // Visibility for Library
            RibbonLibrary.Visibility = Visibility.Visible;

            if (!hasQualiFeature)
            {
                RibbonStep5.IsEnabled = false;
                RibbonStep5b.IsEnabled = false;
            }

            RibbonStep9.IsEnabled = hasQualityFeature;
=======
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
>>>>>>> origin/main
        }

        private void LoadProject()
        {
            if (_project == null) return;
            TxtProjectTitleHeader.Text = _project.Title;
            StatusBarProjectStatus.Text = _project.Status.ToString();
            
<<<<<<< HEAD
            ViewDashboard.LoadProject(_project);
=======
            // Pass project to all views
>>>>>>> origin/main
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
<<<<<<< HEAD
            ViewLibrary.LoadProject(_project);

=======

            // Initial progress update
>>>>>>> origin/main
            UpdateProgress();
        }

        private void UpdateProgress()
        {
<<<<<<< HEAD
=======
            // Reset all
>>>>>>> origin/main
            StepProtocol.Opacity = 0.4;
            StepVariables.Opacity = 0.4;
            StepData.Opacity = 0.4;
            StepAnalysis.Opacity = 0.4;
            StepReport.Opacity = 0.4;

<<<<<<< HEAD
            StepProtocol.Opacity = 1.0; 
            if (_project.Variables != null && _project.Variables.Count > 0) StepVariables.Opacity = 1.0;
=======
            // Simple logic for progress visualization
            StepProtocol.Opacity = 1.0; 
            if (_project.Variables != null && _project.Variables.Count > 0) StepVariables.Opacity = 1.0;
            // Add more conditions as needed...
>>>>>>> origin/main
        }

        private void Ribbon_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (sender is RadioButton rb && ProjectTabControl != null && rb.Tag != null)
            {
                if (int.TryParse(rb.Tag.ToString(), out int index))
                {
                    ProjectTabControl.SelectedIndex = index;
<<<<<<< HEAD
                    if (index == 0) ViewDashboard.LoadProject(_project);
                    if (index == 11) ViewLibrary.LoadProject(_project);
=======
>>>>>>> origin/main
                }
            }
        }

        private void QuickSave_Click(object sender, RoutedEventArgs e)
        {
            _projectService.SaveProject(_project);
<<<<<<< HEAD
            StatusBarLastSaved.Text = $"{App.GetString("LabelLastSaved")} {DateTime.Now:HH:mm:ss}";
            ViewDashboard.LoadProject(_project);
        }

        public void SyncVariablesToAllViews()
        {
            if (_project == null) return;
            ViewProtocol.LoadVariables(_project.Variables ?? new List<StudyVariable>());
            ViewVariableDesign.LoadVariables(_project.Variables ?? new List<StudyVariable>(), _project.StudyType == StudyType.Qualitative);
            ViewDataEntry.LoadProject(_project);
            UpdateProgress();
=======
            StatusBarLastSaved.Text = $"Dernière sauvegarde : {DateTime.Now:HH:mm:ss}";
>>>>>>> origin/main
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
