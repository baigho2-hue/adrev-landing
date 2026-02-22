using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AdRev.Domain.Models;
using AdRev.Domain.Quality;
using AdRev.Core.Services;

namespace AdRev.Desktop.Views.Project
{
    public partial class QualityCheckView : UserControl
    {
        private ResearchProject? _project;
        private readonly QualityService _qualityService = new QualityService();

        public QualityCheckView()
        {
            InitializeComponent();
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
            LoadChecklists();
        }

        private void LoadChecklists()
        {
            if (_project == null) return;

            var checklists = _qualityService.GetRecommendedChecklists(_project);
            if (!checklists.Any())
            {
                checklists = _qualityService.GetAllChecklists();
            }

            QualityChecklistCombo.ItemsSource = checklists;
            if (checklists.Count > 0)
                QualityChecklistCombo.SelectedIndex = 0;
        }

        private void QualityChecklistCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QualityChecklistCombo.SelectedItem is QualityChecklist checklist)
            {
                QualityChecklistDesc.Text = checklist.Description;
                QualityChecklistTitle.Text = $"Checklist : {checklist.Name}";
                RenderQualityChecklist(checklist);
            }
        }

        private void RenderQualityChecklist(QualityChecklist checklist)
        {
            QualityItemsPanel.Children.Clear();

            foreach (var section in checklist.Sections)
            {
                // Section Header
                var secHeader = new TextBlock 
                { 
                    Text = section.Name, 
                    FontSize = 16, 
                    FontWeight = FontWeights.Bold, 
                    Foreground = Brushes.Navy,
                    Margin = new Thickness(0, 15, 0, 10)
                };
                QualityItemsPanel.Children.Add(secHeader);

                // Items
                foreach (var item in section.Items)
                {
                    var itemPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 15) };
                    
                    var cb = new CheckBox 
                    { 
                        Content = new TextBlock { Text = item.Requirement, TextWrapping = TextWrapping.Wrap },
                        IsChecked = item.IsMet,
                        FontSize = 14
                    };
                    
                    cb.Checked += (s, ev) => { item.IsMet = true; UpdateQualityScore(checklist); };
                    cb.Unchecked += (s, ev) => { item.IsMet = false; UpdateQualityScore(checklist); };

                    itemPanel.Children.Add(cb);

                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        itemPanel.Children.Add(new TextBlock 
                        { 
                            Text = item.Description, 
                            FontSize = 11, 
                            Foreground = Brushes.Gray, 
                            Margin = new Thickness(25, 2, 0, 0),
                            TextWrapping = TextWrapping.Wrap
                        });
                    }

                    QualityItemsPanel.Children.Add(itemPanel);
                }
            }
            UpdateQualityScore(checklist);
        }

        private void UpdateQualityScore(QualityChecklist checklist)
        {
            int total = 0;
            int met = 0;

            foreach (var section in checklist.Sections)
            {
                total += section.Items.Count;
                met += section.Items.Count(i => i.IsMet);
            }

            QualityScoreText.Text = $"{met} / {total}";
            QualityScoreProgress.Maximum = total;
            QualityScoreProgress.Value = met;
        }

        private void GenerateQualityReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Rapport de conformité généré (Simulation).", "AdRev Quality");
        }
    }
}
