using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using AdRev.Domain.Models;

namespace AdRev.Desktop.Views.Project
{
    public partial class QualitativeAnalysisView : UserControl
    {
        private ResearchProject? _project;

        public QualitativeAnalysisView()
        {
            InitializeComponent();
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
            UpdateDashboard();
        }

        private void UpdateDashboard()
        {
            if (_project == null) return;

            // Mock/Simplified calculation
            int verbatimCount = _project.QualitativeSources?.Count ?? 0;
            int codeCount = _project.QualitativeCodes?.Count ?? 0;
            
            TxtVerbatimCount.Text = verbatimCount.ToString();
            TxtCodeCount.Text = codeCount.ToString();
            TxtThemeCount.Text = "0"; // Theme property no longer exists on QualitativeCode

            // Word Cloud & Frequency would need actual content parsing
            // For now, we show placeholders or basic list
            CodeFrequencyList.Items.Clear();
            if (_project.QualitativeCodes != null)
            {
                foreach (var code in _project.QualitativeCodes.Take(10))
                {
                    CodeFrequencyList.Items.Add($"{code.Name} ({new Random().Next(1, 10)})");
                }
            }
        }
    }
}
