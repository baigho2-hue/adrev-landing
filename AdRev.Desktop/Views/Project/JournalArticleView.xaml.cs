using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using AdRev.Domain.Models;
using AdRev.Desktop.Services;

namespace AdRev.Desktop.Views.Project
{
    public partial class JournalArticleView : UserControl
    {
        private ResearchProject? _project;
        private readonly JournalService _journalService = new JournalService();

        public JournalArticleView()
        {
            InitializeComponent();
            LoadJournals();
        }

        private void LoadJournals()
        {
            var journals = _journalService.GetCommonJournals();
            ReportJournalCombo.ItemsSource = journals;
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
        }

        private void ReportJournalCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportJournalCombo.SelectedItem is JournalSubmissionCriteria journal)
            {
                JournalTitleText.Text = $"Écriture pour : {journal.JournalName}";
                JournalSummaryPanel.Visibility = Visibility.Visible;
                JournalConstraintsText.Text = $"Abstract: {journal.MaxWordCountAbstract} mots\n" +
                                              $"Corps: {journal.MaxWordCountBody} mots\n" +
                                              $"Style: {journal.CitationStyle}\n" +
                                              $"Réf. Max: {journal.MaxReferences}";
                JournalUrlLink.Tag = journal.GuidelinesUrl;
                TargetWordCountText.Text = $"Cible : {journal.MaxWordCountBody} mots";
            }
        }

        private void JournalUrlLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (JournalUrlLink.Tag is string url && !string.IsNullOrEmpty(url))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible d'ouvrir le lien : {ex.Message}", "Erreur");
                }
            }
        }

        private void GenerateStructure_Click(object sender, RoutedEventArgs e)
        {
            if (ReportJournalCombo.SelectedItem is JournalSubmissionCriteria journal)
            {
                GenerateArticleSkeleton(journal);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une revue d'abord.");
            }
        }

        private void GenerateArticleSkeleton(JournalSubmissionCriteria journal)
        {
            var doc = new FlowDocument();
            
            // Title
            var titlePara = new Paragraph(new Run("[TITRE DE L'ARTICLE]")) { FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, Margin = new Thickness(0,0,0,20) };
            doc.Blocks.Add(titlePara);

            // Abstract
            var absHeader = new Paragraph(new Run("ABSTRACT")) { FontSize = 14, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray };
            doc.Blocks.Add(absHeader);
            doc.Blocks.Add(new Paragraph(new Run($"({(journal.RequiresStructuredAbstract ? "Structuré" : "Libre")} - Max {journal.MaxWordCountAbstract} mots)")) { FontStyle = FontStyles.Italic, Foreground = Brushes.LightGray });
            doc.Blocks.Add(new Paragraph(new Run("")));

            // Sections
            foreach(var sec in journal.RequiredSections)
            {
                var header = new Paragraph(new Run(sec.ToUpper())) { FontSize = 18, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 20, 0, 10), Foreground = Brushes.Navy };
                doc.Blocks.Add(header);
                doc.Blocks.Add(new Paragraph(new Run("Commencez à rédiger ici...")));
            }

            ArticleEditor.Document = doc;
        }

        private void SaveDraft_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Brouillon sauvegardé (Simulation).", "AdRev");
        }
    }
}
