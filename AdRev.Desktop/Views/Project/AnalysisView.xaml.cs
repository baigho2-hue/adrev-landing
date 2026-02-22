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
using AdRev.Desktop.Helpers;

namespace AdRev.Desktop.Views.Project
{
    public partial class AnalysisView : UserControl
    {
        private ResearchProject? _project;
        private ObservableCollection<AnalysisPlanItem> _analysisPlan = new ObservableCollection<AnalysisPlanItem>();
        private List<Dictionary<string, object>> _projectData = new List<Dictionary<string, object>>();
        private ObservableCollection<StudyVariable> _importedVariables = new ObservableCollection<StudyVariable>();
        private readonly FeatureManager _featureManager;
        private readonly InterpretationService _interpretationService;
        private readonly StatisticsService _statisticsService;
        
        private int _tableCount = 0;
        private int _figureCount = 0;

        public AnalysisView()
        {
            _featureManager = new FeatureManager(new LicensingService());
            _interpretationService = new InterpretationService();
            _statisticsService = new StatisticsService();
            InitializeComponent();
            PlanItemsList.ItemsSource = _analysisPlan;
            VariablesList.ItemsSource = _importedVariables;
            
            PopulateTestTypes();
        }

        private void PopulateTestTypes()
        {
            PlanTestTypeCombo.Items.Clear();

            // Baseline (Student+)
            PlanTestTypeCombo.Items.Add(new ComboBoxItem { Content = "Analyse Univariée (Descriptif Complet)", Tag = "Descriptif" });

            // Professional+ Features
            bool isPro = _featureManager.IsFeatureAvailable(AppFeature.InferentialStats);
            
            PlanTestTypeCombo.Items.Add(new ComboBoxItem { Content = "Comparaison de Moyennes (T-Test) " + (isPro ? "" : "🔒"), Tag = "Comparaison", IsEnabled = isPro });
            PlanTestTypeCombo.Items.Add(new ComboBoxItem { Content = "ANOVA (Comparaison > 2 groupes) " + (isPro ? "" : "🔒"), Tag = "ANOVA", IsEnabled = isPro });
            PlanTestTypeCombo.Items.Add(new ComboBoxItem { Content = "Test d'Association (Chi2) " + (isPro ? "" : "🔒"), Tag = "Association", IsEnabled = isPro });

            // Elite Features
            bool isElite = _featureManager.IsFeatureAvailable(AppFeature.RegressionAnalysis);
            
            PlanTestTypeCombo.Items.Add(new ComboBoxItem { Content = "Analyse Multivariée (Modèle Global) " + (isElite ? "" : "🔒"), Tag = "Multivariate", IsEnabled = isElite });
            PlanTestTypeCombo.Items.Add(new ComboBoxItem { Content = "Régression Linéaire " + (isElite ? "" : "🔒"), Tag = "Regression", IsEnabled = isElite });

            PlanTestTypeCombo.SelectedIndex = 0;
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
            
            // Sync Analysis Plan
            _analysisPlan.Clear();
            if (_project.AnalysisPlan != null)
            {
                foreach (var item in _project.AnalysisPlan)
                    _analysisPlan.Add(item);
            }

            // Sync Data from Project (Data Entry)
            if (_project.DataRows != null && _project.DataRows.Count > 0)
            {
                _projectData = _project.DataRows;
                ActiveDataSourceText.Text = "Données du Projet (Saisie)";
            }
            
            // Load Report Content if exists
            if (!string.IsNullOrEmpty(_project.ReportContent))
            {
                try
                {
                    using (var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(_project.ReportContent)))
                    {
                        var flowDocument = (FlowDocument)System.Windows.Markup.XamlReader.Load(stream);
                        AnalysisDoc = flowDocument;
                        AnalysisDocViewer.Document = AnalysisDoc;
                    }
                }
                catch { /* Ignore error on load, maybe invalid XAML */ }
            }

            // Gate the Qualitative Tab within this view
            if (this.Parent is TabControl tc || VisualTreeHelper.GetParent(this) is TabControl tc2)
            {
                // This might be tricky if the parent isn't immediately available. 
                // Better to gate the tabs in ProjectWindow (already done).
            }

            UpdateAnalysisVariables();
        }


        private async void ImportData_Click(object sender, RoutedEventArgs e)
        {
            // Quality Instruction
            // Quality Instruction
            await DialogHelper.ShowMessage("Instructions de Qualité pour l'Importation :\n\n" +
                            "1. La première ligne de votre fichier DOIT contenir les noms des variables (sans cellules vides).\n" +
                            "2. Chaque colonne doit représenter une variable unique.\n" +
                            "3. Les types (chiffres, textes) seront détectés automatiquement.\n\n" +
                            "Veuillez vérifier votre fichier Excel/CSV avant de continuer.", 
                            "Contrôle Qualité Données");

            var openFileDialog = new OpenFileDialog 
            { 
                Filter = "Data files (*.csv, *.xlsx)|*.csv;*.xlsx|CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*" 
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    var importService = new DataImportService();
                    
                    List<Dictionary<string, object>> importedData = new List<Dictionary<string, object>>();
                    string extension = System.IO.Path.GetExtension(filePath).ToLower();

                    if (extension == ".xlsx")
                    {
                        LoadingOverlay.Visibility = Visibility.Visible;
                        await Task.Run(() => importedData = importService.ImportFromExcel(filePath));
                    }
                    else
                    {
                        LoadingOverlay.Visibility = Visibility.Visible;
                        await Task.Run(() => importedData = importService.ImportFromCsv(filePath));
                    }

                    if (importedData.Count > 0)
                    {
                        _projectData = importedData;
                        ActiveDataSourceText.Text = System.IO.Path.GetFileName(filePath);
                        
                        // Infer variables from data
                        var variables = importService.InferVariables(_projectData);
                        _importedVariables.Clear();
                        
                        // SYNC WITH PROJECT
                        if (_project != null)
                        {
                            _project.Variables.Clear(); // For "Quick Analysis", we replace. For continuous, we might append.
                            _project.Variables.AddRange(variables);
                        }

                        foreach (var v in variables) _importedVariables.Add(v);

                        // Refresh UI Selections
                        UpdateAnalysisVariables();

                        await DialogHelper.ShowMessage($"{_projectData.Count} lignes importées avec succès.\n{variables.Count} variables détectées et ajoutées au projet.", "Succès Importation");
                    }
                    else
                    {
                        await DialogHelper.ShowWarning("Le fichier semble vide ou ne contient aucune donnée valide.", "Avertissement");
                    }
                }
                catch (Exception ex)
                {
                    await DialogHelper.ShowError($"Erreur d'importation :\n{ex.Message}\n\nVeuillez corriger votre fichier.", "Erreur Critique");
                }
                finally
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void UpdateAnalysisVariables()
        {
            if (_project == null) return;
            // Populate ComboBoxes
            ComboVariable1.ItemsSource = null;
            ComboVariable1.ItemsSource = _project.Variables;
            
            ComboVariable2.ItemsSource = null;
            ComboVariable2.ItemsSource = _project.Variables;
        }

        private void PlanTestTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = PlanTestTypeCombo.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;
            string tag = selectedItem.Tag?.ToString() ?? "";

            // Reset Visibility
            ComboVariable2.Visibility = Visibility.Collapsed;
            MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboVariable1, "Variable à Analyser");

            if (tag == "Comparaison" || tag == "Association" || tag == "Regression" || tag == "ANOVA" || tag == "Multivariate")
            {
                // Bivariate / Multivariate
                ComboVariable2.Visibility = Visibility.Visible;
                
                if (tag == "Comparaison" || tag == "ANOVA") // T-Test/ANOVA: Var1=Quanti, Var2=Quali(Groups)
                {
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboVariable1, "Variable Quantitative (Moyenne)");
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboVariable2, "Variable de Groupe (Qualitative)");
                }
                else if (tag == "Association") // Chi2: 2 Quali
                {
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboVariable1, "Variable 1 (Ligne)");
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboVariable2, "Variable 2 (Colonne)");
                }
                else if (tag == "Regression" || tag == "Multivariate")
                {
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboVariable1, "Variable Dépendante (Y)");
                    MaterialDesignThemes.Wpf.HintAssist.SetHint(ComboVariable2, "Variable Explicative (X)");
                }
            }
        }

        private void AddToPlan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PlanTitleBox.Text)) return;

            var selectedType = PlanTestTypeCombo.SelectedItem as ComboBoxItem;
            if (selectedType == null) return;

            var v1 = ComboVariable1.SelectedItem as StudyVariable;
            var v2 = ComboVariable2.SelectedItem as StudyVariable;

            var newItem = new AnalysisPlanItem
            {
                Title = PlanTitleBox.Text,
                TestType = selectedType.Content?.ToString() ?? "N/A",
                Description = $"Généré le {DateTime.Now:dd/MM/yyyy}",
                Variable1 = v1?.Name ?? "",
                Variable2 = v2?.Name ?? "",
                IncludeTable = CheckIncludeTable.IsChecked == true,
                IncludeChart = CheckIncludeChart.IsChecked == true
            };

            _analysisPlan.Add(newItem);
            
            // Sync with Project
            if (_project != null)
            {
                _project.AnalysisPlan.Add(newItem);
            }

            PlanTitleBox.Text = string.Empty;
            ComboVariable1.SelectedIndex = -1;
            ComboVariable2.SelectedIndex = -1;
        }

        private async void RunPlanItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is AnalysisPlanItem item)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                await Task.Run(() => {
                     // Simulate slight delay for UX if analysis is too fast
                     System.Threading.Thread.Sleep(500); 
                });
                await ExecuteAnalysis(item);
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ExecuteAnalysis(AnalysisPlanItem item)
        {
            AnalysisDoc.Blocks.Clear();
            _tableCount = 0;
            _figureCount = 0;
            
            // Refresh Data from Project just in case
            if (_project != null && _project.DataRows.Count > 0)
            {
                 _projectData = _project.DataRows;
            }

            // Header
            AnalysisDoc.Blocks.Add(new Paragraph(new Run(item.Title)) { FontSize = 22, FontWeight = FontWeights.Bold, Foreground = Brushes.Navy, TextAlignment = TextAlignment.Center, Margin = new Thickness(0,0,0,15) });

            if (_projectData.Count == 0 && (_project?.Variables == null || _project.Variables.Count == 0))
            {
                AnalysisDoc.Blocks.Add(new Paragraph(new Run("Veuillez d'abord importer des données ou définir des variables.")) { Foreground = Brushes.Orange, Margin = new Thickness(0, 20, 0, 0) });
                return;
            }

            // FILTER & PREPARE DATA
            // Use Project Variables as source of truth
            var targetVariables = _project?.Variables.AsEnumerable() ?? _importedVariables.AsEnumerable();
            
            if (!string.IsNullOrEmpty(item.Variable1))
            {
                targetVariables = targetVariables.Where(v => v.Name == item.Variable1 || v.Name == item.Variable2);
            }
            
            int n = _projectData.Count;
            AnalysisDoc.Blocks.Add(new Section(new Paragraph(new Run($"Population analysée : N={n}")) { FontSize = 14, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkSlateGray }));

            // --- ANALYSIS LOOP ---
            foreach (var variable in targetVariables)
            {
                var rawValues = _projectData.Select(row => row.ContainsKey(variable.Name) ? row[variable.Name] : null).Where(x => x != null).ToList();
                if (rawValues.Count == 0) { AnalysisDoc.Blocks.Add(new Paragraph(new Run($"Aucune donnée valide pour {variable.Name}."))); continue; }

                // 1. STATISTICS TABLE
                // Cast explicit to ensure List<object> (non-nullable)
                var nonNullValues = rawValues.Where(v => v != null).Cast<object>().ToList();

                if (variable.Type == VariableType.QuantitativeContinuous || variable.Type == VariableType.QuantitativeDiscrete)
                {
                    if (item.IncludeTable)
                    {
                        var numericValues = nonNullValues
                            .Select(x => x?.ToString())
                            .Where(s => !string.IsNullOrWhiteSpace(s) && double.TryParse(s, out _))
                            .Select(s => double.Parse(s!))
                            .OrderBy(x => x)
                            .ToList();

                        GenerateDescriptiveTable(variable, numericValues);
                    }
                    
                    if (item.IncludeChart)
                    {
                        GenerateHistogram(variable, nonNullValues);
                    }
                }
                else
                {
                    if (item.IncludeTable)
                    {
                        GenerateFrequencyTable(variable, nonNullValues);
                    }
                    
                    if (item.IncludeChart)
                    {
                        GeneratePieChart(variable, nonNullValues);
                    }
                }
            }

            item.IsExecuted = true;
            item.ResultSummary = $"Réussi ({n} obs)";
        }

        private void GenerateHistogram(StudyVariable variable, List<object> values)
        {
             try 
             {
                 // Robust parsing
                 var numericValues = values
                    .Select(x => x?.ToString())
                    .Where(s => !string.IsNullOrWhiteSpace(s) && double.TryParse(s, out _))
                    .Select(s => double.Parse(s!))
                    .OrderBy(x => x)
                    .ToList();
                 
                 if (numericValues.Count == 0) return;

                 var doubles = numericValues; // Alias for compatibility
                 int binCount = 8;
                 double min = doubles.First();
                 double max = doubles.Last();
                 double width = (max - min) / binCount;
                 if (width == 0) width = 1;

                 var labels = new string[binCount];
                 var counts = new int[binCount];

                 for (int i = 0; i < binCount; i++)
                 {
                     double l = min + (i * width);
                     double h = min + ((i + 1) * width);
                     labels[i] = $"{l:0.#}-{h:0.#}";
                     counts[i] = doubles.Count(v => v >= l && (i == binCount - 1 ? v <= h : v < h));
                 }

                 var chart = new LiveCharts.Wpf.CartesianChart { Height = 250, Margin = new Thickness(0,0,0,0) };
                 chart.Series.Add(new LiveCharts.Wpf.ColumnSeries { Title="N", Values = new LiveCharts.ChartValues<int>(counts), DataLabels=true });
                 chart.AxisX.Add(new LiveCharts.Wpf.Axis { Title = variable.Name, Labels = labels });
                 chart.AxisY.Add(new LiveCharts.Wpf.Axis { Title = "Effectif", MinValue = 0 });

                 CreateEditableChartContainer(variable, chart, values);
             }
             catch { }
        }

        private void GeneratePieChart(StudyVariable variable, List<object> values)
        {
             var groups = values.GroupBy(x => x?.ToString() ?? "N/A").Select(g => new { Label = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).Take(8);
             
             var chart = new LiveCharts.Wpf.PieChart { Height = 250, Margin = new Thickness(0,0,0,0), LegendLocation = LiveCharts.LegendLocation.Right };
             var series = new LiveCharts.SeriesCollection();
             foreach(var g in groups)
                series.Add(new LiveCharts.Wpf.PieSeries { Title = g.Label, Values = new LiveCharts.ChartValues<int> { g.Count }, DataLabels = true });
             
             chart.Series = series;
             CreateEditableChartContainer(variable, chart, values);
        }

        private void CreateEditableChartContainer(StudyVariable variable, LiveCharts.Wpf.Charts.Base.Chart chartControl, List<object> values)
        {
            // Container for the chart
            var chartContainer = new BlockUIContainer();
            var grid = new Grid { Height = 300, Margin = new Thickness(0, 10, 0, 30), Background = Brushes.Transparent };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Row 0: Chart
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 1: Title

            // Editable Title (at the BOTTOM for charts)
            _figureCount++;
            var titleBox = new TextBox 
            { 
                Text = $"Figure {_figureCount} : {(!string.IsNullOrEmpty(variable.Description) ? variable.Description : variable.Name)}", 
                FontWeight = FontWeights.Bold, 
                HorizontalAlignment = HorizontalAlignment.Center,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                FontSize = 14
            };
            Grid.SetRow(titleBox, 1);
            grid.Children.Add(titleBox);

            // Settings Button (Gear Icon)
            var settingsBtn = new Button
            {
                Content = "⚙️",
                FontFamily = new FontFamily("Segoe UI Emoji"),
                ToolTip = "Options du graphique (Type, Axes...)",
                Width = 30, Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, -5, 5, 0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            grid.Children.Add(settingsBtn);

            // Initial Chart (at the TOP for charts)
            Grid.SetRow(chartControl, 0);
            grid.Children.Add(chartControl);

            // Context Menu
            var contextMenu = new ContextMenu();
            
            // 1. Edit Axes
            var editAxesItem = new MenuItem { Header = "Modifier les titres des axes..." };
            editAxesItem.Click += async (s, args) => 
            {
                var currentChart = grid.Children.OfType<LiveCharts.Wpf.Charts.Base.Chart>().FirstOrDefault();
                if (currentChart is LiveCharts.Wpf.CartesianChart cc)
                {
                     string newX = Microsoft.VisualBasic.Interaction.InputBox("Titre de l'axe X :", "Modifier Graphique", cc.AxisX.Count > 0 ? cc.AxisX[0].Title : "Axe X");
                     if (!string.IsNullOrEmpty(newX) && cc.AxisX.Count > 0) cc.AxisX[0].Title = newX;

                     string newY = Microsoft.VisualBasic.Interaction.InputBox("Titre de l'axe Y :", "Modifier Graphique", cc.AxisY.Count > 0 ? cc.AxisY[0].Title : "Axe Y");
                     if (!string.IsNullOrEmpty(newY) && cc.AxisY.Count > 0) cc.AxisY[0].Title = newY;
                }
                else
                {
                    await DialogHelper.ShowMessage("Option non disponible pour ce type de graphique.", "Info");
                }
            };
            contextMenu.Items.Add(editAxesItem);
            
            // 2. Change Chart Type
            var typeMenuItem = new MenuItem { Header = "Changer le type de graphique" };
            
            if (variable.Type == VariableType.QuantitativeContinuous || variable.Type == VariableType.QuantitativeDiscrete)
            {
                // -- Histogramme group --
                var histoMenu = new MenuItem { Header = "Histogrammes" };
                var itemHisto = new MenuItem { Header = "Histogramme (Distribution)" };
                itemHisto.Click += (s, e) => ReplaceChart(grid, variable, values, "Histogram");
                histoMenu.Items.Add(itemHisto);
                typeMenuItem.Items.Add(histoMenu);

                // -- Courbes & Aires group --
                var lineMenu = new MenuItem { Header = "Courbes et Aires" };
                var itemLine = new MenuItem { Header = "Courbe" };
                itemLine.Click += (s, e) => ReplaceChart(grid, variable, values, "Line");
                lineMenu.Items.Add(itemLine);
                
                var itemArea = new MenuItem { Header = "Aires (Surface)" };
                itemArea.Click += (s, e) => ReplaceChart(grid, variable, values, "Area");
                lineMenu.Items.Add(itemArea);
                typeMenuItem.Items.Add(lineMenu);
                
                // -- Barres & Colonnes group --
                var barMenu = new MenuItem { Header = "Barres et Colonnes" };
                var itemBarCol = new MenuItem { Header = "Colonnes (Valeurs Exactes)" };
                itemBarCol.Click += (s, e) => ReplaceChart(grid, variable, values, "BarValues"); // Vertical
                barMenu.Items.Add(itemBarCol);

                var itemBarRow = new MenuItem { Header = "Barres (Horizontales)" };
                itemBarRow.Click += (s, e) => ReplaceChart(grid, variable, values, "RowValues"); // Horizontal
                barMenu.Items.Add(itemBarRow);
                typeMenuItem.Items.Add(barMenu);

                // -- Nuage de points group --
                var scatterMenu = new MenuItem { Header = "Nuage de Points" };
                var itemScatter = new MenuItem { Header = "Nuage de points (Valeurs brutes)" };
                itemScatter.Click += (s, e) => ReplaceChart(grid, variable, values, "Scatter");
                scatterMenu.Items.Add(itemScatter);
                typeMenuItem.Items.Add(scatterMenu);
            }
            else
            {
                // -- Secteurs group --
                var pieMenu = new MenuItem { Header = "Secteurs" };
                var itemPie = new MenuItem { Header = "Secteurs (Camembert)" };
                itemPie.Click += (s, e) => ReplaceChart(grid, variable, values, "Pie");
                pieMenu.Items.Add(itemPie);

                var itemDonut = new MenuItem { Header = "Anneau" };
                itemDonut.Click += (s, e) => ReplaceChart(grid, variable, values, "Donut");
                pieMenu.Items.Add(itemDonut);
                typeMenuItem.Items.Add(pieMenu);

                // -- Barres group --
                var barMenu = new MenuItem { Header = "Barres et Colonnes" };
                var itemCol = new MenuItem { Header = "Colonnes (Verticales)" };
                itemCol.Click += (s, e) => ReplaceChart(grid, variable, values, "BarQuali");
                barMenu.Items.Add(itemCol);

                var itemRow = new MenuItem { Header = "Barres (Horizontales)" };
                itemRow.Click += (s, e) => ReplaceChart(grid, variable, values, "RowQuali");
                barMenu.Items.Add(itemRow);
                typeMenuItem.Items.Add(barMenu);
            }
            
            contextMenu.Items.Add(typeMenuItem);
            grid.ContextMenu = contextMenu;
            
            // Wire up the button
            settingsBtn.Click += (s, e) => 
            {
                contextMenu.PlacementTarget = settingsBtn;
                contextMenu.IsOpen = true;
            };

            chartContainer.Child = grid;
            AnalysisDoc.Blocks.Add(chartContainer);
        }

        private async void ReplaceChart(Grid grid, StudyVariable variable, List<object> values, string type)
        {
            // Remove existing chart
            var existing = grid.Children.OfType<LiveCharts.Wpf.Charts.Base.Chart>().FirstOrDefault();
            if (existing != null) grid.Children.Remove(existing);

            LiveCharts.Wpf.Charts.Base.Chart? newChart = null;

            try 
            {
                if (type == "Pie" || type == "Donut")
                {
                     var groups = values.GroupBy(x => x?.ToString() ?? "N/A").Select(g => new { Label = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).Take(10);
                     var chart = new LiveCharts.Wpf.PieChart { LegendLocation = LiveCharts.LegendLocation.Right };
                     if (type == "Donut") chart.InnerRadius = 50; 
                     
                     chart.Series = new LiveCharts.SeriesCollection();
                     foreach(var g in groups) chart.Series.Add(new LiveCharts.Wpf.PieSeries { Title = g.Label, Values = new LiveCharts.ChartValues<int> { g.Count }, DataLabels = true });
                     newChart = chart;
                }
                else if (type == "BarQuali" || type == "RowQuali")
                {
                     var groups = values.GroupBy(x => x?.ToString() ?? "N/A").Select(g => new { Label = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).Take(15);
                     var chart = new LiveCharts.Wpf.CartesianChart();
                     
                     if (type == "RowQuali")
                     {
                         chart.Series = new LiveCharts.SeriesCollection { new LiveCharts.Wpf.RowSeries { Title = "Effectif", Values = new LiveCharts.ChartValues<int>(groups.Select(g => g.Count)), DataLabels = true } };
                         chart.AxisY.Add(new LiveCharts.Wpf.Axis { Labels = groups.Select(g => g.Label).ToList() }); 
                         chart.AxisX.Add(new LiveCharts.Wpf.Axis { Title = "Effectif", MinValue = 0 });
                     }
                     else
                     {
                         chart.Series = new LiveCharts.SeriesCollection { new LiveCharts.Wpf.ColumnSeries { Title = "Effectif", Values = new LiveCharts.ChartValues<int>(groups.Select(g => g.Count)), DataLabels = true } };
                         chart.AxisX.Add(new LiveCharts.Wpf.Axis { Labels = groups.Select(g => g.Label).ToList(), Separator = new LiveCharts.Wpf.Separator { Step = 1 } });
                         chart.AxisY.Add(new LiveCharts.Wpf.Axis { Title = "Effectif", MinValue = 0 });
                     }
                     newChart = chart;
                }
                else if (type == "Scatter")
                {
                     // Index vs Value
                     var doubles = values.Select(x => x?.ToString()).Where(s => !string.IsNullOrWhiteSpace(s) && double.TryParse(s, out _)).Select(s => double.Parse(s!)).ToList();
                     var chart = new LiveCharts.Wpf.CartesianChart();
                     var scatterValues = new LiveCharts.ChartValues<LiveCharts.Defaults.ObservablePoint>();
                     for(int i=0; i<doubles.Count; i++) scatterValues.Add(new LiveCharts.Defaults.ObservablePoint(i, doubles[i]));

                     chart.Series.Add(new LiveCharts.Wpf.ScatterSeries { Title="Valeurs", Values = scatterValues, MinPointShapeDiameter = 5, MaxPointShapeDiameter = 10 });
                     chart.AxisX.Add(new LiveCharts.Wpf.Axis { Title = "Index" });
                     chart.AxisY.Add(new LiveCharts.Wpf.Axis { Title = variable.Name });
                     newChart = chart;
                }
                else if (type == "Histogram" || type == "Line" || type == "Area")
                {
                     var doubles = values.Select(x => x?.ToString()).Where(s => !string.IsNullOrWhiteSpace(s) && double.TryParse(s, out _)).Select(s => double.Parse(s!)).OrderBy(x => x).ToList();
                     int binCount = 10;
                     if (doubles.Count < 10) binCount = doubles.Count > 0 ? doubles.Count : 1; 
                     
                     double min = doubles.First(); double max = doubles.Last();
                     double width = (max - min) / binCount;
                     if (width == 0) width = 1;

                     var labels = new string[binCount]; var counts = new int[binCount];
                     for (int i = 0; i < binCount; i++) {
                         double l = min + (i * width); double h = min + ((i + 1) * width);
                         labels[i] = $"{l:0.#}-{h:0.#}";
                         counts[i] = doubles.Count(v => v >= l && (i == binCount - 1 ? v <= h : v < h));
                     }

                     var chart = new LiveCharts.Wpf.CartesianChart();
                     if (type == "Line" || type == "Area")
                     {
                        var lineSeries = new LiveCharts.Wpf.LineSeries { Title="Distribution", Values = new LiveCharts.ChartValues<int>(counts), PointGeometrySize=10, DataLabels=true };
                        if (type == "Area") lineSeries.Fill = Brushes.SkyBlue; // Area style
                        else lineSeries.Fill = Brushes.Transparent; // Line style
                        
                        chart.Series.Add(lineSeries);
                     }
                     else
                        chart.Series.Add(new LiveCharts.Wpf.ColumnSeries { Title="Distribution", Values = new LiveCharts.ChartValues<int>(counts), DataLabels=true });
                     
                     chart.AxisX.Add(new LiveCharts.Wpf.Axis { Title = variable.Name, Labels = labels });
                     chart.AxisY.Add(new LiveCharts.Wpf.Axis { Title = "Effectif", MinValue = 0 });
                     newChart = chart;
                }
                else if (type == "BarValues" || type == "RowValues") // Exact values for quanti
                {
                     var doubles = values.Select(x => x?.ToString()).Where(s => !string.IsNullOrWhiteSpace(s) && double.TryParse(s, out _)).Select(s => double.Parse(s!)).OrderBy(x => x).ToList();
                     var groups = doubles.GroupBy(x => x).Select(g => new { Val = g.Key, Count = g.Count() }).OrderBy(g => g.Val);
                     
                     if (groups.Count() > 50 && !await DialogHelper.ShowConfirmation("Il y a beaucoup de valeurs uniques (>50). Le graphique risque d'être illisible. Continuer ?", "Attention")) return;

                     var chart = new LiveCharts.Wpf.CartesianChart();
                     
                     if (type == "RowValues") // Horizontal
                     {
                         chart.Series = new LiveCharts.SeriesCollection { new LiveCharts.Wpf.RowSeries { Title = "Fréquence", Values = new LiveCharts.ChartValues<int>(groups.Select(g => g.Count)), DataLabels = true } };
                         chart.AxisY.Add(new LiveCharts.Wpf.Axis { Title = "Valeur", Labels = groups.Select(g => g.Val.ToString()).ToList() });
                         chart.AxisX.Add(new LiveCharts.Wpf.Axis { Title = "Effectif", MinValue = 0 });
                     }
                     else // Vertical
                     {
                         chart.Series = new LiveCharts.SeriesCollection { new LiveCharts.Wpf.ColumnSeries { Title = "Fréquence", Values = new LiveCharts.ChartValues<int>(groups.Select(g => g.Count)), DataLabels = true } };
                         chart.AxisX.Add(new LiveCharts.Wpf.Axis { Title = "Valeur", Labels = groups.Select(g => g.Val.ToString()).ToList() });
                         chart.AxisY.Add(new LiveCharts.Wpf.Axis { Title = "Effectif", MinValue = 0 });
                     }
                     
                     newChart = chart;
                }
            }
            catch { }

            if (newChart != null)
            {
                newChart.Margin = new Thickness(10);
                Grid.SetRow(newChart, 1);
                grid.Children.Add(newChart);
            }
        }

        private void GenerateDescriptiveTable(StudyVariable variable, List<double> doubles)
        {
            if (doubles.Count == 0) return;

            // 1. Table Title (TOP)
            _tableCount++;
            string roman = _statisticsService.ToRoman(_tableCount);
            var titleBox = new TextBox { Text = $"Tableau {roman} : {(!string.IsNullOrEmpty(variable.Description) ? variable.Description : variable.Name)}", FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(0, 10, 0, 5), BorderThickness = new Thickness(0), Background = Brushes.Transparent };
            AnalysisDoc.Blocks.Add(new BlockUIContainer(titleBox));

            double mean = doubles.Average();
            double median = doubles[doubles.Count / 2];
            if (doubles.Count % 2 == 0) median = (doubles[doubles.Count / 2 - 1] + doubles[doubles.Count / 2]) / 2.0;
            
            double min = doubles.First();
            double max = doubles.Last();
            double stdDev = Math.Sqrt(doubles.Sum(d => Math.Pow(d - mean, 2)) / doubles.Count);

            Table t = new Table() { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 15) };
            t.Columns.Add(new TableColumn() { Width = new GridLength(250) }); 
            t.Columns.Add(new TableColumn() { Width = new GridLength(150) }); 

            TableRowGroup rg = new TableRowGroup();
            rg.Rows.Add(CreateRow("Statistique", "Valeur", true));
            rg.Rows.Add(CreateRow("Moyenne", mean.ToString("F2")));
            rg.Rows.Add(CreateRow("Médiane", median.ToString("F2")));
            rg.Rows.Add(CreateRow("Écart-Type", stdDev.ToString("F2")));
            rg.Rows.Add(CreateRow("Minimum", min.ToString("F2")));
            rg.Rows.Add(CreateRow("Maximum", max.ToString("F2")));
            rg.Rows.Add(CreateRow("N Valide", doubles.Count.ToString()));
            
            t.RowGroups.Add(rg);
            AnalysisDoc.Blocks.Add(t);

            // AUTO-INTERPRETATION
            var statsDict = new Dictionary<string, double> { { "Mean", mean }, { "Median", median }, { "StdDev", stdDev }, { "Min", min }, { "Max", max } };
            string interpretation = _interpretationService.InterpretDescriptive(variable, statsDict);
            AddMovableInterpretation(interpretation);
        }

        private void GenerateFrequencyTable(StudyVariable variable, List<object> values)
        {
            var groups = values.GroupBy(x => x?.ToString() ?? "N/A").Select(g => new { Label = g.Key, Count = g.Count(), Pct = (double)g.Count() / values.Count * 100 }).OrderByDescending(g => g.Count).ToList();

            // 1. Table Title (TOP)
            _tableCount++;
            string roman = _statisticsService.ToRoman(_tableCount);
            var titleBox = new TextBox { Text = $"Tableau {roman} : {(!string.IsNullOrEmpty(variable.Description) ? variable.Description : variable.Name)}", FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(0, 10, 0, 5), BorderThickness = new Thickness(0), Background = Brushes.Transparent };
            AnalysisDoc.Blocks.Add(new BlockUIContainer(titleBox));

            int totalN = groups.Sum(g => g.Count);
            double totalPct = groups.Sum(g => g.Pct);

            Table t = new Table() { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 15) };
            t.Columns.Add(new TableColumn() { Width = new GridLength(300) }); 
            t.Columns.Add(new TableColumn() { Width = new GridLength(100) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(100) });

            TableRowGroup rg = new TableRowGroup();
            rg.Rows.Add(CreateRow("Catégorie", "Effectif (N)", true, "Pourcentage (%)"));
            foreach (var g in groups) rg.Rows.Add(CreateRow(g.Label ?? "N/A", g.Count.ToString(), false, g.Pct.ToString("F1") + "%"));
            rg.Rows.Add(CreateRow("Total", totalN.ToString(), true, totalPct.ToString("F0") + "%"));
            
            t.RowGroups.Add(rg);
            AnalysisDoc.Blocks.Add(t);

            // AUTO-INTERPRETATION
            var freqList = groups.Select(g => new KeyValuePair<string, double>(g.Label, g.Pct)).ToList();
            string interpretation = _interpretationService.InterpretFrequencies(variable, freqList);
            AddMovableInterpretation(interpretation);
        }

        private void AddMovableInterpretation(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            var grid = new Grid { Margin = new Thickness(0, 5, 0, 20) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textBox = new TextBox
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                BorderThickness = new Thickness(1, 0, 0, 0),
                BorderBrush = Brushes.LightGray,
                Padding = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                FontStyle = FontStyles.Italic,
                FontSize = 13
            };
            grid.Children.Add(textBox);

            var moveBtn = new Button
            {
                Content = "↕",
                ToolTip = "Déplacer le bloc (Monter/Descendre)",
                Width = 25, Height = 25,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, 0, 0, 0),
                Background = Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            Grid.SetColumn(moveBtn, 1);
            grid.Children.Add(moveBtn);

            var container = new BlockUIContainer(grid);
            
            moveBtn.Click += (s, e) =>
            {
                var blocks = AnalysisDoc.Blocks.ToList();
                int index = blocks.IndexOf(container);
                if (index != -1)
                {
                    if (index < blocks.Count - 1) // Simple swap with element below
                    {
                        var target = blocks[index + 1];
                        AnalysisDoc.Blocks.Remove(container);
                        AnalysisDoc.Blocks.InsertAfter(target, container);
                    }
                    else if (index > 0) // Swap with element above if at end
                    {
                        var target = blocks[index - 1];
                        AnalysisDoc.Blocks.Remove(container);
                        AnalysisDoc.Blocks.InsertBefore(target, container);
                    }
                }
            };

            AnalysisDoc.Blocks.Add(container);
        }
        
        private Paragraph CreateAnalysisHeader(string text)
        {
            return new Paragraph(new Run(text.ToUpper())) 
            { 
                FontSize = 18, 
                FontWeight = FontWeights.Bold, 
                Foreground = (Brush)Application.Current.Resources["PrimaryBrush"],
                Margin = new Thickness(0, 20, 0, 10),
                BorderBrush = (Brush)Application.Current.Resources["PrimaryBrush"],
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 0, 0, 5)
            };
        }

        private TableRow CreateRow(string col1, string col2, bool isHeader = false, string? col3 = null)
        {
            var tr = new TableRow();
            tr.Background = isHeader ? Brushes.LightGray : Brushes.White;
            
            // Standard static cells for robustness
            tr.Cells.Add(new TableCell(new Paragraph(new Run(col1))) { Padding = new Thickness(5), FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal });
            tr.Cells.Add(new TableCell(new Paragraph(new Run(col2))) { Padding = new Thickness(5), FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal });
            
            if (col3 != null) 
                tr.Cells.Add(new TableCell(new Paragraph(new Run(col3))) { Padding = new Thickness(5), FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal });

            return tr;
        }





        private async void SaveToProject_Click(object sender, RoutedEventArgs e)
        {
            if (_project == null) return;
            
            // Serialize Document to String
            string xamlString = System.Windows.Markup.XamlWriter.Save(AnalysisDoc);
            _project.ReportContent = xamlString;

            await DialogHelper.ShowMessage("Les résultats (Rapport et Plan) ont été sauvegardés dans le projet.", "Sauvegarde Réussie");
        }

        private async void ExportReport_Click(object sender, RoutedEventArgs e)
        {
             await DialogHelper.ShowMessage("Fonctionnalité d'exportation vers PDF/Word en cours d'intégration.", "Export");
        }
        private async void RecodeVariable_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedVar = menuItem?.CommandParameter as StudyVariable;

            if (selectedVar != null)
            {
                var dialog = new Windows.RecodeDialog(selectedVar.RecodingInstructions);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var instructions = dialog.ResultInstructions;
                        selectedVar.RecodingInstructions = instructions; // Update stored rules

                        // Apply Transformation
                        var service = new RecodeService();
                        
                        // Extract raw column
                        var rawValues = _projectData.Select(row => row.ContainsKey(selectedVar.Name) ? row[selectedVar.Name] : null).ToList();
                        
                        // Transform
                        var nonNullRaw = rawValues.Where(v => v != null).Select(v => v!).ToList();
                        var newValues = service.Recode(nonNullRaw, instructions, selectedVar.Type);
                        
                        // Create New Variable
                        var newName = selectedVar.Name + "_Recoded";
                        int counter = 1;
                        while (_importedVariables.Any(v => v.Name == newName))
                        {
                            newName = selectedVar.Name + "_Recoded_" + counter;
                            counter++;
                        }

                        // Update Data
                        for (int i = 0; i < _projectData.Count; i++)
                        {
                            if (i < newValues.Count)
                                _projectData[i][newName] = newValues[i];
                        }

                        // Add Variable to UI
                        var newVar = new StudyVariable
                        {
                             Name = newName,
                             Prompt = $"Recodage de {selectedVar.Name}",
                             Type = VariableType.Text // Recoded is usually text/categorical
                        };
                        _importedVariables.Add(newVar);
                        
                        // Sync with Project if exists
                        if (_project != null) _project.Variables.Add(newVar);
                        UpdateAnalysisVariables();

                        await DialogHelper.ShowMessage($"Nouvelle variable '{newName}' créée avec succès !", "Recodage Terminé");
                    }
                    catch (Exception ex)
                    {
                        await DialogHelper.ShowError($"Erreur lors du recodage : {ex.Message}", "Erreur");
                    }
                }
            }
        }
    }
}
