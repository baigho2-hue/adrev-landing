using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using AdRev.Domain.Models;
using AdRev.Domain.Variables;
using AdRev.Domain.Enums;
using AdRev.Core.Services;
using Microsoft.Win32;

namespace AdRev.Desktop.Views.Project
{
    public partial class AnalysisView : UserControl
    {
        private ResearchProject? _project;
        private ObservableCollection<AnalysisPlanItem> _analysisPlan = new ObservableCollection<AnalysisPlanItem>();
        private List<Dictionary<string, object>> _projectData = new List<Dictionary<string, object>>();
        private ObservableCollection<StudyVariable> _importedVariables = new ObservableCollection<StudyVariable>();
        private readonly FeatureManager _featureManager;

        public AnalysisView()
        {
            _featureManager = new FeatureManager(new LicensingService());
            InitializeComponent();
            PlanItemsList.ItemsSource = _analysisPlan;
            
            PopulateTestTypes();
        }

        private void PopulateTestTypes()
        {
            PlanTestTypeCombo.Items.Clear();

            // Baseline (Student+)
            PlanTestTypeCombo.Items.Add(new ComboBoxItem { Content = "Description Simple", Tag = "Descriptif" });

            // Professional+ Features
            bool isPro = _featureManager.IsFeatureAvailable(AppFeature.InferentialStats);
            var inferentialItem = new ComboBoxItem { Content = "Comparaison de Moyennes (T-Test) " + (isPro ? "" : "🔒"), Tag = "Comparaison", IsEnabled = isPro };
            PlanTestTypeCombo.Items.Add(inferentialItem);
            
            var assocItem = new ComboBoxItem { Content = "Test d'Association (Chi2) " + (isPro ? "" : "🔒"), Tag = "Association", IsEnabled = isPro };
            PlanTestTypeCombo.Items.Add(assocItem);

            // Elite Features
            bool isElite = _featureManager.IsFeatureAvailable(AppFeature.RegressionAnalysis);
            PlanTestTypeCombo.Items.Add(new ComboBoxItem { Content = "Régression Linéaire " + (isElite ? "" : "🔒"), Tag = "Regression", IsEnabled = isElite });
            PlanTestTypeCombo.Items.Add(new ComboBoxItem { Content = "Régression Logistique " + (isElite ? "" : "🔒"), Tag = "Logistic", IsEnabled = isElite });

            PlanTestTypeCombo.SelectedIndex = 0;
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
            
            // Gate the Qualitative Tab within this view
            if (this.Parent is TabControl tc || VisualTreeHelper.GetParent(this) is TabControl tc2)
            {
                // This might be tricky if the parent isn't immediately available. 
                // Better to gate the tabs in ProjectWindow (already done).
            }

            UpdateAnalysisVariables();
        }

        private void UpdateAnalysisVariables()
        {
            // Update UI with available variables
            // In a real app, populate some variable selection combos
        }

        private void ImportData_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*" };
            if (openFileDialog.ShowDialog() == true)
            {
                // Simple CSV parsing (mock)
                ActiveDataSourceText.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
                _projectData = new List<Dictionary<string, object>>(); 
                // In a real app, use a proper CSV service
                 MessageBox.Show("Données importées avec succès (Simulation).", "Importation");
            }
        }

        private void AddToPlan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PlanTitleBox.Text)) return;

            var selectedType = PlanTestTypeCombo.SelectedItem as ComboBoxItem;
            if (selectedType == null) return;

            var newItem = new AnalysisPlanItem
            {
                Title = PlanTitleBox.Text,
                TestType = selectedType.Content?.ToString() ?? "N/A",
                Description = $"Généré le {DateTime.Now:dd/MM/yyyy}"
            };

            _analysisPlan.Add(newItem);
            PlanTitleBox.Text = string.Empty;
        }

        private void RunPlanItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is AnalysisPlanItem item)
            {
                ExecuteAnalysis(item);
            }
        }

        private void ExecuteAnalysis(AnalysisPlanItem item)
        {
            AnalysisDoc.Blocks.Clear();
            
            // Header
            AnalysisDoc.Blocks.Add(new Paragraph(new Run(item.Title)) { FontSize = 18, FontWeight = FontWeights.Bold, Foreground = Brushes.Navy, TextAlignment = TextAlignment.Center });
            AnalysisDoc.Blocks.Add(new Paragraph(new Run($"Test: {item.TestType} | Date: {DateTime.Now}")) { FontSize = 10, Foreground = Brushes.Gray, TextAlignment = TextAlignment.Center });

            if (_projectData.Count == 0)
            {
                AnalysisDoc.Blocks.Add(new Paragraph(new Run("Veuillez d'abord importer des données.")) { Foreground = Brushes.Orange, Margin = new Thickness(0, 20, 0, 0) });
                return;
            }

            // Mock rendering
            AnalysisDoc.Blocks.Add(new Paragraph(new Run("Résultats de l'analyse (Simulés pour la démo)")) { Margin = new Thickness(0, 20, 0, 10), FontWeight = FontWeights.Bold });
            AnalysisDoc.Blocks.Add(new Paragraph(new Run("• Population totale : " + _projectData.Count)));
            AnalysisDoc.Blocks.Add(new Paragraph(new Run("• Test effectué avec succès.")));
            
            item.IsExecuted = true;
            item.ResultSummary = "Exécuté";
        }

        private void GenerateSmartPlan_Click(object sender, RoutedEventArgs e)
        {
            // Simple logic from MainWindow
            _analysisPlan.Add(new AnalysisPlanItem { Title = "Description de l'échantillon", TestType = "Descriptif" });
            MessageBox.Show("Plan intelligent généré.", "Succès");
        }
    }
}
