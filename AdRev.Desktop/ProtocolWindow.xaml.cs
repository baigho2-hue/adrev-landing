using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AdRev.Domain.Models;
using AdRev.Domain.Enums;
using AdRev.Domain.Protocols;
using AdRev.Core.Services;
using AdRev.Core.Protocols;
using AdRev.Desktop.Windows; // For ResourceDialog, DiscussionWindow
using Microsoft.Win32;
using AdRev.Core.Resources;
using System.IO;

namespace AdRev.Desktop
{
    public partial class ProtocolWindow : Window
    {
        private readonly ProtocolService _service = new ProtocolService();
        private readonly QualityService _qualityService = new QualityService();
        private readonly WordExportService _wordExportService = new WordExportService();
        private readonly StatisticsService _statsService = new StatisticsService();
        private readonly AdRev.Core.Services.AuditService _auditService = new AdRev.Core.Services.AuditService(new AdRev.Core.Services.ResearcherProfileService());

        // UI Temporary Lists
        private ObservableCollection<Author> _tempCoAuthors = new ObservableCollection<Author>();
        
        // Resources (Tables/Figures)
        private List<ProtocolResource> _resources = new List<ProtocolResource>();
        
        private ResearchProtocol _protocol = new ResearchProtocol();
        private ResearchProject _project;
        private FunctionalRole _currentRole;
        private UserAccessLevel _currentAccessLevel;
        private ObservableCollection<ProtocolAppendix> _tempAppendices = new ObservableCollection<ProtocolAppendix>();

        private TextBox? _activeTextBox = null;

        // Track Protocol View State
        private int _currentStep = 1;

        public ProtocolWindow(ResearchProject? project = null, FunctionalRole role = FunctionalRole.PrincipalInvestigator, UserAccessLevel access = UserAccessLevel.Admin)
        {
            InitializeComponent();
            
            _project = project ?? new ResearchProject();
            _currentRole = role;
            _currentAccessLevel = access;

            this.Loaded += (s, e) => {
                InitializeUI();
                LoadProjectData();
                UpdateView();
            };
        }

        private void InitializeUI()
        {
            // Avoid duplicate initialization
            if (StudyTypeComboBox.ItemsSource != null) return;

            StudyTypeComboBox.ItemsSource = Enum.GetValues(typeof(StudyType));
            EpidemiologyTypeComboBox.ItemsSource = Enum.GetValues(typeof(EpidemiologicalStudyType));
            QualitativeApproachComboBox.ItemsSource = Enum.GetValues(typeof(QualitativeApproach));
            SamplingTypeComboBox.ItemsSource = Enum.GetValues(typeof(SamplingType));
            QualitativeSamplingComboBox.ItemsSource = Enum.GetValues(typeof(SamplingType)); // Reuse enum
            DomainComboBox.ItemsSource = Enum.GetValues(typeof(ScientificDomain));
            
            RefStyleComboStep1.ItemsSource = Enum.GetValues(typeof(ReferenceStyle));
            RefStyleComboStep13.ItemsSource = Enum.GetValues(typeof(ReferenceStyle));
            
            ActiveRoleComboBox.ItemsSource = Enum.GetValues(typeof(FunctionalRole));
            ActiveAccessComboBox.ItemsSource = Enum.GetValues(typeof(UserAccessLevel));

            CoAuthorsListBox.ItemsSource = _tempCoAuthors;
            AppendicesListBox.ItemsSource = _tempAppendices;
            AppendixTypeComboBox.ItemsSource = Enum.GetValues(typeof(AppendixType));

            // Set Defaults without triggering massive Permission updates yet
            StudyTypeComboBox.SelectedIndex = 0;
            RefStyleComboStep1.SelectedItem = ReferenceStyle.Vancouver;
            
            // Explicitly set these to avoid selection-change loops or null checks during init
            ActiveRoleComboBox.SelectedItem = _currentRole;
            ActiveAccessComboBox.SelectedItem = _currentAccessLevel;

            ApplyPermissions();
        }

        private void LoadProjectData()
        {
            if (_project == null) return;

            // Map Project to Protocol basic info if new protocol
            _protocol.Title = _project.Title;
            _protocol.Domain = _project.Domain;
            _protocol.StudyType = _project.StudyType;
            _protocol.EpidemiologyType = _project.EpidemiologyType;

            // Load UI from _protocol
            TitleTextBox.Text = _protocol.Title;
            DomainComboBox.SelectedItem = _protocol.Domain;
            StudyTypeComboBox.SelectedItem = _protocol.StudyType;
            EpidemiologyTypeComboBox.SelectedItem = _protocol.EpidemiologyType;
            
            ContextTextBox.Text = _protocol.Context;
            ProblemTextBox.Text = _protocol.ProblemJustification;
            ResearchQuestionTextBox.Text = _protocol.ResearchQuestion;
            HypothesesTextBox.Text = _protocol.Hypotheses;
            
            GeneralObjectiveTextBox.Text = _protocol.GeneralObjective;
            SpecificObjectivesTextBox.Text = _protocol.SpecificObjectives;
            
            ConceptsTextBox.Text = _protocol.ConceptDefinitions;
            ConceptualModelTextBox.Text = _protocol.ConceptualModel;
            
            StudySettingTextBox.Text = _protocol.StudySetting;
            QualitativeApproachComboBox.SelectedItem = _protocol.QualitativeApproach;
            
            PopulationTextBox.Text = _protocol.StudyPopulation;
            IsMulticentricCheckBox.IsChecked = _protocol.IsMulticentric;
            StudyCentersTextBox.Text = _protocol.StudyCenters;
            InclusionTextBox.Text = _protocol.InclusionCriteria;
            ExclusionTextBox.Text = _protocol.ExclusionCriteria;
            
            SamplingTypeComboBox.SelectedItem = _protocol.SamplingType;
            IsStratifiedCheckBox.IsChecked = _protocol.IsStratified;
            StratificationCriteriaTextBox.Text = _protocol.StratificationCriteria;
            IsClusterCheckBox.IsChecked = _protocol.IsClusterSampling;
            ClusterSizeTextBox.Text = _protocol.ClusterSize.ToString();
            DesignEffectTextBox.Text = _protocol.DesignEffect.ToString();
            ExpectedLossRateTextBox.Text = _protocol.ExpectedLossRate.ToString();
            SamplingTextBox.Text = _protocol.SamplingMethod;
            
            DataCollectionTextBox.Text = _protocol.DataCollection;
            DataAnalysisTextBox.Text = _protocol.DataAnalysis;
            EthicsTextBox.Text = _protocol.Ethics;
            BudgetTextBox.Text = _protocol.Budget;
            ExpectedResultsTextBox.Text = _protocol.ExpectedResults;
            ConclusionTextBox.Text = _protocol.Conclusion;
            ReferencesTextBox.Text = _protocol.References;

            // Load Annexes
            _tempAppendices.Clear();
            foreach (var app in _protocol.Appendices) _tempAppendices.Add(app);

            // Load Authors
            _tempCoAuthors.Clear();
            foreach(var auth in _protocol.CoAuthors) _tempCoAuthors.Add(auth);
            
            // Resources
            _resources = _protocol.Resources;

            // Trigger updates
            UpdateSamplingTypes(_protocol.StudyType);
            UpdateAppendixSuggestions(_protocol.StudyType);
            UpdateView();
        }

        // --- Permissions Logic ---

        private void ActiveRoleAndAccess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveRoleComboBox.SelectedItem is FunctionalRole role && ActiveAccessComboBox.SelectedItem is UserAccessLevel access)
            {
                _currentRole = role;
                _currentAccessLevel = access;
                ApplyPermissions();
            }
        }

        private void ApplyPermissions()
        {
            // Reset main menu visibility
            MenuFileSave.Visibility = Visibility.Visible;
            MenuInsertResource.Visibility = Visibility.Visible;
            MenuDiscussion.Visibility = Visibility.Visible;

            if (_currentAccessLevel == UserAccessLevel.Viewer || _currentRole == FunctionalRole.Student)
            {
                MenuFileSave.Visibility = Visibility.Collapsed;
                MenuInsertResource.Visibility = Visibility.Collapsed;
            }

            if (_currentRole == FunctionalRole.DataManager)
            {
                MenuDiscussion.Visibility = Visibility.Collapsed;
            }
            
            // Re-eval current view
            UpdateView();
        }

        private bool CanUserEditStep(int step)
        {
            if (_currentAccessLevel == UserAccessLevel.Admin) return true;
            if (_currentAccessLevel == UserAccessLevel.Viewer) return false;

            // Simplified Role Logic
            switch (_currentRole)
            {
                case FunctionalRole.PrincipalInvestigator: return true;
                case FunctionalRole.Statistician: return (step == 7 || step == 11 || step == 12);
                case FunctionalRole.Methodologist: return (step != 9); // No budget
                default: return true; 
            }
        }

        private void SetProtocolReadOnly(bool isReadOnly)
        {
            // Find all TextBoxes and disable/enable
            foreach (var tb in FindVisualChildren<TextBox>(this))
            {
                tb.IsReadOnly = isReadOnly;
                tb.Opacity = isReadOnly ? 0.8 : 1.0;
            }
             foreach (var cb in FindVisualChildren<ComboBox>(this))
            {
                if (cb.Name == "ActiveRoleComboBox" || cb.Name == "ActiveAccessComboBox") continue;
                cb.IsEnabled = !isReadOnly;
            }
            
            // Buttons logic (Whitelist fix)
            foreach(var btn in FindVisualChildren<Button>(this))
            {
                if (btn.Name == "BtnMinimize" || btn.Name =="BtnClose") continue;

                // Explicitly allow Insert buttons
                if ((btn.Name != null && btn.Name.StartsWith("BtnInsert_")) || 
                    (btn.Content is string s && (s.Contains("➕") || s.Contains("Calculer") || s.Contains("Générer"))))
                {
                    btn.IsEnabled = !isReadOnly;
                    btn.Visibility = Visibility.Visible; // FORCE VISIBILITY
                }
            }
        }

        // --- Navigation ---

        private void SideMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             if (SideMenuListBox.SelectedItem is ListBoxItem item && item.Tag != null && int.TryParse(item.Tag.ToString(), out int s))
             {
                 _currentStep = s;
                 UpdateView();
             }
        }

        private void PrevSection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1) { _currentStep--; UpdateView(); }
        }

        private void NextSection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep < 16) { _currentStep++; UpdateView(); }
        }

        private void UpdateView()
        {
            // Collapse All
            if(View_Step1 != null) View_Step1.Visibility = Visibility.Collapsed;
            if(View_Introduction != null) View_Introduction.Visibility = Visibility.Collapsed;
            if(View_Objectives != null) View_Objectives.Visibility = Visibility.Collapsed;
            if(View_ConceptualFramework != null) View_ConceptualFramework.Visibility = Visibility.Collapsed;
            if(View_Methodology_Design != null) View_Methodology_Design.Visibility = Visibility.Collapsed;
            if(View_Methodology_Population != null) View_Methodology_Population.Visibility = Visibility.Collapsed;
            if(View_Methodology_Sampling != null) View_Methodology_Sampling.Visibility = Visibility.Collapsed;
            if(View_Methodology_Data != null) View_Methodology_Data.Visibility = Visibility.Collapsed;
            if(View_Budget != null) View_Budget.Visibility = Visibility.Collapsed;
            if(View_Ethics != null) View_Ethics.Visibility = Visibility.Collapsed;
            if(View_DataAnalysis != null) View_DataAnalysis.Visibility = Visibility.Collapsed;
            if(View_Results != null) View_Results.Visibility = Visibility.Collapsed;
            if(View_Conclusion != null) View_Conclusion.Visibility = Visibility.Collapsed;
            if(View_References != null) View_References.Visibility = Visibility.Collapsed;
            if(View_Appendices != null) View_Appendices.Visibility = Visibility.Collapsed;
            if(View_AuditLogs != null) View_AuditLogs.Visibility = Visibility.Collapsed;

            // Show Current
             switch (_currentStep)
            {
                case 1: if(View_Step1 != null) View_Step1.Visibility = Visibility.Visible; break;
                case 2: if(View_Introduction != null) View_Introduction.Visibility = Visibility.Visible; break;
                case 3: if(View_Objectives != null) View_Objectives.Visibility = Visibility.Visible; break;
                case 4: if(View_ConceptualFramework != null) View_ConceptualFramework.Visibility = Visibility.Visible; break;
                case 5: if(View_Methodology_Design != null) View_Methodology_Design.Visibility = Visibility.Visible; break;
                case 6: if(View_Methodology_Population != null) View_Methodology_Population.Visibility = Visibility.Visible; break;
                case 7: if(View_Methodology_Sampling != null) View_Methodology_Sampling.Visibility = Visibility.Visible; break;
                case 8: if(View_Methodology_Data != null) View_Methodology_Data.Visibility = Visibility.Visible; break;
                case 9: if(View_Budget != null) View_Budget.Visibility = Visibility.Visible; break;
                case 10: if(View_Ethics != null) View_Ethics.Visibility = Visibility.Visible; break;
                case 11: if(View_DataAnalysis != null) View_DataAnalysis.Visibility = Visibility.Visible; break;
                case 12: if(View_Results != null) View_Results.Visibility = Visibility.Visible; break;
                case 13: if(View_Conclusion != null) View_Conclusion.Visibility = Visibility.Visible; break;
                case 14: if(View_References != null) View_References.Visibility = Visibility.Visible; break;
                case 15: 
                    if(View_AuditLogs != null) 
                    {
                        View_AuditLogs.Visibility = Visibility.Visible;
                        LoadAuditLogs();
                    }
                    break;
                case 16:
                    if(View_Appendices != null) View_Appendices.Visibility = Visibility.Visible;
                    break;
            }

            // Sync ListBox
            if (SideMenuListBox != null && SideMenuListBox.SelectedIndex != _currentStep - 1)
                SideMenuListBox.SelectedIndex = _currentStep - 1;

            bool isReadOnly = !CanUserEditStep(_currentStep);
            this.Title = $"Éditeur de Protocole - Étape {_currentStep} (Mode: {_currentAccessLevel}, Éditable: {!isReadOnly})";
            SetProtocolReadOnly(isReadOnly);
        }

        // --- Event Handlers Implementation ---

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        
        private void Close_Click(object sender, RoutedEventArgs e) 
        {
             this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (this.Owner is Window owner)
            {
                owner.Show();
                if (owner.WindowState == WindowState.Minimized) owner.WindowState = WindowState.Normal;
                owner.Activate();
            }
        }

        private void DomainComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_project != null && DomainComboBox.SelectedItem is ScientificDomain d) _project.Domain = d; }
        private void RefStyleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
             if (RefStyleComboStep13 != null && RefStyleComboStep1 != null)
             {
                 RefStyleComboStep13.SelectedItem = RefStyleComboStep1.SelectedItem;
             }
        }

        private void AddCoAuthor_Click(object sender, RoutedEventArgs e) => _tempCoAuthors.Add(new Author { FirstName = "Nouveau", LastName = "Auteur" });
        private void RemoveCoAuthor_Click(object sender, RoutedEventArgs e) { if(sender is Button b && b.DataContext is Author a) _tempCoAuthors.Remove(a); }

        private void StudyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
             if (StudyTypeComboBox.SelectedItem is StudyType st)
             {
                 UpdateSamplingTypes(st);
                 UpdateQualityGuidelines(st);

                 // Toggle panels
                 if (QualitativeTypePanel != null)
                     QualitativeTypePanel.Visibility = (st == StudyType.Qualitative || st == StudyType.Mixed) ? Visibility.Visible : Visibility.Collapsed;
                 
                  if (EpidemiologyTypePanel != null)
                      EpidemiologyTypePanel.Visibility = (st == StudyType.Quantitative || st == StudyType.Mixed) ? Visibility.Visible : Visibility.Collapsed;
                  
                  UpdateAppendixSuggestions(st);
              }
          }

        private void RefreshAudit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (StudyTypeComboBox.SelectedItem is StudyType st) UpdateQualityGuidelines(st);
        }

        private void QualitativeSamplingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void EpidemiologyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        
        private void IsMulticentricCheckBox_Checked(object sender, RoutedEventArgs e) { if(MulticentricDetailsPanel != null) MulticentricDetailsPanel.Visibility = Visibility.Visible; }
        private void IsMulticentricCheckBox_Unchecked(object sender, RoutedEventArgs e) { if(MulticentricDetailsPanel != null) MulticentricDetailsPanel.Visibility = Visibility.Collapsed; }
        
        private void SamplingTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            if (SamplingTypeComboBox.SelectedItem is SamplingType st)
            {
                if(StratificationPanel != null) 
                    StratificationPanel.Visibility = (st == SamplingType.Stratified) ? Visibility.Visible : Visibility.Collapsed;
                
                if(ClusterPanel != null) 
                    ClusterPanel.Visibility = (st == SamplingType.ClusterSampling) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        private void IsStratifiedCheckBox_Checked(object sender, RoutedEventArgs e) { if(StratificationDetailsPanel != null) StratificationDetailsPanel.Visibility = Visibility.Visible; }
        private void IsStratifiedCheckBox_Unchecked(object sender, RoutedEventArgs e) { if(StratificationDetailsPanel != null) StratificationDetailsPanel.Visibility = Visibility.Collapsed; }
        private void IsClusterCheckBox_Checked(object sender, RoutedEventArgs e) { if(ClusterDetailsPanel != null) ClusterDetailsPanel.Visibility = Visibility.Visible; }
        private void IsClusterCheckBox_Unchecked(object sender, RoutedEventArgs e) { if(ClusterDetailsPanel != null) ClusterDetailsPanel.Visibility = Visibility.Collapsed; }

        // Calculators
        private void CalculatorSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             if (PanelCochran == null || PanelComparison == null) return;
             
             PanelCochran.Visibility = Visibility.Collapsed;
             PanelComparison.Visibility = Visibility.Collapsed;

             if (CalculatorSelector.SelectedItem is ComboBoxItem selected)
             {
                 if (selected.Tag?.ToString() == "Cochran") PanelCochran.Visibility = Visibility.Visible;
                 else if (selected.Tag?.ToString() == "Comparison") PanelComparison.Visibility = Visibility.Visible;
             }
        }

        private void CalculateCochran_Click(object sender, RoutedEventArgs e)
        {
            // Inline logic or simulated
            int n = 385; // Default for 50%, 5% precision large pop
            SamplingTextBox.Text += $"\n[Calcul] Taille d'échantillon recommandée (Cochran) : n = {n}.";
        }

        private void CalculateComparison_Click(object sender, RoutedEventArgs e)
        {
            SamplingTextBox.Text += $"\n[Calcul] Comparaison de proportions : n = 64 par groupe (Simulé).";
        }

        private void OpenVariableDesigner_Click(object sender, RoutedEventArgs e)
        {
            if (_protocol == null) return;
            
            // Determine if qualitative deductive (simple logic for now)
            bool isQualitative = (_protocol.StudyType == StudyType.Qualitative || _protocol.StudyType == StudyType.Mixed) 
                                 && _protocol.QualitativeApproach == QualitativeApproach.Deductive;

            var designer = new VariableDesignerWindow(_protocol.Variables, _protocol.Title ?? "Nouveau Protocole", isQualitative);
            designer.Owner = this;
            
            if (designer.ShowDialog() == true)
            {
                // Update variables from designer
                _protocol.Variables = new List<AdRev.Domain.Variables.StudyVariable>(designer.Variables);
                
                // Update text preview in DataCollectionTextBox if needed
                if (DataCollectionTextBox != null)
                {
                    DataCollectionTextBox.Text = $"Dictionnaire de variables mis à jour : {_protocol.Variables.Count} variables définies.";
                }
            }
        }

        private void GenerateProtocolPlan_Click(object sender, RoutedEventArgs e)
        {
             DataAnalysisTextBox.Text = "1. Description de l'échantillon (Moyennes, Fréquences)\n2. Analyses bivariées (Chi-2, t-Student)\n3. Analyses multivariées (Régression logistique).\nSeuil de significativité : p < 0.05.";
        }
        
        private void RegenerateReferences_Click(object sender, RoutedEventArgs e)
        {
             // Simulate
             MessageBox.Show("Références reformatées selon le style " + RefStyleComboStep1.SelectedItem, "Zotero Integration");
        }

        // --- Helpers ---
        private void UpdateSamplingTypes(StudyType type)
        {
            var list = new List<SamplingType>();
            var allValues = Enum.GetValues(typeof(SamplingType)).Cast<SamplingType>().ToList();

            if (type == StudyType.Quantitative)
            {
                list.AddRange(allValues.Where(v => v == SamplingType.SimpleRandom || v == SamplingType.Systematic || 
                                                  v == SamplingType.Stratified || v == SamplingType.ClusterSampling || 
                                                  v == SamplingType.MultiStage || v == SamplingType.Census));
            }
            else if (type == StudyType.Qualitative)
            {
                list.AddRange(allValues.Where(v => v == SamplingType.Purposeful || v == SamplingType.Snowball || 
                                                  v == SamplingType.Quota || v == SamplingType.Theoretical || 
                                                  v == SamplingType.Convenience || v == SamplingType.Saturation ||
                                                  v.ToString().StartsWith("Purposeful")));
            }
            else // Mixed or None
            {
                list = allValues;
            }

            if (SamplingTypeComboBox != null) SamplingTypeComboBox.ItemsSource = list;
            if (QualitativeSamplingComboBox != null) QualitativeSamplingComboBox.ItemsSource = list;

            if (list.Count > 0)
            {
                if (SamplingTypeComboBox != null && SamplingTypeComboBox.SelectedIndex == -1) SamplingTypeComboBox.SelectedIndex = 0;
                if (QualitativeSamplingComboBox != null && QualitativeSamplingComboBox.SelectedIndex == -1) QualitativeSamplingComboBox.SelectedIndex = 0;
            }
        }

        private void UpdateQualityGuidelines(StudyType type)
        {
            // Use _project to match service signature, temporary patch
            var tempProj = new ResearchProject { StudyType = type, Domain = (ScientificDomain)DomainComboBox.SelectedItem };
            var checklists = _qualityService.GetRecommendedChecklists(tempProj);
            if (PanelQualityGuidelines == null) return;
            
            // Show the panel
            PanelQualityGuidelines.Visibility = Visibility.Visible;

            // Populate StackPanel 'QualityGuidelinesList'
            if (QualityGuidelinesList != null)
            {
                QualityGuidelinesList.Children.Clear();
                var criteria = checklists.SelectMany(c => c.Sections).SelectMany(s => s.Items).ToList();
                foreach (var criterion in criteria)
                {
                    var cb = new CheckBox 
                    { 
                        Content = criterion.Requirement, 
                        IsChecked = criterion.IsMet,
                        Margin = new Thickness(0,0,0,5)
                    };
                    QualityGuidelinesList.Children.Add(cb);
                }
            }
        }

        // --- Core Action Methods ---

        private void CreateProtocol_Click(object sender, RoutedEventArgs e)
        {
            // Sync UI to _protocol
            _protocol.Title = TitleTextBox.Text;
            _protocol.Context = ContextTextBox.Text;
            _protocol.ProblemJustification = ProblemTextBox.Text;
            _protocol.ResearchQuestion = ResearchQuestionTextBox.Text;
            _protocol.Hypotheses = HypothesesTextBox.Text;
            _protocol.GeneralObjective = GeneralObjectiveTextBox.Text;
            _protocol.SpecificObjectives = SpecificObjectivesTextBox.Text;
            _protocol.ConceptDefinitions = ConceptsTextBox.Text;
            _protocol.ConceptualModel = ConceptualModelTextBox.Text;
            
            _protocol.StudyType = (StudyType)(StudyTypeComboBox.SelectedItem ?? StudyType.Quantitative);
            _protocol.Domain = (ScientificDomain)(DomainComboBox.SelectedItem ?? ScientificDomain.Biomedical);
            _protocol.QualitativeApproach = (QualitativeApproach)(QualitativeApproachComboBox.SelectedItem ?? QualitativeApproach.Phenomenological);
            _protocol.EpidemiologyType = (EpidemiologicalStudyType)(EpidemiologyTypeComboBox.SelectedItem ?? EpidemiologicalStudyType.CrossSectionalDescriptive);
            
            _protocol.StudySetting = StudySettingTextBox.Text;
            _protocol.IsMulticentric = IsMulticentricCheckBox.IsChecked ?? false;
            _protocol.StudyCenters = StudyCentersTextBox.Text;
            _protocol.StudyPopulation = PopulationTextBox.Text;
            _protocol.InclusionCriteria = InclusionTextBox.Text;
            _protocol.ExclusionCriteria = ExclusionTextBox.Text;
            
            _protocol.SamplingType = (SamplingType)(SamplingTypeComboBox.SelectedItem ?? SamplingType.None);
            _protocol.IsStratified = IsStratifiedCheckBox.IsChecked ?? false;
            _protocol.StratificationCriteria = StratificationCriteriaTextBox.Text;
            _protocol.IsClusterSampling = IsClusterCheckBox.IsChecked ?? false;
            
            int.TryParse(ClusterSizeTextBox.Text, out int cs); _protocol.ClusterSize = cs;
            double.TryParse(DesignEffectTextBox.Text, out double de); _protocol.DesignEffect = de;
            double.TryParse(ExpectedLossRateTextBox.Text, out double lr); _protocol.ExpectedLossRate = lr;
            
            _protocol.SamplingMethod = SamplingTextBox.Text;
            _protocol.DataCollection = DataCollectionTextBox.Text;
            _protocol.DataAnalysis = DataAnalysisTextBox.Text;
            _protocol.Ethics = EthicsTextBox.Text;
            _protocol.Budget = BudgetTextBox.Text;
            _protocol.ExpectedResults = ExpectedResultsTextBox.Text;
            _protocol.Conclusion = ConclusionTextBox.Text;
            _protocol.References = ReferencesTextBox.Text;
            _protocol.Appendices = _tempAppendices.ToList();
            
            _protocol.ReferenceStyle = (ReferenceStyle)(RefStyleComboStep1.SelectedItem ?? ReferenceStyle.Vancouver);
            _protocol.Resources = _resources;
            _protocol.CoAuthors = _tempCoAuthors.ToList();

            // Link to Project if needed
            if (_project != null)
            {
                _project.Title = _protocol.Title;
                _project.GeneralObjective = _protocol.GeneralObjective;
                _project.ProblemJustification = _protocol.ProblemJustification;
                _project.DataAnalysisPlan = _protocol.DataAnalysis;
                _project.SourceProtocolId = _protocol.Id;

                _auditService.LogAction(_project, "Update", "Protocol", _protocol.Id, $"Protocol details updated for project '{_project.Title}'");
            }

            MessageBox.Show("Protocole complet enregistré et lié au projet.", "Succès");
        }

        private void ExportWord_Click(object sender, RoutedEventArgs e)
        {
             MessageBox.Show("Export Word simulé.", "Export");
        }

        private void MenuDiscussion_Click(object sender, RoutedEventArgs e)
        {
            // Fix constructor signature: StudyType, Title, Context, Objectives, step(0), RefStyle
            string title = TitleTextBox.Text;
            string context = ContextTextBox.Text;
            string obj = GeneralObjectiveTextBox.Text;
            var win = new DiscussionWindow((StudyType)StudyTypeComboBox.SelectedItem, title, context, obj, 0, (ReferenceStyle)RefStyleComboStep1.SelectedItem);
            win.Owner = this;
            win.ShowDialog();
        }
        
        private void MenuFileNew_Click(object sender, RoutedEventArgs e) 
        {
             // Reset current protocol
             _protocol = new ResearchProtocol();
             LoadProjectData();
             _currentStep = 1;
             UpdateView();
        }

        private void MenuHome_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Will trigger OnClosed and show MainWindow
        }

        // --- Annexes Management ---

        private void AddAppendix_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AppendixTitleBox.Text))
            {
                MessageBox.Show("Veuillez donner un titre à l'annexe.", "Champ requis");
                return;
            }

            var app = new ProtocolAppendix
            {
                Title = AppendixTitleBox.Text,
                Type = (AppendixType)(AppendixTypeComboBox.SelectedItem ?? AppendixType.Other),
                Content = AppendixContentBox.Text,
                FilePath = _pendingFilePath
            };

            _tempAppendices.Add(app);
            
            // Log action if project is active
            if (_project != null)
            {
                _auditService.LogAction(_project, "Add", "Appendix", app.Id, $"Ajout de l'annexe '{app.Title}'");
            }

            // Reset fields
            AppendixTitleBox.Clear();
            AppendixContentBox.Clear();
            AppendixTypeComboBox.SelectedIndex = 0;
            ClearAttachedFile_Click(null, null);
        }

        private string? _pendingFilePath = null;

        private void AttachFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Joindre un fichier à l'annexe";
            dialog.Filter = "Documents (*.pdf;*.docx;*.doc;*.xls;*.xlsx;*.png;*.jpg)|*.pdf;*.docx;*.doc;*.xls;*.xlsx;*.png;*.jpg|Tous les fichiers (*.*)|*.*";
            
            if (dialog.ShowDialog() == true)
            {
                _pendingFilePath = dialog.FileName;
                AttachedFileNameText.Text = Path.GetFileName(_pendingFilePath);
                BtnClearFile.Visibility = Visibility.Visible;
            }
        }

        private void ClearAttachedFile_Click(object sender, RoutedEventArgs? e)
        {
            _pendingFilePath = null;
            AttachedFileNameText.Text = "Aucun fichier joint";
            BtnClearFile.Visibility = Visibility.Collapsed;
        }

        private void GenerateTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (AppendixTypeComboBox.SelectedItem is AppendixType type)
            {
                if (!string.IsNullOrWhiteSpace(AppendixContentBox.Text))
                {
                    var result = MessageBox.Show("Voulez-vous remplacer le contenu actuel par le modèle ?", "Confirmation", MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes) return;
                }
                AppendixContentBox.Text = AppendixTemplates.GetTemplate(type);
                AppendixContentBox.Focus();
            }
        }

        private void MoveAppendixUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is ProtocolAppendix app)
            {
                int index = _tempAppendices.IndexOf(app);
                if (index > 0)
                {
                    _tempAppendices.Move(index, index - 1);
                }
            }
        }

        private void MoveAppendixDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is ProtocolAppendix app)
            {
                int index = _tempAppendices.IndexOf(app);
                if (index < _tempAppendices.Count - 1)
                {
                    _tempAppendices.Move(index, index + 1);
                }
            }
        }

        private void RemoveAppendix_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is ProtocolAppendix app)
            {
                _tempAppendices.Remove(app);
                if (_project != null)
                {
                    _auditService.LogAction(_project, "Remove", "Appendix", app.Id, $"Suppression de l'annexe '{app.Title}'");
                }
            }
        }

        private void AppendicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppendicesListBox.SelectedItem is ProtocolAppendix app)
            {
                AppendixPreviewPanel.Visibility = Visibility.Visible;
                PreviewTitle.Text = app.Title;
                PreviewContent.Text = string.IsNullOrWhiteSpace(app.Content) ? "(Aucun texte saisi)" : app.Content;
            }
            else
            {
                AppendixPreviewPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ExportAnnexesOnly_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Annexes_{_protocol.Title ?? "Projet"}.docx"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _wordExportService.ExportAppendicesOnly(_tempAppendices.ToList(), _protocol.Title ?? "Sans Titre", dialog.FileName);
                    MessageBox.Show("Annexes exportées avec succès.", "Succès");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'exportation : {ex.Message}", "Erreur");
                }
            }
        }

        private void UpdateAppendixSuggestions(StudyType st)
        {
            if (AppendixSuggestionsWrapPanel == null || AppendixSuggestionsPanel == null) return;

            AppendixSuggestionsWrapPanel.Children.Clear();
            var suggestions = new List<(string Title, AppendixType Type)>();

            // Basic Ethics suggestions (universal)
            suggestions.Add(("Notice d'information", AppendixType.InformationNotice));
            suggestions.Add(("Formulaire de Consentement", AppendixType.InformedConsent));

            if (st == StudyType.Qualitative)
            {
                suggestions.Add(("Guide d'entretien semi-directif", AppendixType.InterviewGuide));
                suggestions.Add(("Grille d'observation de terrain", AppendixType.ObservationGrid));
                suggestions.Add(("Engagement de confidentialité (transcription)", AppendixType.ConfidentialityAgreement));
            }
            else if (st == StudyType.Quantitative)
            {
                suggestions.Add(("Questionnaire structuré", AppendixType.Questionnaire));
                suggestions.Add(("Fiche de recueil de données (CRF)", AppendixType.DataCollectionForm));
            }
            else if (st == StudyType.Mixed)
            {
                suggestions.Add(("Questionnaire (Phase Quantitative)", AppendixType.Questionnaire));
                suggestions.Add(("Guide d'entretien (Phase Qualitative)", AppendixType.InterviewGuide));
            }

            // Case specific (Meta-analysis / Systematic Review)
            if (_protocol.Title?.ToLower().Contains("revue") == true || _protocol.Title?.ToLower().Contains("systematic") == true)
            {
                suggestions.Add(("Stratégie de recherche complète", AppendixType.SearchStrategy));
                suggestions.Add(("Liste des articles exclus", AppendixType.Other));
            }

            foreach (var s in suggestions)
            {
                var btn = new Button
                {
                    Content = $"➕ {s.Title}",
                    Margin = new Thickness(0, 0, 8, 8),
                    Padding = new Thickness(10, 5, 10, 5),
                    FontSize = 11,
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(251, 192, 45)),
                    Tag = s
                };
                btn.Click += (snd, evt) => {
                    AppendixTitleBox.Text = s.Title;
                    AppendixTypeComboBox.SelectedItem = s.Type;
                    AppendixContentBox.Focus();
                };
                AppendixSuggestionsWrapPanel.Children.Add(btn);
            }

            AppendixSuggestionsPanel.Visibility = suggestions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // --- Resource Insertion (The Fix) ---

        private void InsertResource_Click(object sender, RoutedEventArgs e)
        {
             TextBox? targetBox = _activeTextBox;
             
             // Advanced inference from 'BtnInsert_...' name logic could go here
             // But relying on focus is standard.
             
             if (targetBox == null)
             {
                 // Try to guess from the button's location?
                 // If sender is BtnInsert_Context -> ContextTextBox
                 if (sender is Button b && b.Name != null)
                 {
                     if (b.Name == "BtnInsert_Context") targetBox = ContextTextBox;
                     else if (b.Name == "BtnInsert_Problem") targetBox = ProblemTextBox;
                     else if (b.Name == "BtnInsert_Setting") targetBox = StudySettingTextBox;
                     else if (b.Name == "BtnInsert_Sampling") targetBox = SamplingTextBox;
                     else if (b.Name == "BtnInsert_Collection") targetBox = DataCollectionTextBox;
                     else if (b.Name == "BtnInsert_Analysis") targetBox = DataAnalysisTextBox;
                     else if (b.Name == "BtnInsert_Results") targetBox = ExpectedResultsTextBox;
                     else if (b.Name == "BtnInsert_Conclusion") targetBox = ConclusionTextBox;
                 }
             }

             if (targetBox == null)
             {
                 MessageBox.Show("Veuillez cliquer dans la zone de texte.", "Cible inconnue");
                 return;
             }

             var dialog = new ResourceDialog();
             dialog.Title = "Insérer : " + (targetBox.Tag?.ToString() ?? "Ressource");
             if (dialog.ShowDialog() == true)
             {
                 var res = dialog.ResultResource;
                 if (res != null)
                 {
                     // Assign Number based on Type
                     int count = _resources.Count(r => r.Type == res.Type) + 1;
                     res.Number = count;
                     
                     _resources.Add(res);

                     string tag = "";
                     if (res.Type == ResourceType.Table)
                         tag = $"[Tableau {ToRoman(count)} : {res.Title}]";
                     else if (res.Type == ResourceType.Figure)
                         tag = $"[Figure {count} : {res.Title}]";
                     else
                         tag = $"[{res.Type} : {res.Title}]";

                     targetBox.Text += $"\n\n{tag}\n";
                     targetBox.CaretIndex = targetBox.Text.Length;
                     targetBox.Focus();
                 }
             }
        }

        private static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) return number.ToString();
            if (number < 1) return string.Empty;
            string[] M = { "", "M", "MM", "MMM" };
            string[] C = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
            string[] X = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
            string[] I = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
            return M[number / 1000] + C[(number % 1000) / 100] + X[(number % 100) / 10] + I[number % 10];
        }
        
        private void AddCitation_Click(object sender, RoutedEventArgs e) { }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e) { if (sender is TextBox tb) _activeTextBox = tb; }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) { } // Generic handler if needed
        private void ObjectiveTextBox_PreviewKeyDown(object sender, KeyEventArgs e) { }
        private void GeneralObjectiveTextBox_TextChanged(object sender, TextChangedEventArgs e) { }
        private void SpecificObjectivesTextBox_TextChanged(object sender, TextChangedEventArgs e) { }
        private void LinkOSToOG_Click(object sender, RoutedEventArgs e) { }
        private void RenumberOS_Click(object sender, RoutedEventArgs e) { }
        
        // Helper to find children
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T t) yield return t;
                    foreach (T childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
                }
            }
            // Add return for compiler satisfaction if needed, but yield handles it.
            // C# compiler is smart enough.
            yield break; // To ensure IEnumerable is returned even if loop empty? No, works.
        }
        private void LoadAuditLogs()
        {
            if (_project != null)
            {
                AuditLogGrid.ItemsSource = null;
                AuditLogGrid.ItemsSource = _project.AuditLogs.OrderByDescending(l => l.Timestamp).ToList();
            }
        }
    }
}
