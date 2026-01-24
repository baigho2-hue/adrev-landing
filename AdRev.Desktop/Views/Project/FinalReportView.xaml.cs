using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdRev.Domain.Models;
using AdRev.Domain.Enums;
using AdRev.Core.Services;
using AdRev.Core.Common;
using AdRev.Desktop.Services;
using Microsoft.Win32;

namespace AdRev.Desktop.Views.Project
{
    public partial class FinalReportView : UserControl
    {
        private ResearchProject? _project;
        private readonly ResearchProjectService _projectService = new ResearchProjectService();

        public FinalReportView()
        {
            InitializeComponent();
            AcademicLevelCombo.ItemsSource = Enum.GetValues(typeof(AcademicLevel));
            AcademicLevelCombo.SelectedIndex = 0;
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
            AcademicLevelCombo.SelectedItem = _project.AcademicLevel;
            TxtFinalConclusion.Text = _project.ConclusionContent;
        }

        private void AcademicLevelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_project != null && AcademicLevelCombo.SelectedItem is AcademicLevel level)
            {
                _project.AcademicLevel = level;
                TargetJournalPanel.Visibility = (level == AcademicLevel.ScientificArticle) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void TargetJournalCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Logic to update target journal in project
        }

        private void ReportBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                TxtWritingAssistantTitle.Text = "Conseils pour la Conclusion";
                TxtWritingAssistantContent.Text = "La conclusion doit répondre à la question de recherche initiale. Évitez d'introduire de nouveaux résultats ici.";
            }
        }

        private void BtnExportFinalReport_Click(object sender, RoutedEventArgs e)
        {
            if (_project == null) return;

            _project.ConclusionContent = TxtFinalConclusion.Text;

            var sfd = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Rapport_{_project.Title}_{DateTime.Now:yyyyMMdd}.docx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    var exporter = new WordExportService();
                    exporter.ExportGlobalReport(_project, sfd.FileName);
                    
                    MessageBox.Show("Le rapport a été généré avec succès.", "Export Réussi");
                    
                    Process.Start(new ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'exportation : {ex.Message}", "Erreur");
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
    }
}
