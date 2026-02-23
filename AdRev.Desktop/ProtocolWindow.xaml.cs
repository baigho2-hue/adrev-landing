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
using AdRev.Desktop.Windows;
using Microsoft.Win32;
using System.IO;
using AdRev.Desktop.Services;

namespace AdRev.Desktop
{
    public partial class ProtocolWindow : Window
    {
        private readonly ProtocolService _service = new ProtocolService();
        private readonly QualityService _qualityService = new QualityService();
        private readonly WordExportService _wordExportService = new WordExportService();
        private readonly StatisticsService _statsService = new StatisticsService();
        private readonly AuditService _auditService = new AuditService(new ResearcherProfileService());

        private ObservableCollection<Author> _tempCoAuthors = new ObservableCollection<Author>();
        private List<ProtocolResource> _resources = new List<ProtocolResource>();
        private ResearchProtocol _protocol = new ResearchProtocol();
        private ResearchProject _project;
        private FunctionalRole _currentRole;
        private UserAccessLevel _currentAccessLevel;
        private ObservableCollection<ProtocolAppendix> _tempAppendices = new ObservableCollection<ProtocolAppendix>();

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
            if (StudyTypeComboBox.ItemsSource != null) return;

            StudyTypeComboBox.ItemsSource = Enum.GetValues(typeof(StudyType));
            EpidemiologyTypeComboBox.ItemsSource = Enum.GetValues(typeof(EpidemiologicalStudyType));
            QualitativeApproachComboBox.ItemsSource = Enum.GetValues(typeof(QualitativeApproach));
            SamplingTypeComboBox.ItemsSource = Enum.GetValues(typeof(SamplingType));
            QualitativeSamplingComboBox.ItemsSource = Enum.GetValues(typeof(SamplingType));
            DomainComboBox.ItemsSource = Enum.GetValues(typeof(ScientificDomain));
            
            RefStyleComboStep1.ItemsSource = Enum.GetValues(typeof(ReferenceStyle));
            RefStyleComboStep13.ItemsSource = Enum.GetValues(typeof(ReferenceStyle));
            
            ActiveRoleComboBox.ItemsSource = Enum.GetValues(typeof(FunctionalRole));
            ActiveAccessComboBox.ItemsSource = Enum.GetValues(typeof(UserAccessLevel));

            CoAuthorsListBox.ItemsSource = _tempCoAuthors;
            AppendicesListBox.ItemsSource = _tempAppendices;
            AppendixTypeComboBox.ItemsSource = Enum.GetValues(typeof(AppendixType));

            StudyTypeComboBox.SelectedIndex = 0;
            RefStyleComboStep1.SelectedItem = ReferenceStyle.Vancouver;
            
            ActiveRoleComboBox.SelectedItem = _currentRole;
            ActiveAccessComboBox.SelectedItem = _currentAccessLevel;

            ApplyPermissions();
        }

        private void LoadProjectData()
        {
            if (_project == null) return;

            ProjectTitleTextBlock.Text = "Projet : " + _project.Title;

            _protocol.Title = _project.Title;
            _protocol.Domain = _project.Domain;
            _protocol.StudyType = _project.StudyType;
            _protocol.EpidemiologyType = _project.EpidemiologyType;

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

            _tempAppendices.Clear();
            foreach (var app in _protocol.Appendices) _tempAppendices.Add(app);

            _tempCoAuthors.Clear();
            foreach(var auth in _protocol.CoAuthors) _tempCoAuthors.Add(auth);
            
            _resources = _protocol.Resources;

            UpdateView();
        }

        private void ActiveRoleAndAccess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveRoleComboBox == null || ActiveAccessComboBox == null) return;
            if (ActiveRoleComboBox.SelectedItem is FunctionalRole role && ActiveAccessComboBox.SelectedItem is UserAccessLevel access)
            {
                _currentRole = role;
                _currentAccessLevel = access;
                ApplyPermissions();
            }
        }

        private void ApplyPermissions()
        {
            if (MenuFileSave == null) return;

            MenuFileSave.Visibility = Visibility.Visible;
            MenuDiscussion.Visibility = Visibility.Visible;

            if (_currentAccessLevel == UserAccessLevel.Viewer || _currentRole == FunctionalRole.Student)
            {
                MenuFileSave.Visibility = Visibility.Collapsed;
            }

            if (_currentRole == FunctionalRole.DataManager)
            {
                MenuDiscussion.Visibility = Visibility.Collapsed;
            }
            
            UpdateView();
        }

        private bool CanUserEditStep(int step)
        {
            if (_currentAccessLevel == UserAccessLevel.Admin) return true;
            if (_currentAccessLevel == UserAccessLevel.Viewer) return false;

            switch (_currentRole)
            {
                case FunctionalRole.PrincipalInvestigator: return true;
                case FunctionalRole.Methodologist: return (step != 9);
                case FunctionalRole.Statistician: return (step == 1 || step == 7 || step == 8 || step == 11 || step == 12);
                case FunctionalRole.DataManager: return (step == 1 || step == 8 || step == 9 || step == 11);
                case FunctionalRole.CoInvestigator: return (step != 9 && step != 10);
                case FunctionalRole.Student: return (step <= 6 || step == 12 || step == 13);
                case FunctionalRole.Monitor: return false;
                default: return true; 
            }
        }

        private void SetProtocolReadOnly(bool isReadOnly)
        {
            foreach (var tb in FindVisualChildren<TextBox>(this))
            {
                tb.IsReadOnly = isReadOnly;
                tb.Opacity = isReadOnly ? 0.8 : 1.0;
                if (!isReadOnly) tb.IsEnabled = true;
            }

            foreach (var cb in FindVisualChildren<ComboBox>(this))
            {
                if (cb.Name == "ActiveRoleComboBox" || cb.Name == "ActiveAccessComboBox") continue;
                cb.IsEnabled = !isReadOnly;
                cb.Opacity = isReadOnly ? 0.8 : 1.0;
            }

            foreach (var chk in FindVisualChildren<CheckBox>(this))
            {
                chk.IsEnabled = !isReadOnly;
                chk.Opacity = isReadOnly ? 0.8 : 1.0;
            }

            foreach (var btn in FindVisualChildren<Button>(this))
            {
                if (btn.Name == "BtnMinimize" || btn.Name == "BtnClose" || btn.Name == "BtnMaximize") continue;
                
                if (isReadOnly)
                {
                    if (btn.Content is string s && (s.Contains("AJOUTER") || s.Contains("CALCULER") || s.Contains("ENREGISTRER") || s.Contains("Supprimer") || s.Contains("Add") || s.Contains("Calculate") || s.Contains("Save") || s.Contains("Delete")))
                    {
                        btn.IsEnabled = false;
                    }
                }
                else
                {
                    btn.IsEnabled = true;
                }
            }
        }

        private void SideMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SideMenuListBox.SelectedItem is ListBoxItem item && item.Tag != null && int.TryParse(item.Tag.ToString(), out int s))
            {
                _currentStep = s;
                UpdateView();
            }
        }

        private void UpdateView()
        {
            if (View_Step1 == null) return;

            View_Step1.Visibility = Visibility.Collapsed;
            View_Introduction.Visibility = Visibility.Collapsed;
            View_Objectives.Visibility = Visibility.Collapsed;
            View_ConceptualFramework.Visibility = Visibility.Collapsed;
            View_Methodology_Design.Visibility = Visibility.Collapsed;
            View_Methodology_Population.Visibility = Visibility.Collapsed;
            View_Methodology_Sampling.Visibility = Visibility.Collapsed;
            View_Methodology_Data.Visibility = Visibility.Collapsed;
            View_Budget.Visibility = Visibility.Collapsed;
            View_Ethics.Visibility = Visibility.Collapsed;
            View_DataAnalysis.Visibility = Visibility.Collapsed;
            View_Results.Visibility = Visibility.Collapsed;
            View_Conclusion.Visibility = Visibility.Collapsed;
            View_References.Visibility = Visibility.Collapsed;
            View_AuditLogs.Visibility = Visibility.Collapsed;
            View_Appendices.Visibility = Visibility.Collapsed;

            switch (_currentStep)
            {
                case 1: View_Step1.Visibility = Visibility.Visible; break;
                case 2: View_Introduction.Visibility = Visibility.Visible; break;
                case 3: View_Objectives.Visibility = Visibility.Visible; break;
                case 4: View_ConceptualFramework.Visibility = Visibility.Visible; break;
                case 5: View_Methodology_Design.Visibility = Visibility.Visible; break;
                case 6: View_Methodology_Population.Visibility = Visibility.Visible; break;
                case 7: View_Methodology_Sampling.Visibility = Visibility.Visible; break;
                case 8: View_Methodology_Data.Visibility = Visibility.Visible; break;
                case 9: View_Budget.Visibility = Visibility.Visible; break;
                case 10: View_Ethics.Visibility = Visibility.Visible; break;
                case 11: View_DataAnalysis.Visibility = Visibility.Visible; break;
                case 12: View_Results.Visibility = Visibility.Visible; break;
                case 13: View_Conclusion.Visibility = Visibility.Visible; break;
                case 14: View_References.Visibility = Visibility.Visible; break;
                case 15: View_AuditLogs.Visibility = Visibility.Visible; break;
                case 16: View_Appendices.Visibility = Visibility.Visible; break;
            }

            bool isReadOnly = !CanUserEditStep(_currentStep);
            SetProtocolReadOnly(isReadOnly);
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();

        private void CreateProtocol_Click(object sender, RoutedEventArgs e)
        {
            _protocol.Title = TitleTextBox.Text;
            _protocol.Domain = DomainComboBox.SelectedItem is ScientificDomain d ? d : ScientificDomain.Biomedical;
            _protocol.ReferenceStyle = RefStyleComboStep1.SelectedItem is ReferenceStyle rs ? rs : ReferenceStyle.Vancouver;

            _protocol.Context = ContextTextBox.Text;
            _protocol.ProblemJustification = ProblemTextBox.Text;
            _protocol.ResearchQuestion = ResearchQuestionTextBox.Text;
            _protocol.Hypotheses = HypothesesTextBox.Text;

            _protocol.GeneralObjective = GeneralObjectiveTextBox.Text;
            _protocol.SpecificObjectives = SpecificObjectivesTextBox.Text;
            
            _protocol.ConceptDefinitions = ConceptsTextBox.Text;
            _protocol.ConceptualModel = ConceptualModelTextBox.Text;

            _protocol.StudyType = StudyTypeComboBox.SelectedItem is StudyType st ? st : StudyType.Quantitative;
            _protocol.EpidemiologyType = EpidemiologyTypeComboBox.SelectedItem is EpidemiologicalStudyType et ? et : EpidemiologicalStudyType.CrossSectionalDescriptive;
            _protocol.QualitativeApproach = QualitativeApproachComboBox.SelectedItem is QualitativeApproach qa ? qa : QualitativeApproach.Phenomenological;
            
            _protocol.StudySetting = StudySettingTextBox.Text;
            _protocol.IsMulticentric = IsMulticentricCheckBox.IsChecked ?? false;
            _protocol.StudyCenters = StudyCentersTextBox.Text;

            _protocol.StudyPopulation = PopulationTextBox.Text;
            _protocol.InclusionCriteria = InclusionTextBox.Text;
            _protocol.ExclusionCriteria = ExclusionTextBox.Text;

            _protocol.SamplingType = SamplingTypeComboBox.SelectedItem is SamplingType sat ? sat : SamplingType.SimpleRandom;
            _protocol.IsStratified = IsStratifiedCheckBox.IsChecked ?? false;
            _protocol.StratificationCriteria = StratificationCriteriaTextBox.Text;
            _protocol.IsClusterSampling = IsClusterCheckBox.IsChecked ?? false;
            
            if (double.TryParse(ClusterSizeTextBox.Text, out double cs)) _protocol.ClusterSize = (int)cs;
            if (double.TryParse(DesignEffectTextBox.Text, out double de)) _protocol.DesignEffect = de;
            if (double.TryParse(ExpectedLossRateTextBox.Text, out double elr)) _protocol.ExpectedLossRate = elr;
            
            _protocol.SamplingMethod = SamplingTextBox.Text;
            
            _protocol.DataCollection = DataCollectionTextBox.Text;
            _protocol.Budget = BudgetTextBox.Text;
            _protocol.Ethics = EthicsTextBox.Text;
            _protocol.DataAnalysis = DataAnalysisTextBox.Text;
            _protocol.ExpectedResults = ExpectedResultsTextBox.Text;
            _protocol.Conclusion = ConclusionTextBox.Text;
            _protocol.References = ReferencesTextBox.Text;

            _protocol.CoAuthors.Clear();
            foreach (var auth in _tempCoAuthors) _protocol.CoAuthors.Add(auth);

            _protocol.Appendices.Clear();
            foreach (var app in _tempAppendices) _protocol.Appendices.Add(app);

            _service.Create(_protocol);
            MessageBox.Show("Protocole enregistré avec succès.");
        }

        private void ExportWord_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "Word Document (*.docx)|*.docx", FileName = $"Protocole_{_protocol.Title}.docx" };
            if (dialog.ShowDialog() == true)
            {
                _wordExportService.ExportProtocolToWord(_protocol, dialog.FileName);
                MessageBox.Show("Exportation terminée.");
            }
        }

        private void MenuDiscussion_Click(object sender, RoutedEventArgs e)
        {
            var win = new DiscussionWindow(
                _protocol.StudyType,
                _protocol.SpecificObjectives ?? string.Empty,
                _protocol.DiscussionPlan ?? string.Empty,
                _protocol.StudyLimitations ?? string.Empty,
                1,
                _protocol.ReferenceStyle);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                _protocol.DiscussionPlan = win.DiscussionPlan;
                _protocol.StudyLimitations = win.Limitations;
            }
        }

        private void AddAppendix_Click(object sender, RoutedEventArgs e)
        {
            var type = AppendixTypeComboBox.SelectedItem is AppendixType t ? t : AppendixType.Other;
            var app = new ProtocolAppendix 
            { 
                Title = type.ToString(), // Can be improved
                Type = type 
            };
            _tempAppendices.Add(app);
        }

        private void RemoveAppendix_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProtocolAppendix app)
            {
                _tempAppendices.Remove(app);
            }
        }

        private void AddCoAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CoAuthFirst.Text) && !string.IsNullOrWhiteSpace(CoAuthLast.Text))
            {
                _tempCoAuthors.Add(new Author 
                { 
                    FirstName = CoAuthFirst.Text, 
                    LastName = CoAuthLast.Text,
                    Institution = CoAuthInst.Text
                });
                
                CoAuthFirst.Clear();
                CoAuthLast.Clear();
                CoAuthInst.Clear();
            }
        }

        private void RemoveCoAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Author author)
            {
                _tempCoAuthors.Remove(author);
            }
        }

        private void StudyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudyTypeComboBox.SelectedItem is StudyType st)
            {
                QualitativeTypePanel.Visibility = (st == StudyType.Qualitative || st == StudyType.Mixed) ? Visibility.Visible : Visibility.Collapsed;
                EpidemiologyTypePanel.Visibility = (st == StudyType.Quantitative || st == StudyType.Mixed) ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        private void DomainComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void RefStyleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void ActiveRoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void ActiveAccessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void MenuFileNew_Click(object sender, RoutedEventArgs e) { }
        private void MenuHome_Click(object sender, RoutedEventArgs e) { }
        private void InsertResource_Click(object sender, RoutedEventArgs e) { }
        private void AddCitation_Click(object sender, RoutedEventArgs e) { }
        private void GeneralObjectiveTextBox_TextChanged(object sender, TextChangedEventArgs e) { }
        private void ObjectiveTextBox_PreviewKeyDown(object sender, KeyEventArgs e) { }
        private void SpecificObjectivesTextBox_TextChanged(object sender, TextChangedEventArgs e) { }
        private void LinkOSToOG_Click(object sender, RoutedEventArgs e) { }
        private void RenumberOS_Click(object sender, RoutedEventArgs e) { }
        private void RefreshAudit_LostFocus(object sender, RoutedEventArgs e) { }
        private void EpidemiologyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void IsMulticentricCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (MulticentricDetailsPanel != null) MulticentricDetailsPanel.Visibility = Visibility.Visible;
        }

        private void IsMulticentricCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (MulticentricDetailsPanel != null) MulticentricDetailsPanel.Visibility = Visibility.Collapsed;
        }

        private void SamplingTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SamplingTypeComboBox.SelectedItem is SamplingType st)
            {
                if (StratificationPanel != null)
                    StratificationPanel.Visibility = (st == SamplingType.Stratified || st == SamplingType.MultiStage) ? Visibility.Visible : Visibility.Collapsed;
                
                if (ClusterPanel != null)
                    ClusterPanel.Visibility = (st == SamplingType.ClusterSampling || st == SamplingType.MultiStage) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void IsStratifiedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (StratificationCriteriaTextBox != null) StratificationCriteriaTextBox.Visibility = Visibility.Visible;
        }

        private void IsStratifiedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (StratificationCriteriaTextBox != null) StratificationCriteriaTextBox.Visibility = Visibility.Collapsed;
        }

        private void IsClusterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Already handled by panel visibility usually, but can toggle specific fields
        }

        private void IsClusterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void CalculatorSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelCochran == null) return;
            PanelCochran.Visibility = (CalculatorSelector.SelectedIndex == 1) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CalculateCochran_Click(object sender, RoutedEventArgs e) 
        {
            MessageBox.Show("Calculateur Cochran : N = (Z² * p * q) / e²\nRésultat estimé : 384", "Calculateur");
        }

        private void QualitativeApproachComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T) yield return (T)child;
                    foreach (T childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
                }
            }
        }
    }
}
