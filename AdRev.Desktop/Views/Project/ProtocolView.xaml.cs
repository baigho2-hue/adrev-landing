using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
<<<<<<< HEAD
using AdRev.Desktop.Windows;
=======
>>>>>>> origin/main
using System.Collections.Generic;
using System.Linq;
using AdRev.Core.Protocols;
using AdRev.Domain.Enums;
using AdRev.Domain.Protocols;
using AdRev.Domain.Models;
using AdRev.Core.Services;
using Microsoft.Win32;
using AdRev.Desktop.Services;
using System.Text.RegularExpressions;
using System.Text;

namespace AdRev.Desktop.Views.Project
{
    public partial class ProtocolView : UserControl
    {
        private readonly ProtocolService _service = new ProtocolService();
        private readonly QualityService _qualityService = new QualityService();
        private readonly WordExportService _wordExportService = new WordExportService();

        private System.Collections.ObjectModel.ObservableCollection<AdRev.Domain.Models.Author> _tempCoAuthors 
            = new System.Collections.ObjectModel.ObservableCollection<AdRev.Domain.Models.Author>();

        private System.Collections.Generic.List<AdRev.Domain.Variables.StudyVariable> _tempVariables 
            = new System.Collections.Generic.List<AdRev.Domain.Variables.StudyVariable>();

        private string _discussionPlan = string.Empty;
        private string _limitations = string.Empty;
        private List<AdRev.Domain.Models.Citation> _citations = new List<AdRev.Domain.Models.Citation>();
        private ReferenceStyle _lastStyle = ReferenceStyle.Vancouver;
        private bool _isSyncingStyle = false;

        private AdRev.Domain.Models.ResearchProject? _project;
        private FunctionalRole _currentRole = FunctionalRole.PrincipalInvestigator;
        private UserAccessLevel _currentAccessLevel = UserAccessLevel.Admin;
        private int _currentStep = 1;
        private const int TotalSteps = 14;

        public ProtocolView()
        {
            InitializeComponent();
<<<<<<< HEAD
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (SideMenuListBox != null && SideMenuListBox.SelectedIndex == -1)
            {
                SideMenuListBox.SelectedIndex = 0;
            }
            UpdateView();
=======
            
            // Wire up selection changed and set initial index ONLY after components are initialized
            if (SideMenuListBox != null)
            {
                SideMenuListBox.SelectionChanged += SideMenu_SelectionChanged;
                SideMenuListBox.SelectedIndex = 0;
            }
>>>>>>> origin/main
        }

        public void LoadProject(AdRev.Domain.Models.ResearchProject project, FunctionalRole role = FunctionalRole.PrincipalInvestigator, UserAccessLevel access = UserAccessLevel.Admin)
        {
            _project = project;
            _currentRole = role;
            _currentAccessLevel = access;

            StudyTypeComboBox.ItemsSource = System.Enum.GetValues(typeof(StudyType));
            StudyTypeComboBox.SelectedIndex = 0;
            
            // Note: EpidemiologyTypeComboBox and QualitativeApproachComboBox might be null if not in XAML yet
            if (EpidemiologyTypeComboBox != null)
            {
                EpidemiologyTypeComboBox.ItemsSource = System.Enum.GetValues(typeof(EpidemiologicalStudyType));
                EpidemiologyTypeComboBox.SelectedIndex = 0;
            }

<<<<<<< HEAD
            if (QualitativeApproachComboBox != null)
            {
                QualitativeApproachComboBox.ItemsSource = System.Enum.GetValues(typeof(QualitativeApproach));
                QualitativeApproachComboBox.SelectedIndex = 0;
            }

            if (QualitativeSamplingComboBox != null)
            {
                QualitativeSamplingComboBox.ItemsSource = System.Enum.GetValues(typeof(SamplingType)).Cast<SamplingType>()
                    .Where(t => IsQualitativeSampling(t)).ToList();
                QualitativeSamplingComboBox.SelectedIndex = 0;
            }
            
            UpdateSamplingTypes();

=======
>>>>>>> origin/main
            // ... Initialize other combos ...
            
            CoAuthorsListBox.ItemsSource = _tempCoAuthors;

            if (_project != null)
            {
                TitleTextBox.Text = _project.Title;
                StudyTypeComboBox.SelectedItem = _project.StudyType;
                DomainComboBox.SelectedItem = _project.Domain;
                AuthorInstitutionBox.Text = _project.Institution;
                ProcessAuthors(_project.Authors);
<<<<<<< HEAD
                _tempVariables = _project.Variables ?? new List<AdRev.Domain.Variables.StudyVariable>();
            }
            
            RefStyleComboStep1.ItemsSource = System.Enum.GetValues(typeof(ReferenceStyle));
            RefStyleComboStep1.SelectedItem = _project?.ReferenceStyle ?? ReferenceStyle.Vancouver;
=======
            }
            
            RefStyleComboStep1.ItemsSource = System.Enum.GetValues(typeof(ReferenceStyle));
            RefStyleComboStep1.SelectedItem = ReferenceStyle.Vancouver;
>>>>>>> origin/main

            DomainComboBox.ItemsSource = System.Enum.GetValues(typeof(ScientificDomain));
            DomainComboBox.SelectedIndex = 0;

            UpdateView();
        }

<<<<<<< HEAD
        public void LoadVariables(List<AdRev.Domain.Variables.StudyVariable> variables)
        {
            _tempVariables = variables ?? new List<AdRev.Domain.Variables.StudyVariable>();
        }

=======
>>>>>>> origin/main
        private void DomainComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Logic moved from ProtocolWindow
        }

        private void ProcessAuthors(string authorsCsv)
        {
            if (string.IsNullOrWhiteSpace(authorsCsv)) return;
            var names = authorsCsv.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var name in names)
            {
                var trimmed = name.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                
                var parts = trimmed.Split(' ');
                string first = parts[0];
                string last = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";

                _tempCoAuthors.Add(new AdRev.Domain.Models.Author 
                { 
                    FirstName = first, 
                    LastName = last,
                    Institution = _project?.Institution ?? ""
                });
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
                    Institution = CoAuthInst.Text,
                    Email = CoAuthEmail.Text
                });
                
                CoAuthFirst.Clear();
                CoAuthLast.Clear();
                CoAuthInst.Clear();
                CoAuthEmail.Clear();
            }
        }

        private void RemoveCoAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (CoAuthorsListBox.SelectedItem is Author author)
            {
                _tempCoAuthors.Remove(author);
            }
        }

        private void SideMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SideMenuListBox.SelectedItem is ListBoxItem item && item.Tag != null)
            {
                if (int.TryParse(item.Tag.ToString(), out int step))
                {
                    _currentStep = step;
                    UpdateView();
                }
            }
        }

        private void UpdateView()
        {
<<<<<<< HEAD
            this.Dispatcher.InvokeAsync(() => {
                // Reset all to Collapsed
                if (View_Step1 != null) View_Step1.Visibility = Visibility.Collapsed;
                if (View_Introduction != null) View_Introduction.Visibility = Visibility.Collapsed;
                if (View_Objectives != null) View_Objectives.Visibility = Visibility.Collapsed;
                if (View_ConceptualModel != null) View_ConceptualModel.Visibility = Visibility.Collapsed;
                if (View_Methodology_Design != null) View_Methodology_Design.Visibility = Visibility.Collapsed;
                if (View_Methodology_Population != null) View_Methodology_Population.Visibility = Visibility.Collapsed;
                if (View_Methodology_Sampling != null) View_Methodology_Sampling.Visibility = Visibility.Collapsed;
                if (View_Methodology_Data != null) View_Methodology_Data.Visibility = Visibility.Collapsed;
                if (View_Budget != null) View_Budget.Visibility = Visibility.Collapsed;
                if (View_Ethics != null) View_Ethics.Visibility = Visibility.Collapsed;
                if (View_DataAnalysis != null) View_DataAnalysis.Visibility = Visibility.Collapsed;
                if (View_Results != null) View_Results.Visibility = Visibility.Collapsed;
                if (View_Conclusion != null) View_Conclusion.Visibility = Visibility.Collapsed;
                if (View_References != null) View_References.Visibility = Visibility.Collapsed;

                // Sync Side Menu if needed
                if (SideMenuListBox != null && SideMenuListBox.SelectedIndex != _currentStep - 1)
                {
                    SideMenuListBox.SelectedIndex = _currentStep - 1;
                }

                // Show current
                switch (_currentStep)
                {
                    case 1: if (View_Step1 != null) View_Step1.Visibility = Visibility.Visible; break;
                    case 2: if (View_Introduction != null) View_Introduction.Visibility = Visibility.Visible; break;
                    case 3: if (View_Objectives != null) View_Objectives.Visibility = Visibility.Visible; break;
                    case 4: if (View_ConceptualModel != null) View_ConceptualModel.Visibility = Visibility.Visible; break;
                    case 5: 
                        if (View_Methodology_Design != null) View_Methodology_Design.Visibility = Visibility.Visible;
                        UpdateQualityGuidelines((StudyType)StudyTypeComboBox.SelectedItem);
                        break;
                    case 6: if (View_Methodology_Population != null) View_Methodology_Population.Visibility = Visibility.Visible; break;
                    case 7: if (View_Methodology_Sampling != null) View_Methodology_Sampling.Visibility = Visibility.Visible; break;
                    case 8: if (View_Methodology_Data != null) View_Methodology_Data.Visibility = Visibility.Visible; break;
                    case 9: 
                        if (View_Budget != null) View_Budget.Visibility = Visibility.Visible; 
                        UpdateBudgetView();
                        break;
                    case 10: if (View_Ethics != null) View_Ethics.Visibility = Visibility.Visible; break;
                    case 11: if (View_DataAnalysis != null) View_DataAnalysis.Visibility = Visibility.Visible; break;
                    case 12: if (View_Results != null) View_Results.Visibility = Visibility.Visible; break;
                    case 13: if (View_Conclusion != null) View_Conclusion.Visibility = Visibility.Visible; break;
                    case 14: if (View_References != null) View_References.Visibility = Visibility.Visible; break;
                }

                // Apply Permissions
                bool canEdit = CanUserEditStep(_currentStep);
                SetProtocolReadOnly(!canEdit);
            });
=======
            // Reset all to Collapsed
            if (View_Step1 != null) View_Step1.Visibility = Visibility.Collapsed;
            if (View_Introduction != null) View_Introduction.Visibility = Visibility.Collapsed;
            if (View_Objectives != null) View_Objectives.Visibility = Visibility.Collapsed;
            if (View_ConceptualModel != null) View_ConceptualModel.Visibility = Visibility.Collapsed;
            if (View_Methodology_Design != null) View_Methodology_Design.Visibility = Visibility.Collapsed;
            if (View_Methodology_Population != null) View_Methodology_Population.Visibility = Visibility.Collapsed;
            if (View_Methodology_Sampling != null) View_Methodology_Sampling.Visibility = Visibility.Collapsed;
            if (View_Methodology_Data != null) View_Methodology_Data.Visibility = Visibility.Collapsed;
            if (View_Budget != null) View_Budget.Visibility = Visibility.Collapsed;
            if (View_Ethics != null) View_Ethics.Visibility = Visibility.Collapsed;
            if (View_DataAnalysis != null) View_DataAnalysis.Visibility = Visibility.Collapsed;
            if (View_Results != null) View_Results.Visibility = Visibility.Collapsed;
            if (View_Conclusion != null) View_Conclusion.Visibility = Visibility.Collapsed;
            if (View_References != null) View_References.Visibility = Visibility.Collapsed;

            // Sync Side Menu if needed
            if (SideMenuListBox != null && SideMenuListBox.SelectedIndex != _currentStep - 1)
            {
                SideMenuListBox.SelectedIndex = _currentStep - 1;
            }

            // Show current
            switch (_currentStep)
            {
                case 1: if (View_Step1 != null) View_Step1.Visibility = Visibility.Visible; break;
                case 2: if (View_Introduction != null) View_Introduction.Visibility = Visibility.Visible; break;
                case 3: if (View_Objectives != null) View_Objectives.Visibility = Visibility.Visible; break;
                case 4: if (View_ConceptualModel != null) View_ConceptualModel.Visibility = Visibility.Visible; break;
                case 5: 
                    if (View_Methodology_Design != null) View_Methodology_Design.Visibility = Visibility.Visible;
                    UpdateQualityGuidelines((StudyType)StudyTypeComboBox.SelectedItem);
                    break;
                case 6: if (View_Methodology_Population != null) View_Methodology_Population.Visibility = Visibility.Visible; break;
                case 7: if (View_Methodology_Sampling != null) View_Methodology_Sampling.Visibility = Visibility.Visible; break;
                case 8: if (View_Methodology_Data != null) View_Methodology_Data.Visibility = Visibility.Visible; break;
                case 9: 
                    if (View_Budget != null) View_Budget.Visibility = Visibility.Visible; 
                    UpdateBudgetView();
                    break;
                case 10: if (View_Ethics != null) View_Ethics.Visibility = Visibility.Visible; break;
                case 11: if (View_DataAnalysis != null) View_DataAnalysis.Visibility = Visibility.Visible; break;
                case 12: if (View_Results != null) View_Results.Visibility = Visibility.Visible; break;
                case 13: if (View_Conclusion != null) View_Conclusion.Visibility = Visibility.Visible; break;
                case 14: if (View_References != null) View_References.Visibility = Visibility.Visible; break;
            }

            // Apply Permissions
            bool canEdit = CanUserEditStep(_currentStep);
            SetProtocolReadOnly(!canEdit);
>>>>>>> origin/main
        }

        private void NextSection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep < TotalSteps)
            {
                _currentStep++;
                UpdateView();
            }
        }

        private void PrevSection_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1)
            {
                _currentStep--;
                UpdateView();
            }
        }

        private void CreateProtocol_Click(object sender, RoutedEventArgs e)
        {
            ValidationMessagesListBox.Items.Clear();
            var protocol = BuildProtocolFromUI();
            var validator = new AdRev.Core.Protocols.ProtocolValidator();
            var validationResult = validator.Validate(protocol);

            if (validationResult.Score == 100)
            {
                 ValidationMessagesListBox.Items.Add($"🌟 EXCELLENT TRAVAIL ! Score Qualité : 100/100");
                 ValidationMessagesListBox.Items.Add("Le protocole respecte tous les critères de qualité.");
            }
            else
            {
                ValidationMessagesListBox.Items.Add($"📊 Score Qualité : {validationResult.Score}/100");
                foreach (var err in validationResult.Errors) ValidationMessagesListBox.Items.Add(err);
                foreach (var sug in validationResult.Suggestions) ValidationMessagesListBox.Items.Add(sug);
            }

            if (validationResult.Errors.Count == 0)
            {
                 string pName = _project != null ? _project.Title : ("Projet_" + DateTime.Now.ToString("yyyyMMdd"));
                 _service.Create(protocol, pName);
                 if (_project != null)
                 {
                     _project.GeneralObjective = protocol.GeneralObjective;
                     _project.SpecificObjectives = protocol.SpecificObjectives;
                     _project.DataAnalysisPlan = protocol.DataAnalysis;
                 }
                 ValidationMessagesListBox.Items.Add($"💾 Protocole enregistré avec succès (ID: {protocol.Id})");
            }
            else
            {
                 ValidationMessagesListBox.Items.Add("🛑 Veuillez corriger les erreurs critiques (❌) avant l'enregistrement.");
            }
        }

        private void ExportWord_Click(object sender, RoutedEventArgs e)
        {
            var protocol = BuildProtocolFromUI();
            var dialog = new SaveFileDialog { Filter = "Word Document|*.docx", FileName = $"Protocole_{_project?.Title ?? "Nouveau"}.docx" };
            if (dialog.ShowDialog() == true)
            {
                _wordExportService.ExportProtocolToWord(protocol, dialog.FileName);
                MessageBox.Show("Exportation terminée !");
            }
        }

        private void MenuFileNew_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment réinitialiser le protocole ?", "Nouveau", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                TitleTextBox.Clear();
                ContextTextBox.Clear();
                ProblemTextBox.Clear();
                ResearchQuestionTextBox.Clear();
                HypothesesTextBox.Clear();
                GeneralObjectiveTextBox.Clear();
                SpecificObjectivesTextBox.Clear();
                ConceptsTextBox.Clear();
                ConceptualModelTextBox.Clear();
                PopulationTextBox.Clear();
                InclusionTextBox.Clear();
                ExclusionTextBox.Clear();
                SamplingTextBox.Clear();
                DataCollectionTextBox.Clear();
                DataAnalysisTextBox.Clear();
                BudgetTextBox.Clear();
                EthicsTextBox.Clear();
                ExpectedResultsTextBox.Clear();
                ConclusionTextBox.Clear();
                ReferencesTextBox.Clear();
                _citations.Clear();
                _tempCoAuthors.Clear();
                _tempVariables.Clear();
                UpdateView();
            }
        }

        private void MenuDiscussion_Click(object sender, RoutedEventArgs e)
        {
             var type = StudyTypeComboBox.SelectedItem is StudyType st ? st : StudyType.Quantitative;
             var obj = SpecificObjectivesTextBox.Text;
             var style = RefStyleComboStep1.SelectedItem as ReferenceStyle? ?? ReferenceStyle.Vancouver;
             int nextIndex = _citations.Count + 1;
             
             var win = new DiscussionWindow(type, obj, _discussionPlan, _limitations, nextIndex, style);
             if (win.ShowDialog() == true)
             {
                 _discussionPlan = win.DiscussionPlan;
                 _limitations = win.Limitations;
                 if (win.NewCitations.Any())
                 {
                     _citations.AddRange(win.NewCitations);
                     RegenerateReferencesList();
                 }
                 ValidationMessagesListBox.Items.Add("✅ Discussion mise à jour !");
             }
        }
        
        private void UpdateQualityGuidelines(StudyType studyType)
        {
            if (PanelQualityGuidelines == null) return;
            
            var currentProtocol = BuildProtocolFromUI();

            var dummyProject = new AdRev.Domain.Models.ResearchProject 
            { 
                StudyType = studyType,
                Domain = DomainComboBox.SelectedItem != null ? (ScientificDomain)DomainComboBox.SelectedItem : ScientificDomain.Biomedical
            };
            var checklists = _qualityService.GetRecommendedChecklists(dummyProject);

            if (checklists == null || checklists.Count == 0)
            {
                PanelQualityGuidelines.Visibility = Visibility.Collapsed;
                return;
            }

            var primaryChecklist = checklists.First();
            _qualityService.EvaluateProtocol(primaryChecklist, currentProtocol);

            QualityStandardName.Text = $"({primaryChecklist.Name})";
            QualityGuidelinesList.Children.Clear();

            foreach (var section in primaryChecklist.Sections)
            {
                var txtSection = new TextBlock 
                { 
                    Text = section.Name, 
                    FontWeight = FontWeights.Bold, 
                    Margin = new Thickness(0, 10, 0, 5),
                    Foreground = (Brush)Application.Current.Resources["PrimaryBrush"]
                };
                QualityGuidelinesList.Children.Add(txtSection);

                foreach (var item in section.Items)
                {
                    var cb = new CheckBox 
                    { 
                        Content = new TextBlock { Text = item.Requirement, TextWrapping = TextWrapping.Wrap },
                        Margin = new Thickness(10, 0, 0, 5),
                        IsChecked = item.IsMet,
                        ToolTip = item.IsMet ? "Critère automatiquement validé par le contenu saisi" : "Ce critère ne semble pas encore rempli"
                    };
                    QualityGuidelinesList.Children.Add(cb);
                }
            }

            PanelQualityGuidelines.Visibility = Visibility.Visible;
        }

        private void UpdateBudgetView()
        {
            if (BudgetSuggestionType == null || BudgetSuggestionsText == null) return;

            var studyType = (StudyType)StudyTypeComboBox.SelectedItem;
            string suggestions = "";
            string typeDesc = "";

            if (studyType == StudyType.Qualitative)
            {
                typeDesc = "Étude Qualitative";
                suggestions = "• Équipement d'enregistrement (Dictaphones, Caméras)\n" +
                              "• Frais de déplacement pour les entretiens (Focus Groups, IDI)\n" +
                              "• Coûts de transcription et traduction\n" +
                              "• Compensation des participants (collation, transport)\n" +
                              "• Logiciel d'analyse (NVivo, Atlas.ti)...";
            }
            else if (studyType == StudyType.Quantitative)
            {
                typeDesc = "Étude Quantitative";
                suggestions = "• Impression des questionnaires (papier) ou Tablettes (ODK/Kobo)\n" +
                              "• Formation et Per diem des enquêteurs\n" +
                              "• Superviseurs de terrain\n" +
                              "• Transport et Logistique de terrain\n" +
                              "• Saisie et Nettoyage des données (Data Clerk)...";
            }
            else 
            {
                typeDesc = "MÉTHODE MIXTE";
                suggestions = "• Combinaison des coûts Quantitatifs et Qualitatifs\n" +
                              "• Réunions de coordination pour l'intégration des données...";
            }

            BudgetSuggestionType.Text = $"({typeDesc})";
            BudgetSuggestionsText.Text = suggestions;
        }

        private bool CanUserEditStep(int step)
        {
            if (_currentAccessLevel == UserAccessLevel.Admin) return true;
            if (_currentAccessLevel == UserAccessLevel.Viewer) return false;

            switch (_currentRole)
            {
                case FunctionalRole.PrincipalInvestigator:
                    return true;
                case FunctionalRole.Methodologist:
                    return (step != 9);
                case FunctionalRole.Statistician:
                    return (step == 7 || step == 8 || step == 11 || step == 12);
                case FunctionalRole.DataManager:
                    return (step == 1 || step == 8 || step == 9 || step == 11);
                case FunctionalRole.CoInvestigator:
                    return (step != 9 && step != 10);
                case FunctionalRole.Student:
                    return (step <= 6 || step == 12 || step == 13);
                default:
                    return false;
            }
        }

        private void SetProtocolReadOnly(bool isReadOnly)
        {
            foreach (var tb in FindVisualChildren<TextBox>(this))
            {
                tb.IsReadOnly = isReadOnly;
                tb.Opacity = isReadOnly ? 0.8 : 1.0;
            }

            foreach (var cb in FindVisualChildren<ComboBox>(this))
            {
                cb.IsEnabled = !isReadOnly;
                cb.Opacity = isReadOnly ? 0.8 : 1.0;
            }

            foreach (var btn in FindVisualChildren<Button>(this))
            {
                if (btn.Content is string s && (s.Contains("➕") || s.Contains("🗑️") || s.Contains("Calculer") || s.Contains("Générer")))
                {
                    btn.IsEnabled = !isReadOnly;
                }
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null)
                    {
                        if (child is T t) yield return t;
                        foreach (T childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
                    }
                }
            }
        }

        private void RefStyleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSyncingStyle) return;
            
            if (sender is ComboBox cb && cb.SelectedItem is ReferenceStyle style)
            {
                _isSyncingStyle = true;
                if (RefStyleComboStep1 != null && RefStyleComboStep1 != cb) RefStyleComboStep1.SelectedItem = style;
                if (RefStyleComboStep14 != null && RefStyleComboStep14 != cb) RefStyleComboStep14.SelectedItem = style;
                
                UpdateTextCitations(_lastStyle, style);
                _lastStyle = style;
                _isSyncingStyle = false;
                RegenerateReferencesList();
            }
        }

        // --- Objectives Logic ---

        private void ObjectiveTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as TextBox;
                if (textBox == null) return;

                string? prefixType = textBox.Tag as string;
                if (string.IsNullOrEmpty(prefixType)) return;

                int caretIndex = textBox.CaretIndex;
                int lineIndex = textBox.GetLineIndexFromCharacterIndex(caretIndex);
                string currentLineText = textBox.GetLineText(lineIndex).Trim();

                string nextPrefix = "";

                if (prefixType == "OS")
                {
                    var match = Regex.Match(currentLineText, @"OS(\d+)\.(\d+)");
                    if (match.Success)
                    {
                        int group = int.Parse(match.Groups[1].Value);
                        int sub = int.Parse(match.Groups[2].Value);
                        nextPrefix = $"OS{group}.{sub + 1}: ";
                    }
                    else
                    {
                        for (int i = lineIndex - 1; i >= 0; i--)
                        {
                            string l = textBox.GetLineText(i).Trim();
                            var mUp = Regex.Match(l, @"OS(\d+)\.(\d+)");
                            if (mUp.Success)
                            {
                                int g = int.Parse(mUp.Groups[1].Value);
                                int s = int.Parse(mUp.Groups[2].Value);
                                nextPrefix = $"OS{g}.{s + 1}: ";
                                break;
                            }
                            var mHead = Regex.Match(l, @"Pour.*(?:OG|Objectif)\s*(\d+)");
                            if (mHead.Success)
                            {
                                int g = int.Parse(mHead.Groups[1].Value);
                                nextPrefix = $"OS{g}.1: ";
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(nextPrefix))
                    {
                         int count = textBox.Text.Split('\n').Count(l => l.Trim().StartsWith("OS"));
                         nextPrefix = $"OS{count + 1}: "; 
                    }
                }
                else
                {
                    int count = 0;
                    var lines = textBox.Text.Split('\n');
                    foreach(var line in lines) if (line.Trim().StartsWith("OG")) count++;
                    nextPrefix = $"OG{count + 1}: ";
                }

                e.Handled = true;
                string insertion = $"\n{nextPrefix}";
                int insPoint = textBox.CaretIndex;
                textBox.Text = textBox.Text.Insert(insPoint, insertion);
                textBox.CaretIndex = insPoint + insertion.Length;
            }
        }

        private void GeneralObjectiveTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (OGCountAlert == null || OGBloomAlert == null) return;
            
            var text = GeneralObjectiveTextBox.Text;
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int ogCount = lines.Count(l => l.Trim().StartsWith("OG"));

            OGCountAlert.Visibility = ogCount > 3 ? Visibility.Visible : Visibility.Collapsed;

            bool anyInvalid = false;
            foreach(var line in lines)
            {
                if (line.Trim().StartsWith("OG") && !CheckBloomVerb(line, "OG"))
                {
                    anyInvalid = true;
                    break;
                }
            }
            OGBloomAlert.Visibility = anyInvalid ? Visibility.Visible : Visibility.Collapsed;
            SuggestOSVerbs(text);
        }

        private void SpecificObjectivesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
             if (OSCountAlert == null || OSBloomAlert == null) return;
            
            var text = SpecificObjectivesTextBox.Text;
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            var groups = lines.Where(l => l.Trim().StartsWith("OS"))
                               .GroupBy(l => l.Trim().Split('.')[0])
                               .ToDictionary(g => g.Key, g => g.Count());
            
            bool tooMany = false;
            string alertMsg = "";
            foreach(var kvp in groups)
            {
                if (kvp.Value > 4)
                {
                    tooMany = true;
                    alertMsg = $"⚠️ Trop d'OS pour {kvp.Key} (Max 4). Actuel: {kvp.Value}";
                    break; 
                }
            }

            if (tooMany)
            {
                OSCountAlert.Text = alertMsg;
                OSCountAlert.Visibility = Visibility.Visible;
            }
            else
            {
                int totalOS = lines.Count(l => l.Trim().StartsWith("OS"));
                int ogCount = GeneralObjectiveTextBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Count(l => l.Trim().StartsWith("OG"));
                if (ogCount == 0) ogCount = 1;
                
                if (totalOS > ogCount * 4) 
                {
                     OSCountAlert.Text = "⚠️ Attention : Titre global > 4 OS par OG.";
                     OSCountAlert.Visibility = Visibility.Visible;
                }
                else
                {
                    OSCountAlert.Visibility = Visibility.Collapsed;
                }
            }

            bool anyInvalid = false;
            foreach(var line in lines)
            {
                if (line.Trim().StartsWith("OS") && !CheckBloomVerb(line, "OS"))
                {
                    anyInvalid = true;
                    break;
                }
            }
            OSBloomAlert.Visibility = anyInvalid ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RenumberOS_Click(object sender, RoutedEventArgs e)
        {
            var text = SpecificObjectivesTextBox.Text;
            if (string.IsNullOrWhiteSpace(text)) return;

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            int currentGroup = 1;
            int currentSub = 1;

            foreach(var line in lines)
            {
                string l = line.Trim();

                if (l.StartsWith("---") || l.Contains("OG"))
                {
                    var m = Regex.Match(l, @"(\d+)");
                    if (m.Success) currentGroup = int.Parse(m.Value);
                    
                    currentSub = 1;
                    sb.AppendLine(l);
                    continue;
                }

                if (l.StartsWith("OS"))
                {
                    var mContent = Regex.Match(l, @"^OS[\d\.]+[:\s]*(.*)");
                    string content = mContent.Success ? mContent.Groups[1].Value : l;
                    if (string.IsNullOrWhiteSpace(content)) continue;

                    sb.AppendLine($"OS{currentGroup}.{currentSub}: {content}");
                    currentSub++;
                }
                else
                {
                     sb.AppendLine($"OS{currentGroup}.{currentSub}: {l}");
                     currentSub++;
                }
            }
            SpecificObjectivesTextBox.Text = sb.ToString();
        }
        
        private void LinkOSToOG_Click(object sender, RoutedEventArgs e)
        {
             var ogText = GeneralObjectiveTextBox.Text;
             var ogLines = ogText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(l => l.Trim().StartsWith("OG"))
                                 .ToList();
             
             if (ogLines.Count == 0)
             {
                 MessageBox.Show("Veuillez d'abord définir au moins un Objectif Général (OG1...).");
                 return;
             }
             
             var sb = new StringBuilder();
             if (SpecificObjectivesTextBox.Text.Length < 10)
             {
                 int i = 1;
                 foreach(var og in ogLines)
                 {
                     sb.AppendLine($"--- Pour {og.Trim().Split(':')[0]} ---");
                     sb.AppendLine($"OS{i}.1: ");
                     sb.AppendLine($"OS{i}.2: ");
                     sb.AppendLine();
                     i++;
                 }
                 SpecificObjectivesTextBox.Text = sb.ToString();
             }
             else
             {
                 if (MessageBox.Show("Voulez-vous reformater la zone d'objectifs spécifiques ?", "Reformater", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                 {
                     int i = 1;
                     foreach(var og in ogLines)
                     {
                         sb.AppendLine($"--- Pour {og.Trim().Split(':')[0]} ---");
                         sb.AppendLine($"OS{i}.1: ");
                         sb.AppendLine($"OS{i}.2: ");
                         i++;
                     }
                     sb.AppendLine();
                     sb.AppendLine("--- Ancien contenu ---");
                     sb.AppendLine(SpecificObjectivesTextBox.Text);
                     SpecificObjectivesTextBox.Text = sb.ToString(); 
                 }
             }
        }

        private bool CheckBloomVerb(string line, string type)
        {
            int colonIndex = line.IndexOf(':');
            string content = colonIndex >= 0 ? line.Substring(colonIndex + 1).Trim() : line.Trim();
            if (string.IsNullOrWhiteSpace(content)) return true;

            var firstWord = content.Split(' ')[0].ToLower();
            var bloomGeneral = new[] { "évaluer", "analyser", "déterminer", "étudier", "comparer", "identifier", "comprendre", "explorer", "illustrer", "décrire" };
            var bloomSpecific = new[] { "calculer", "lister", "définir", "citer", "nommer", "classer", "estimer", "quantifier", "vérifier", "interpréter", "caractériser" };
            var allBloom = bloomGeneral.Concat(bloomSpecific).ToArray();
            return allBloom.Any(v => firstWord.StartsWith(v));
        }

        private void SuggestOSVerbs(string ogText)
        {
            if (OSSuggestions == null) return;
            
            string suggestions = "💡 Verbes suggérés : ";
            if (ogText.ToLower().Contains("comparer")) suggestions += "Décrire (groupes), Mesurer (différences), Tester (hypothèse)...";
            else if (ogText.ToLower().Contains("évaluer")) suggestions += "Estimer, Calculer, Identifier (facteurs)...";
            else if (ogText.ToLower().Contains("décrire")) suggestions += "Lister, Caractériser, Classifier...";
            else suggestions += "Quantifier, Vérifier, Déterminer...";

            OSSuggestions.Text = suggestions;
            OSSuggestions.Visibility = Visibility.Visible;
        }

        // --- Calculators Logic ---

        private void CalculatorSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelCochran == null || PanelComparison == null) return;
            
            PanelCochran.Visibility = Visibility.Collapsed;
            PanelComparison.Visibility = Visibility.Collapsed;

            if (CalculatorSelector.SelectedItem is ComboBoxItem item)
            {
                string? tag = item.Tag?.ToString();
                if (tag == "Cochran") PanelCochran.Visibility = Visibility.Visible;
                else if (tag == "Comparison") PanelComparison.Visibility = Visibility.Visible;
            }
        }

        private void CalculateCochran_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double p = double.Parse(Calc_Cochran_P.Text) / 100.0;
                double d = double.Parse(Calc_Cochran_D.Text) / 100.0;
                double N_pop = 0;
                if (!string.IsNullOrWhiteSpace(Calc_Cochran_N.Text)) double.TryParse(Calc_Cochran_N.Text, out N_pop);

                var selectedItem = (ComboBoxItem)Calc_Cochran_Z.SelectedItem;
                double z = double.Parse(selectedItem.Tag?.ToString() ?? "1.96", System.Globalization.CultureInfo.InvariantCulture);

                double n0 = (Math.Pow(z, 2) * p * (1 - p)) / Math.Pow(d, 2);
                double nFinal = n0;
                string formulaUsed = "Cochran (population infinie)";

                if (N_pop > 0)
                {
                    nFinal = n0 / (1 + ((n0 - 1) / N_pop));
                    formulaUsed = $"Schwartz (population finie N={N_pop})";
                }

                int nCeiling = (int)Math.Ceiling(nFinal);
                ResultCochran.Text = $"N requis = {nCeiling} sujets";
                SamplingTextBox.Text = $"La taille d'échantillon minimal a été calculée selon la formule de {formulaUsed}. " +
                                       $"Avec une prévalence attendue de {p*100}%, une marge d'erreur de {d*100}% et un niveau de confiance de 95% (Z={z}), " +
                                       $"le nombre de sujets requis est de {nCeiling}.";
            }
            catch { MessageBox.Show("Vérifiez vos valeurs numériques."); }
        }

        private void CalculateComparison_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double p1 = double.Parse(Calc_Comp_P1.Text) / 100.0;
                double p2 = double.Parse(Calc_Comp_P2.Text) / 100.0;
                var itemAlpha = (ComboBoxItem)Calc_Comp_Alpha.SelectedItem;
                double zAlpha = double.Parse(itemAlpha.Tag?.ToString() ?? "1.96", System.Globalization.CultureInfo.InvariantCulture); 
                var itemBeta = (ComboBoxItem)Calc_Comp_Power.SelectedItem;
                double zBeta = double.Parse(itemBeta.Tag?.ToString() ?? "1.28", System.Globalization.CultureInfo.InvariantCulture);

                bool isMatched = Chk_Matched.IsChecked == true;
                double nFinal = 0;

                if (!isMatched)
                {
                    double numerator = Math.Pow(zAlpha + zBeta, 2) * ( (p1*(1-p1)) + (p2*(1-p2)) );
                    double denominator = Math.Pow(p1 - p2, 2);
                    if (denominator == 0) return;
                    nFinal = Math.Ceiling(numerator / denominator);
                    ResultComparison.Text = $"N = {nFinal} par groupe (Total: {nFinal*2})";
                    SamplingTextBox.Text = $"Pour mettre en évidence une différence entre P1={Calc_Comp_P1.Text}% et P2={Calc_Comp_P2.Text}% " +
                                           $"avec une puissance de 80% et un risque alpha de 5%, la taille d'échantillon requise est de {nFinal} sujets par groupe.";
                }
                else
                {
                    double phi = 0.2; 
                    double term1 = p1 * (1 - p1);
                    double term2 = p2 * (1 - p2);
                    double termCorr = 2 * phi * Math.Sqrt(term1 * term2);
                    double numerator = Math.Pow(zAlpha + zBeta, 2) * (term1 + term2 - termCorr);
                    double denominator = Math.Pow(p1 - p2, 2);
                    if (denominator == 0) return;
                    nFinal = Math.Ceiling(numerator / denominator);
                    ResultComparison.Text = $"N = {nFinal} PAIRES";
                    SamplingTextBox.Text = $"S'agissant d'une étude appariée, avec P1={Calc_Comp_P1.Text}% et P2={Calc_Comp_P2.Text}%, " +
                                           $"il est nécessaire d'inclure {nFinal} PAIRES de sujets.";
                }
            }
            catch { MessageBox.Show("Erreur de calcul."); }
        }

        // --- Citation Management ---

        private void AddCitation_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var targetTag = btn?.Tag?.ToString();
            TextBox? targetBox = null;

            if (targetTag == "Context") targetBox = ContextTextBox;
            else if (targetTag == "ProblemTextBox") targetBox = ProblemTextBox;
            else if (targetTag == "StudySettingTextBox") targetBox = StudySettingTextBox;
            else if (targetTag == "ConceptualModelTextBox") targetBox = ConceptualModelTextBox;
            else if (targetTag == "InclusionTextBox") targetBox = InclusionTextBox;
            else if (targetTag == "SamplingTextBox") targetBox = SamplingTextBox;
            else if (targetTag == "DataCollectionTextBox") targetBox = DataCollectionTextBox;
            else if (targetTag == "EthicsTextBox") targetBox = EthicsTextBox;
            else if (targetTag == "DataAnalysisTextBox") targetBox = DataAnalysisTextBox;
            else if (targetTag == "ExpectedResultsTextBox") targetBox = ExpectedResultsTextBox;
            else if (targetTag == "ConclusionTextBox") targetBox = ConclusionTextBox;

            var style = RefStyleComboStep1.SelectedItem as ReferenceStyle? ?? ReferenceStyle.Vancouver;
            var win = new CitationEntryWindow();
            if (win.ShowDialog() == true && win.ResultCitations.Any())
            {
                foreach (var cit in win.ResultCitations)
                {
                    _citations.Add(cit);
                    
                    if (targetBox != null)
                    {
                        string marker = (style == ReferenceStyle.Vancouver) ? $"[{_citations.Count}]" : $"({cit.Authors}, {cit.Year})";
                        int caret = targetBox.CaretIndex;
                        targetBox.Text = targetBox.Text.Insert(caret, " " + marker);
                        targetBox.CaretIndex = caret + marker.Length + 1;
                    }
                }
                RegenerateReferencesList();
            }
        }

        private void RegenerateReferences_Click(object sender, RoutedEventArgs e) => RegenerateReferencesList();

        private void RegenerateReferencesList()
        {
            if (ReferencesTextBox == null) return;
            var style = RefStyleComboStep1.SelectedItem as ReferenceStyle? ?? ReferenceStyle.Vancouver;
            var sb = new StringBuilder();
            for (int i = 0; i < _citations.Count; i++)
            {
                var c = _citations[i];
                if (style == ReferenceStyle.Vancouver) sb.AppendLine($"{i + 1}. {c.Authors}. {c.Title}. {c.Journal}. {c.Year};{c.Volume}({c.Issue}):{c.Pages}.");
                else sb.AppendLine($"{c.Authors} ({c.Year}). {c.Title}. {c.Journal}, {c.Volume}({c.Issue}), {c.Pages}.");
            }
            ReferencesTextBox.Text = sb.ToString();
        }

        private void UpdateTextCitations(ReferenceStyle oldStyle, ReferenceStyle newStyle)
        {
            var boxes = new List<TextBox> { ContextTextBox, ProblemTextBox, ResearchQuestionTextBox, HypothesesTextBox, GeneralObjectiveTextBox, SpecificObjectivesTextBox, ConceptsTextBox, ConceptualModelTextBox, StudySettingTextBox, PopulationTextBox, StudyCentersTextBox, InclusionTextBox, ExclusionTextBox, SamplingTextBox, DataCollectionTextBox, DataAnalysisTextBox, BudgetTextBox, EthicsTextBox, ExpectedResultsTextBox, ConclusionTextBox };
            foreach (var box in boxes)
            {
                if (box == null || string.IsNullOrEmpty(box.Text)) continue;
                string text = box.Text;
                for (int i = 0; i < _citations.Count; i++)
                {
                    var cit = _citations[i];
                    string oldMarker = (oldStyle == ReferenceStyle.Vancouver) ? $"[{i + 1}]" : $"({cit.Authors}, {cit.Year})";
                    string newMarker = (newStyle == ReferenceStyle.Vancouver) ? $"[{i + 1}]" : $"({cit.Authors}, {cit.Year})";
                    if (text.Contains(oldMarker)) text = text.Replace(oldMarker, newMarker);
                }
                box.Text = text;
            }
            if (!string.IsNullOrEmpty(_discussionPlan)) _discussionPlan = UpdateStringCitations(_discussionPlan, oldStyle, newStyle);
            if (!string.IsNullOrEmpty(_limitations)) _limitations = UpdateStringCitations(_limitations, oldStyle, newStyle);
        }

        private string UpdateStringCitations(string text, ReferenceStyle oldStyle, ReferenceStyle newStyle)
        {
            if (string.IsNullOrEmpty(text)) return text;
            for (int i = 0; i < _citations.Count; i++)
            {
                var cit = _citations[i];
                string oldMarker = (oldStyle == ReferenceStyle.Vancouver) ? $"[{i + 1}]" : $"({cit.Authors}, {cit.Year})";
                string newMarker = (newStyle == ReferenceStyle.Vancouver) ? $"[{i + 1}]" : $"({cit.Authors}, {cit.Year})";
                if (text.Contains(oldMarker)) text = text.Replace(oldMarker, newMarker);
            }
            return text;
        }

        // --- Protocol Building ---

        private ResearchProtocol BuildProtocolFromUI()
        {
            return new ResearchProtocol
            {
                Id = Guid.NewGuid().ToString(),
                ProjectId = _project?.Id,
                Title = TitleTextBox?.Text ?? "",
                StudyType = StudyTypeComboBox?.SelectedItem is StudyType st ? st : StudyType.Quantitative,
<<<<<<< HEAD
                QualitativeApproach = QualitativeApproachComboBox?.SelectedItem is QualitativeApproach qa ? qa : QualitativeApproach.Inductive,
                EpidemiologyType = EpidemiologyTypeComboBox?.SelectedItem is EpidemiologicalStudyType et ? et : EpidemiologicalStudyType.CrossSectionalDescriptive,
                Domain = DomainComboBox?.SelectedItem is ScientificDomain sd ? sd : ScientificDomain.Biomedical,
                PrincipalAuthor = new Author { FirstName = AuthorFirstNameBox?.Text ?? "", LastName = AuthorLastNameBox?.Text ?? "", Institution = AuthorInstitutionBox?.Text ?? "", Email = AuthorEmailBox?.Text ?? "" },
                CoAuthors = _tempCoAuthors.ToList<AdRev.Domain.Models.Author>(),
=======
                Domain = DomainComboBox?.SelectedItem is ScientificDomain sd ? sd : ScientificDomain.Biomedical,
                PrincipalAuthor = new Author { FirstName = AuthorFirstNameBox?.Text ?? "", LastName = AuthorLastNameBox?.Text ?? "", Institution = AuthorInstitutionBox?.Text ?? "", Email = AuthorEmailBox?.Text ?? "" },
>>>>>>> origin/main
                Context = ContextTextBox?.Text ?? "",
                ProblemJustification = ProblemTextBox?.Text ?? "",
                ResearchQuestion = ResearchQuestionTextBox?.Text ?? "",
                Hypotheses = HypothesesTextBox?.Text ?? "",
                GeneralObjective = GeneralObjectiveTextBox?.Text ?? "",
                SpecificObjectives = SpecificObjectivesTextBox?.Text ?? "",
                ConceptDefinitions = ConceptsTextBox?.Text ?? "",
                StudySetting = StudySettingTextBox?.Text ?? "",
                ConceptualModel = ConceptualModelTextBox?.Text ?? "",
                IsMulticentric = IsMulticentricCheckBox?.IsChecked == true,
                StudyCenters = StudyCentersTextBox?.Text ?? "",
                StudyPopulation = PopulationTextBox?.Text ?? "",
                InclusionCriteria = InclusionTextBox?.Text ?? "",
                ExclusionCriteria = ExclusionTextBox?.Text ?? "",
                SamplingType = SamplingTypeComboBox?.SelectedItem is SamplingType sm ? sm : SamplingType.None,
                IsStratified = IsStratifiedCheckBox?.IsChecked == true,
                StratificationCriteria = StratificationCriteriaTextBox?.Text ?? "",
                IsClusterSampling = IsClusterCheckBox?.IsChecked == true,
                ClusterSize = int.TryParse(ClusterSizeTextBox?.Text, out int cs) ? cs : 0,
                DesignEffect = double.TryParse(DesignEffectTextBox?.Text, out double de) ? de : 1.0,
                ExpectedLossRate = double.TryParse(ExpectedLossRateTextBox?.Text, out double lr) ? lr : 0.0,
                SamplingMethod = SamplingTextBox?.Text ?? "",
                DataCollection = DataCollectionTextBox?.Text ?? "",
                DataAnalysis = DataAnalysisTextBox?.Text ?? "",
                DiscussionPlan = _discussionPlan,
                StudyLimitations = _limitations,
                Budget = BudgetTextBox?.Text ?? "",
                Ethics = EthicsTextBox?.Text ?? "",
                ExpectedResults = ExpectedResultsTextBox?.Text ?? "",
                Conclusion = ConclusionTextBox?.Text ?? "",
                References = ReferencesTextBox?.Text ?? "",
                ReferenceStyle = RefStyleComboStep1?.SelectedItem is ReferenceStyle rs ? rs : ReferenceStyle.Vancouver,
                Citations = _citations
            };
        }

        // --- SAP/TAP Generation ---

        private void GenerateProtocolPlan_Click(object sender, RoutedEventArgs e)
        {
            string specObjs = SpecificObjectivesTextBox.Text;
            if (string.IsNullOrWhiteSpace(specObjs)) { MessageBox.Show("Définissez d'abord vos Objectifs Spécifiques (Étape 3)."); return; }
            var studyType = (StudyType)StudyTypeComboBox.SelectedItem;
            var sb = new StringBuilder();
            if (studyType == StudyType.Qualitative) GenerateThematicAnalysisPlan(sb, specObjs);
            else GenerateStatisticalAnalysisPlan(sb, specObjs);
            DataAnalysisTextBox.Text = sb.ToString();
            MessageBox.Show("Plan d'analyse généré !");
        }

        private void GenerateThematicAnalysisPlan(StringBuilder sb, string specObjs)
        {
            sb.AppendLine("PLAN D'ANALYSE THÉMATIQUE (TAP) - Cadre de Braun & Clarke");
            sb.AppendLine("==============================================");
            sb.AppendLine("1. Familiarisation; 2. Codage; 3. Recherche de thèmes; 4. Revue; 5. Définition; 6. Rapport.");
            sb.AppendLine("\nAnalyses spécifiques :");
            int idx = 1;
            foreach (var line in specObjs.Split('\n')) 
                if (!string.IsNullOrWhiteSpace(line) && line.Contains(":")) sb.AppendLine($"{idx++}. Pour : {line.Trim()}\n   -> Exploration thématique réflexive.");
        }

        private void GenerateStatisticalAnalysisPlan(StringBuilder sb, string specObjs)
        {
            sb.AppendLine("PLAN D'ANALYSE STATISTIQUE (SAP)");
            sb.AppendLine("==================================");
            sb.AppendLine("1. ANALYSE DESCRIPTIVE : Fréquences, Moyennes/Médianes.");
            sb.AppendLine("2. ANALYSES INFÉRENTIELLES :");
            int idx = 1;
            foreach (var line in specObjs.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains(":")) continue;
                sb.Append($"2.{idx++} Pour : {line.Trim()}\n   -> Test suggéré : ");
                string l = line.ToLower();
                if (l.Contains("comparer")) sb.AppendLine("T-Test / ANOVA / Chi2.");
                else if (l.Contains("associer") || l.Contains("facteur")) sb.AppendLine("Régression / Chi2.");
                else sb.AppendLine("Estimation IC95%.");
            }
        }

        // --- Other Handlers ---

        private void OpenVariableDesigner_Click(object sender, RoutedEventArgs e)
        {
            var win = new VariableDesignerWindow(_tempVariables, _project?.Title ?? "Nouveau Projet");
<<<<<<< HEAD
            if (win.ShowDialog() == true)
            {
                var newVars = win.Variables.ToList();
                _tempVariables = newVars;
                if (_project != null) _project.Variables = newVars;
                
                // Diffuser la mise à jour à toutes les vues
                var parent = Window.GetWindow(this) as AdRev.Desktop.Windows.ProjectWindow;
                parent?.SyncVariablesToAllViews();
            }
=======
            if (win.ShowDialog() == true) _tempVariables = win.Variables.ToList();
>>>>>>> origin/main
        }

        private void StudyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QualitativeTypePanel == null || EpidemiologyTypePanel == null) return;
<<<<<<< HEAD
            if (StudyTypeComboBox.SelectedItem == null) return;

            var st = (StudyType)StudyTypeComboBox.SelectedItem;
            
            // Sync with current project
            if (_project != null) _project.StudyType = st;

            QualitativeTypePanel.Visibility = st == StudyType.Qualitative ? Visibility.Visible : Visibility.Collapsed;
            EpidemiologyTypePanel.Visibility = (st == StudyType.Quantitative) ? Visibility.Visible : Visibility.Collapsed;
            
            UpdateQualityGuidelines(st);
            UpdateSamplingTypes();

            // Notify parent window to show/hide qualitative tabs
            var parent = Window.GetWindow(this) as ProjectWindow;
            parent?.ApplyGating();
        }

        private void UpdateSamplingTypes()
        {
            if (SamplingTypeComboBox == null || StudyTypeComboBox == null || StudyTypeComboBox.SelectedItem == null) return;

            var currentStudyType = (StudyType)StudyTypeComboBox.SelectedItem;
            var allTypes = System.Enum.GetValues(typeof(SamplingType)).Cast<SamplingType>();

            List<SamplingType> filtered;
            if (currentStudyType == StudyType.Qualitative)
            {
                var approach = (QualitativeApproach)(QualitativeApproachComboBox?.SelectedItem ?? QualitativeApproach.Inductive);
                filtered = allTypes.Where(t => IsAllowedForApproach(t, approach)).ToList();
            }
            else
            {
                filtered = allTypes.Where(t => !IsQualitativeSampling(t) && t != SamplingType.None).ToList();
            }

            SamplingTypeComboBox.ItemsSource = filtered;
            
            // Populate Step 5 qualitative sampler as well
            if (QualitativeSamplingComboBox != null && currentStudyType == StudyType.Qualitative)
            {
                QualitativeSamplingComboBox.ItemsSource = filtered;
            }

            if (filtered.Any()) SamplingTypeComboBox.SelectedIndex = 0;
        }

        private bool IsAllowedForApproach(SamplingType t, QualitativeApproach approach)
        {
            // Base filters
            if (t == SamplingType.Saturation || t == SamplingType.Convenience) return true;

            switch (approach)
            {
                case QualitativeApproach.GroundedTheory:
                    return t == SamplingType.Theoretical || t == SamplingType.Saturation;
                
                case QualitativeApproach.CaseStudy:
                    return t.ToString().Contains("CaseStudy") || t == SamplingType.Purposeful;
                
                case QualitativeApproach.Phenomenological:
                    return t == SamplingType.Purposeful || t == SamplingType.PurposefulTypical || t == SamplingType.PurposefulExtreme || t == SamplingType.Saturation;
                
                case QualitativeApproach.Ethnography:
                    return t == SamplingType.Purposeful || t == SamplingType.Snowball || t == SamplingType.EventContextual || t == SamplingType.Convenience;
                
                case QualitativeApproach.Narrative:
                    return t == SamplingType.Purposeful || t == SamplingType.Snowball;
                
                case QualitativeApproach.DiscourseAnalysis:
                    return t == SamplingType.Purposeful || t == SamplingType.PurposefulCritical;
                
                case QualitativeApproach.ActionResearch:
                    return t == SamplingType.Purposeful || t == SamplingType.Saturation;
                
                case QualitativeApproach.Descriptive:
                    return t == SamplingType.Purposeful || t == SamplingType.Quota || t == SamplingType.Convenience || t == SamplingType.Saturation;
                
                default:
                    return IsQualitativeSampling(t);
            }
        }

        private void QualitativeApproachComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSamplingTypes();
            RefreshAudit_LostFocus(sender, e);
        }

        private bool IsQualitativeSampling(SamplingType t)
        {
            string s = t.ToString();
            return s.Contains("Purposeful") || 
                   s.Contains("CaseStudy") || 
                   s.Contains("Theoretical") || 
                   s.Contains("Snowball") || 
                   t == SamplingType.Saturation || 
                   t == SamplingType.EventContextual ||
                   t == SamplingType.Quota || // In this app context, Quota in Quali is common
                   t == SamplingType.Convenience;
=======
            var st = (StudyType)StudyTypeComboBox.SelectedItem;
            QualitativeTypePanel.Visibility = st == StudyType.Qualitative ? Visibility.Visible : Visibility.Collapsed;
            EpidemiologyTypePanel.Visibility = st == StudyType.Quantitative ? Visibility.Visible : Visibility.Collapsed;
            UpdateQualityGuidelines(st);
>>>>>>> origin/main
        }

        private void EpidemiologyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             UpdateQualityGuidelines((StudyType)StudyTypeComboBox.SelectedItem);
        }

        private void SamplingTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StratificationPanel == null || ClusterPanel == null) return;
<<<<<<< HEAD
            if (SamplingTypeComboBox.SelectedItem == null) return;

            var type = (SamplingType)SamplingTypeComboBox.SelectedItem;
            StratificationPanel.Visibility = (type == SamplingType.Stratified || type == SamplingType.StratifiedCluster) ? Visibility.Visible : Visibility.Collapsed;
            ClusterPanel.Visibility = (type == SamplingType.ClusterSampling || type == SamplingType.StratifiedCluster || type == SamplingType.MultiStage) ? Visibility.Visible : Visibility.Collapsed;
            
            // Sync with QualitativeSamplingComboBox if in Step 5
            if (QualitativeSamplingComboBox != null && (StudyType)StudyTypeComboBox.SelectedItem == StudyType.Qualitative)
            {
                if (QualitativeSamplingComboBox.SelectedItem != SamplingTypeComboBox.SelectedItem)
                {
                    QualitativeSamplingComboBox.SelectedItem = SamplingTypeComboBox.SelectedItem;
                }
            }
=======
            var type = (SamplingType)SamplingTypeComboBox.SelectedItem;
            StratificationPanel.Visibility = (type == SamplingType.Stratified || type == SamplingType.StratifiedCluster) ? Visibility.Visible : Visibility.Collapsed;
            ClusterPanel.Visibility = (type == SamplingType.ClusterSampling || type == SamplingType.StratifiedCluster || type == SamplingType.MultiStage) ? Visibility.Visible : Visibility.Collapsed;
>>>>>>> origin/main
        }

        private void IsMulticentricCheckBox_Checked(object sender, RoutedEventArgs e) { if (MulticentricDetailsPanel != null) MulticentricDetailsPanel.Visibility = Visibility.Visible; }
        private void IsMulticentricCheckBox_Unchecked(object sender, RoutedEventArgs e) { if (MulticentricDetailsPanel != null) MulticentricDetailsPanel.Visibility = Visibility.Collapsed; }
        private void IsStratifiedCheckBox_Checked(object sender, RoutedEventArgs e) { if (StratificationDetailsPanel != null) StratificationDetailsPanel.Visibility = Visibility.Visible; }
        private void IsStratifiedCheckBox_Unchecked(object sender, RoutedEventArgs e) { if (StratificationDetailsPanel != null) StratificationDetailsPanel.Visibility = Visibility.Collapsed; }
        private void IsClusterCheckBox_Checked(object sender, RoutedEventArgs e) { if (ClusterDetailsPanel != null) ClusterDetailsPanel.Visibility = Visibility.Visible; }
        private void IsClusterCheckBox_Unchecked(object sender, RoutedEventArgs e) { if (ClusterDetailsPanel != null) ClusterDetailsPanel.Visibility = Visibility.Collapsed; }
        private void RefreshAudit_LostFocus(object sender, RoutedEventArgs e) => UpdateQualityGuidelines((StudyType)StudyTypeComboBox.SelectedItem);
    }
}
