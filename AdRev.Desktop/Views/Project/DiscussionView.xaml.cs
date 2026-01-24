using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using AdRev.Domain.Models;
using AdRev.Domain.Enums;

namespace AdRev.Desktop.Views.Project
{
    public partial class DiscussionView : UserControl
    {
        private ResearchProject? _project;

        public DiscussionView()
        {
            InitializeComponent();
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
            LoadDiscussionData();
        }

        private void LoadDiscussionData()
        {
            if (_project == null) return;

            TxtDiscussionMain.Text = _project.DiscussionContent;
            TxtDiscussionLimits.Text = _project.LimitationsContent;
            CitationsListBox.ItemsSource = _project.References;

            UpdateDiscussionSupport();
        }

        private void UpdateDiscussionSupport()
        {
            if (_project == null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("✨ CONSEILS D'EXPERT :");
            if (_project.StudyType == StudyType.Quantitative)
            {
                sb.AppendLine("• Discutez la validité interne et les biais de sélection.");
                sb.AppendLine("• Comparez vos p-values avec la littérature.");
            }
            else
            {
                sb.AppendLine("• Soulignez la richesse des verbatims.");
                sb.AppendLine("• Discutez de votre propre réflexité.");
            }
            
            TxtDiscussionExpertTips.Text = sb.ToString();
        }

        private void QuickAddCitation_Click(object sender, RoutedEventArgs e)
        {
            if (_project == null || string.IsNullOrWhiteSpace(TxtDoiSearch.Text)) return;

            var citation = new Citation
            {
                Title = "Référence : " + TxtDoiSearch.Text,
                Authors = "Auteur Inconnu",
                Year = DateTime.Now.Year.ToString(),
                Doi = TxtDoiSearch.Text
            };

            _project.References.Add(citation);
            CitationsListBox.ItemsSource = null;
            CitationsListBox.ItemsSource = _project.References;
            TxtDoiSearch.Clear();
        }

        private void InsertCitation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Citation citation)
            {
                var target = TxtDiscussionMain; // Or check focused one
                int caret = target.CaretIndex;
                string marker = $" [{citation.Authors}, {citation.Year}]";
                target.Text = target.Text.Insert(caret, marker);
                target.CaretIndex = caret + marker.Length;
                target.Focus();
            }
        }

        private void GenerateSmartDiscussion_Click(object sender, RoutedEventArgs e)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("1. RÉSUMÉ DES RÉSULTATS CLÉS");
            sb.AppendLine("- Rappel de l'objectif principal.");
            sb.AppendLine();
            sb.AppendLine("2. COMPARAISON AVEC LA LITTÉRATURE");
            sb.AppendLine("- [Citer des études similaires]");
            
            TxtDiscussionMain.Text = sb.ToString();
        }

        private void SaveDiscussion_Click(object sender, RoutedEventArgs e)
        {
            if (_project == null) return;
            _project.DiscussionContent = TxtDiscussionMain.Text;
            _project.LimitationsContent = TxtDiscussionLimits.Text;
            MessageBox.Show("Rédacteur sauvegardé.", "Succès");
        }
    }
}
