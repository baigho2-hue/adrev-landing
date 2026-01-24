using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using AdRev.Core.Protocols;
using AdRev.Domain.Enums;
using AdRev.Domain.Protocols;
using AdRev.Domain.Models;

using AdRev.Core.Services;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Win32;
using AdRev.Desktop.Services;

namespace AdRev.Desktop
{
    public partial class ProtocolWindow : Window
    {
        private readonly ProtocolService _service = new ProtocolService();
        private readonly QualityService _qualityService = new QualityService();
        private readonly WordExportService _wordExportService = new WordExportService();

        // Liste temporaire pour l'UI
        private System.Collections.ObjectModel.ObservableCollection<AdRev.Domain.Models.Author> _tempCoAuthors 
            = new System.Collections.ObjectModel.ObservableCollection<AdRev.Domain.Models.Author>();

        // Liste temporaire pour les variables (Masque de Saisie)
        private System.Collections.Generic.List<AdRev.Domain.Variables.StudyVariable> _tempVariables 
            = new System.Collections.Generic.List<AdRev.Domain.Variables.StudyVariable>();

        // Stockage tampon pour la discussion (venant de la fenêtre externe)
        private string _discussionPlan = string.Empty;
        private string _limitations = string.Empty;

        private AdRev.Domain.Models.ResearchProject? _project;
        private FunctionalRole _currentRole = FunctionalRole.PrincipalInvestigator;
        private UserAccessLevel _currentAccessLevel = UserAccessLevel.Admin;

        public ProtocolWindow() : this(new AdRev.Domain.Models.ResearchProject(), FunctionalRole.PrincipalInvestigator, UserAccessLevel.Admin) { }

        public ProtocolWindow(AdRev.Domain.Models.ResearchProject? project, FunctionalRole role = FunctionalRole.PrincipalInvestigator, UserAccessLevel access = UserAccessLevel.Admin)
        {
            InitializeComponent();
            _project = project;
            _currentRole = role;
            _currentAccessLevel = access;

            StudyTypeComboBox.ItemsSource = System.Enum.GetValues(typeof(StudyType));
            StudyTypeComboBox.SelectedIndex = 0;
            
            EpidemiologyTypeComboBox.ItemsSource = System.Enum.GetValues(typeof(EpidemiologicalStudyType));
            EpidemiologyTypeComboBox.SelectedIndex = 0;

            QualitativeApproachComboBox.ItemsSource = System.Enum.GetValues(typeof(QualitativeApproach));
            QualitativeApproachComboBox.SelectedIndex = 0;
            
            SamplingTypeComboBox.ItemsSource = System.Enum.GetValues(typeof(SamplingType));
            SamplingTypeComboBox.SelectedIndex = 0;
            UpdateSamplingTypes((StudyType)StudyTypeComboBox.SelectedItem);
            
            CoAuthorsListBox.ItemsSource = _tempCoAuthors;

            if (_project != null)
            {
                ProjectTitleTextBlock.Text = "Projet : " + _project.Title;
                TitleTextBox.Text = _project.Title;
                StudyTypeComboBox.SelectedItem = _project.StudyType;
                DomainComboBox.SelectedItem = _project.Domain;
                AuthorInstitutionBox.Text = _project.Institution;
                ProcessAuthors(_project.Authors);
            }
            
            RefStyleComboStep1.ItemsSource = System.Enum.GetValues(typeof(ReferenceStyle));
            RefStyleComboStep13.ItemsSource = System.Enum.GetValues(typeof(ReferenceStyle));
            
            // Default check
            RefStyleComboStep1.SelectedItem = ReferenceStyle.Vancouver;
            RefStyleComboStep13.SelectedItem = ReferenceStyle.Vancouver;

            DomainComboBox.ItemsSource = System.Enum.GetValues(typeof(ScientificDomain));
            DomainComboBox.SelectedIndex = 0;

            // Role selection for shared protocols simulation
            ActiveRoleComboBox.ItemsSource = System.Enum.GetValues(typeof(FunctionalRole));
            ActiveRoleComboBox.SelectedItem = _currentRole;

            ActiveAccessComboBox.ItemsSource = System.Enum.GetValues(typeof(UserAccessLevel));
            ActiveAccessComboBox.SelectedItem = _currentAccessLevel;

            // Only show role selection for shared projects (team projects)
            if (_project != null && (_project.Team != null && _project.Team.Any()))
            {
                SharingRolePanel.Visibility = Visibility.Visible;
            }
            else
            {
                SharingRolePanel.Visibility = Visibility.Collapsed;
            }

            ApplyPermissions();
        }

        private void ActiveRoleAndAccess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveRoleComboBox == null || ActiveAccessComboBox == null) return;
            
            if (ActiveRoleComboBox.SelectedItem is FunctionalRole role && 
                ActiveAccessComboBox.SelectedItem is UserAccessLevel access)
            {
                _currentRole = role;
                _currentAccessLevel = access;
                ApplyPermissions();
            }
        }

        private void ApplyPermissions()
        {
            // Reset all to Visible first (Collaboration principle: see more, edit less)
            for (int i = 0; i < SideMenuListBox.Items.Count; i++)
            {
                if (SideMenuListBox.Items[i] is ListBoxItem item) item.Visibility = Visibility.Visible;
            }
            MenuFileNew.Visibility = Visibility.Visible;
            MenuFileSave.Visibility = Visibility.Visible;
            MenuDiscussion.Visibility = Visibility.Visible;

            // Simple RBAC by disabling sidebar items ONLY if truly irrelevant
            for (int i = 0; i < SideMenuListBox.Items.Count; i++)
            {
                var item = SideMenuListBox.Items[i] as ListBoxItem;
                if (item == null) continue;

                var tag = item.Tag?.ToString();
                if (string.IsNullOrEmpty(tag)) continue;

                int step = int.Parse(tag);
                bool canSee = true;

                switch (_currentRole)
                {
                    case FunctionalRole.Student:
                        // Hide internal stuff like budget/ethics for students if needed
                        canSee = (step != 9); 
                        break;
                    case FunctionalRole.Monitor:
                        // ARC sees medical/data parts, maybe not budget
                        canSee = (step != 9);
                        break;
                }

                item.Visibility = canSee ? Visibility.Visible : Visibility.Collapsed;
                
                // Visual feedback: if can see but not edit, maybe add a lock icon simulation?
                // For now, we'll just handle editability in UpdateView.
            }

            // Menu RBAC
            if (_currentAccessLevel == UserAccessLevel.Viewer || _currentRole == FunctionalRole.Student || _currentRole == FunctionalRole.Monitor)
            {
                MenuFileSave.Visibility = Visibility.Collapsed;
            }
            
            if (_currentRole == FunctionalRole.DataManager || _currentAccessLevel == UserAccessLevel.Viewer)
            {
                MenuDiscussion.Visibility = Visibility.Collapsed;
            }

            // Refresh the current view to apply read-only status to the active step
            UpdateView();
        }

        private bool CanUserEditStep(int step)
        {
            if (_currentAccessLevel == UserAccessLevel.Admin) return true;
            if (_currentAccessLevel == UserAccessLevel.Viewer) return false;

            // Editor level - Role based permissions
            switch (_currentRole)
            {
                case FunctionalRole.PrincipalInvestigator:
                    return true;
                case FunctionalRole.Methodologist:
                    // Can edit almost everything except maybe budget
                    return (step != 9);
                case FunctionalRole.Statistician:
                    // Sampling (7), Collecte (8), Analysis (11), Results (12)
                    return (step == 7 || step == 8 || step == 11 || step == 12);
                case FunctionalRole.DataManager:
                    // Info (1), Collecte (8), Budget (9), Analysis (11)
                    return (step == 1 || step == 8 || step == 9 || step == 11);
                case FunctionalRole.CoInvestigator:
                    // Most parts except administrative/budget/ethics
                    return (step != 9 && step != 10);
                case FunctionalRole.Student:
                    // Intro, Objectives, Design, Results
                    return (step <= 6 || step == 12 || step == 13);
                default:
                    return false;
            }
        }

        private void SetProtocolReadOnly(bool isReadOnly)
        {
            // Traverse the UI and set textboxes/comboboxes
            // This is a simplified version, ideally use a property on ViewModel
            
            foreach (var tb in FindVisualChildren<TextBox>(this))
            {
                tb.IsReadOnly = isReadOnly;
                tb.Opacity = isReadOnly ? 0.8 : 1.0;
            }

            foreach (var cb in FindVisualChildren<ComboBox>(this))
            {
                // ComboBox doesn't have IsReadOnly for selection, use IsEnabled
                // Except for our simulation ones!
                if (cb.Name == "ActiveRoleComboBox" || cb.Name == "ActiveAccessComboBox") continue;
                cb.IsEnabled = !isReadOnly;
                cb.Opacity = isReadOnly ? 0.8 : 1.0;
            }

            foreach (var btn in FindVisualChildren<Button>(this))
            {
                // Disable action buttons (Add co-author, calculate, etc.)
                if (btn.Name == "Minimize_Click" || btn.Name == "Close_Click" || btn.IsEnabled == false) continue;
                // Don't disable navigation buttons if we had them (Previous/Next)
                // But specifically creation/edit buttons
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
                        if (child is T t)
                        {
                            yield return t;
                        }

                        foreach (T childOfChild in FindVisualChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }
// ...
// (Warning: Need to target specific chunks separately or use multi_replace. Using multi_replace logic with separate chunks in one call? No, replace_file_content is one block. I will do separate calls or use multi_replace.)

// I will use multi_replace for this to save steps.

        private void DomainComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DomainComboBox.SelectedItem is ScientificDomain domain)
            {
                if (domain != ScientificDomain.Biomedical)
                {
                    DesignPrecisionLabel.Text = "Type de Design Spécifique";
                }
                else
                {
                    DesignPrecisionLabel.Text = "Précision du Devis (Méthodologie)";
                }
                
                // Refresh Audit
                if (StudyTypeComboBox != null) UpdateQualityGuidelines((StudyType)StudyTypeComboBox.SelectedItem);
            }
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

        private void OpenVariableDesigner_Click(object sender, RoutedEventArgs e)
        {
            bool isQualitativeDeductive = false;
            
            if (StudyTypeComboBox.SelectedItem is StudyType st && (st == StudyType.Qualitative || st == StudyType.Mixed))
            {
                if (QualitativeApproachComboBox.SelectedItem is QualitativeApproach qa && qa == QualitativeApproach.Deductive)
                {
                    isQualitativeDeductive = true;
                }
            }

            var designer = new VariableDesignerWindow(_tempVariables, TitleTextBox.Text, isQualitativeDeductive);
            if (designer.ShowDialog() == true)
            {
                // Mise à jour de la liste avec les nouvelles données
                _tempVariables = new System.Collections.Generic.List<AdRev.Domain.Variables.StudyVariable>(designer.Variables);
                MessageBox.Show($"{_tempVariables.Count} variables définies avec succès.", "Variables Sauvegardées", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StudyTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (EpidemiologyTypePanel == null) return;

            var selected = (StudyType)StudyTypeComboBox.SelectedItem;
            
            // Reset to default (hidden)
            if (EpidemiologyTypePanel != null) EpidemiologyTypePanel.Visibility = Visibility.Collapsed;
            if (QualitativeTypePanel != null) QualitativeTypePanel.Visibility = Visibility.Collapsed;

            if (selected == StudyType.Quantitative)
            {
                if (EpidemiologyTypePanel != null) EpidemiologyTypePanel.Visibility = Visibility.Visible;
            }
            else if (selected == StudyType.Qualitative)
            {
                if (QualitativeTypePanel != null) QualitativeTypePanel.Visibility = Visibility.Visible;
                
                // Clear Quantitative inputs
                EpidemiologyTypeComboBox.SelectedIndex = 0; 
                if (PanelCochran != null) PanelCochran.Visibility = Visibility.Collapsed;
                if (PanelComparison != null) PanelComparison.Visibility = Visibility.Collapsed;
                if (CalculatorSelector != null) SelectCalculator("None");
            }
            else if (selected == StudyType.Mixed)
            {
                if (EpidemiologyTypePanel != null) EpidemiologyTypePanel.Visibility = Visibility.Visible;
                if (QualitativeTypePanel != null) QualitativeTypePanel.Visibility = Visibility.Visible;
            }

            UpdateSamplingTypes(selected);
            UpdateQualityGuidelines(selected);
        }

        // --- Logic Objectifs (Auto-Numbering & Validation) ---

        private void ObjectiveTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as System.Windows.Controls.TextBox;
                if (textBox == null) return;

                string? prefixType = textBox.Tag as string; // "OG" ou "OS"
                if (string.IsNullOrEmpty(prefixType)) return;

                // Grab current line to infer context
                int caretIndex = textBox.CaretIndex;
                int lineIndex = textBox.GetLineIndexFromCharacterIndex(caretIndex);
                string currentLineText = textBox.GetLineText(lineIndex).Trim();

                string nextPrefix = "";

                if (prefixType == "OS")
                {
                    // Advanced OS logic: Try to parse "OSx.y" from PREVIOUS line if current is empty?
                    // Or parse CURRENT line if user types "OS1.1: content[ENTER]"
                    
                    // Regex capture: OS(\d+)\.(\d+)
                    var match = System.Text.RegularExpressions.Regex.Match(currentLineText, @"OS(\d+)\.(\d+)");
                    if (match.Success)
                    {
                        int group = int.Parse(match.Groups[1].Value);
                        int sub = int.Parse(match.Groups[2].Value);
                        nextPrefix = $"OS{group}.{sub + 1}: ";
                    }
                    else
                    {
                        // Fallback: Check lines ABOVE for valid OSx.y or --- OGx ---
                        // This scan up helps if we just pressed enter on an empty line
                        for (int i = lineIndex - 1; i >= 0; i--)
                        {
                            string l = textBox.GetLineText(i).Trim();
                            var mUp = System.Text.RegularExpressions.Regex.Match(l, @"OS(\d+)\.(\d+)");
                            if (mUp.Success)
                            {
                                int g = int.Parse(mUp.Groups[1].Value);
                                int s = int.Parse(mUp.Groups[2].Value);
                                nextPrefix = $"OS{g}.{s + 1}: ";
                                break;
                            }
                            // Detect Header "--- Pour OG2"
                            // If we hit a header, restart at x.1
                            var mHead = System.Text.RegularExpressions.Regex.Match(l, @"Pour.*(?:OG|Objectif)\s*(\d+)");
                            if (mHead.Success)
                            {
                                int g = int.Parse(mHead.Groups[1].Value);
                                nextPrefix = $"OS{g}.1: ";
                                break;
                            }
                        }
                    }

                    // Worst case fallback
                    if (string.IsNullOrEmpty(nextPrefix))
                    {
                         // Count total lines starting with OS
                         int count = textBox.Text.Split('\n').Count(l => l.Trim().StartsWith("OS"));
                         nextPrefix = $"OS{count + 1}: "; 
                    }
                }
                else // OG
                {
                    // Standard linear logic for OG
                    int count = 0;
                    var lines = textBox.Text.Split('\n');
                    foreach(var line in lines) if (line.Trim().StartsWith("OG")) count++;
                    nextPrefix = $"OG{count + 1}: ";
                }

                e.Handled = true;
                string insertion = $"\n{nextPrefix}";
                
                // Safe insertion at caret
                int insPoint = textBox.CaretIndex;
                textBox.Text = textBox.Text.Insert(insPoint, insertion);
                textBox.CaretIndex = insPoint + insertion.Length;
            }
        }

        private void GeneralObjectiveTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (OGCountAlert == null || OGBloomAlert == null) return;
            
            var text = GeneralObjectiveTextBox.Text;
            var lines = text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            int ogCount = lines.Count(l => l.Trim().StartsWith("OG"));

            // 1. Validation nombre
            OGCountAlert.Visibility = ogCount > 3 ? Visibility.Visible : Visibility.Collapsed;

            // 2. Validation Bloom
            // Simple check: check if any line starts with a Bloom verb (after prefix)
            bool validBloom = lines.Length == 0 || lines.Any(l => CheckBloomVerb(l, "OG"));
            // For OGs, we want ALL to be valid generally, or at least one. Let's start with loose "at least one valid"
            // Or stricter: "Any line starting with OG must have a bloom verb"
            
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
            
            // Suggest verbs for OS based on OG (if any valid OG found)
            SuggestOSVerbs(text);
        }

        private void SpecificObjectivesTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
             if (OSCountAlert == null || OSBloomAlert == null) return;
            
            var text = SpecificObjectivesTextBox.Text;
            var lines = text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            // 1. Validation structurée (Max 4 par OG)
            // On regroupe par "OSx"
            var groups = lines.Where(l => l.Trim().StartsWith("OS"))
                              .GroupBy(l => l.Trim().Split('.')[0]) // Groupe par "OS1", "OS2"...
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
                // check global ratio just in case structure isn't used
                int totalOS = lines.Count(l => l.Trim().StartsWith("OS"));
                int ogCount = GeneralObjectiveTextBox.Text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries).Count(l => l.Trim().StartsWith("OG"));
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

             // 2. Bloom
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
            var sb = new System.Text.StringBuilder();

            int currentGroup = 1; // Default
            int currentSub = 1;

            foreach(var line in lines)
            {
                string l = line.Trim();

                // Detect Separator/Header (set via LinkOSToOG or manual)
                if (l.StartsWith("---") || l.Contains("OG"))
                {
                    // Attempt to parse group ID from header (e.g. "Pour OG2")
                    var m = System.Text.RegularExpressions.Regex.Match(l, @"(\d+)");
                    if (m.Success) currentGroup = int.Parse(m.Value);
                    
                    currentSub = 1; // Reset sub index
                    sb.AppendLine(l); // Keep header
                    continue;
                }

                // Detect OS Content
                if (l.StartsWith("OS"))
                {
                    // Strip old prefix (OS x.y : content)
                    // Regex: ^OS\d+(\.\d+)?:?\s*(.*)
                    var mContent = System.Text.RegularExpressions.Regex.Match(l, @"^OS[\d\.]+[:\s]*(.*)");
                    string content = mContent.Success ? mContent.Groups[1].Value : l;
                    
                    if (string.IsNullOrWhiteSpace(content)) continue; // skip empty bullets

                    sb.AppendLine($"OS{currentGroup}.{currentSub}: {content}");
                    currentSub++;
                }
                else
                {
                    // Other text or pure content logic? 
                    // Assume it belongs to previous bullet or is a new one?
                    // Treat as new bullet for now to be safe
                     sb.AppendLine($"OS{currentGroup}.{currentSub}: {l}");
                     currentSub++;
                }
            }
            
            SpecificObjectivesTextBox.Text = sb.ToString();
        }
        
        private void LinkOSToOG_Click(object sender, RoutedEventArgs e)
        {
             // Helper: Read OGs and scaffold OS structure
             var ogText = GeneralObjectiveTextBox.Text;
             var ogLines = ogText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(l => l.Trim().StartsWith("OG"))
                                 .ToList();
             
             if (ogLines.Count == 0)
             {
                 MessageBox.Show("Veuillez d'abord définir au moins un Objectif Général (OG1...).");
                 return;
             }
             
             var sb = new System.Text.StringBuilder();
             // Preserve existing content if it looks structured, otherwise clear? 
             // Let's just append or replace if empty/default
             
             if (SpecificObjectivesTextBox.Text.Length < 10) // "OS1: " is default
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
                 if (MessageBox.Show("Voulez-vous reformater la zone d'objectifs spécifiques ? Cela ajoutera une structure basée sur vos OG.", "Reformater", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                 {
                     int i = 1;
                     foreach(var og in ogLines)
                     {
                         sb.AppendLine($"--- Pour {og.Trim().Split(':')[0]} ---");
                         sb.AppendLine($"OS{i}.1: ");
                         sb.AppendLine($"OS{i}.2: ");
                         i++;
                     }
                     // Append old text at bottom for reference?
                     sb.AppendLine();
                     sb.AppendLine("--- Ancien contenu ---");
                     sb.AppendLine(SpecificObjectivesTextBox.Text);
                     SpecificObjectivesTextBox.Text = sb.ToString(); 
                 }
             }
        }


        private bool CheckBloomVerb(string line, string type)
        {
            // Remove prefix "OG1: "
            int colonIndex = line.IndexOf(':');
            string content = colonIndex >= 0 ? line.Substring(colonIndex + 1).Trim() : line.Trim();
            
            if (string.IsNullOrWhiteSpace(content)) return true; // Ignore empty

            var firstWord = content.Split(' ')[0].ToLower();
            
            // Bloom Lists (Simplified & Expanded for Quali)
            var bloomGeneral = new[] { "évaluer", "analyser", "déterminer", "étudier", "comparer", "identifier", "comprendre", "explorer", "illustrer", "décrire" };
            var bloomSpecific = new[] { "calculer", "lister", "définir", "citer", "nommer", "classer", "estimer", "quantifier", "vérifier", "interpréter", "caractériser" };
            
            // Merge for broad check, or specific
            var allBloom = bloomGeneral.Concat(bloomSpecific).ToArray();
            
            // Sophisticated check could stem the word, here strict match or startsWith
            return allBloom.Any(v => firstWord.StartsWith(v));
        }

        private void SuggestOSVerbs(string ogText)
        {
            if (OSSuggestions == null) return;
            
            string suggestions = "💡 Verbes suggérés : ";
            
            if (ogText.ToLower().Contains("comparer"))
                suggestions += "Décrire (groupes), Mesurer (différences), Tester (hypothèse)...";
            else if (ogText.ToLower().Contains("évaluer"))
                suggestions += "Estimer, Calculer, Identifier (facteurs)...";
            else if (ogText.ToLower().Contains("décrire"))
                suggestions += "Lister, Caractériser, Classifier...";
            else
                suggestions += "Quantifier, Vérifier, Déterminer...";

            OSSuggestions.Text = suggestions;
            OSSuggestions.Visibility = Visibility.Visible;
        }

        private void UpdateQualityGuidelines(StudyType studyType)
        {
            if (PanelQualityGuidelines == null) return;
            
            // Reconstruit le protocole actuel pour l'évaluation
            var currentProtocol = BuildProtocolFromUI();

            // Temporary project object to leverage the service selection logic
            var dummyProject = new AdRev.Domain.Models.ResearchProject 
            { 
                StudyType = studyType,
                Domain = DomainComboBox.SelectedItem != null ? (ScientificDomain)DomainComboBox.SelectedItem : ScientificDomain.Biomedical,
                EpidemiologyType = EpidemiologyTypeComboBox.SelectedItem != null ? (EpidemiologicalStudyType)EpidemiologyTypeComboBox.SelectedItem : EpidemiologicalStudyType.CrossSectionalDescriptive
            };
            var checklists = _qualityService.GetRecommendedChecklists(dummyProject);

            if (checklists == null || checklists.Count == 0)
            {
                PanelQualityGuidelines.Visibility = Visibility.Collapsed;
                return;
            }

            // Take the first recommended checklist (primary)
            var primaryChecklist = checklists.First();
            
            // Évaluation automatique
            _qualityService.EvaluateProtocol(primaryChecklist, currentProtocol);

            QualityStandardName.Text = $"({primaryChecklist.Name})";
            QualityGuidelinesList.Children.Clear();

            foreach (var section in primaryChecklist.Sections)
            {
                var txtSection = new System.Windows.Controls.TextBlock 
                { 
                    Text = section.Name, 
                    FontWeight = FontWeights.Bold, 
                    Margin = new Thickness(0, 10, 0, 5),
                    Foreground = (Brush)Application.Current.Resources["PrimaryBrush"]
                };
                QualityGuidelinesList.Children.Add(txtSection);

                foreach (var item in section.Items)
                {
                    var cb = new System.Windows.Controls.CheckBox 
                    { 
                        Content = new System.Windows.Controls.TextBlock { Text = item.Requirement, TextWrapping = TextWrapping.Wrap },
                        Margin = new Thickness(10, 0, 0, 5),
                        IsChecked = item.IsMet,
                        ToolTip = item.IsMet ? "Critère automatiquement validé par le contenu saisi" : "Ce critère ne semble pas encore rempli"
                    };
                    QualityGuidelinesList.Children.Add(cb);
                }
            }

            PanelQualityGuidelines.Visibility = Visibility.Visible;
        }

        private void UpdateSamplingTypes(StudyType studyType)
        {
            if (SamplingTypeComboBox == null) return;

            var allSamplingTypes = System.Enum.GetValues(typeof(SamplingType)).Cast<SamplingType>().ToList();
            List<SamplingType> filteredTypes;

            switch (studyType)
            {
                case StudyType.Quantitative:
                    filteredTypes = allSamplingTypes.Where(t => 
                        t == SamplingType.SimpleRandom || 
                        t == SamplingType.Systematic || 
                        t == SamplingType.Stratified || 
                        t == SamplingType.ClusterSampling || 
                        t == SamplingType.MultiStage || 
                        t == SamplingType.StratifiedCluster ||
                        t == SamplingType.Census ||
                        t == SamplingType.Convenience ||
                        t == SamplingType.None
                    ).ToList();
                    break;

                case StudyType.Qualitative:
                    filteredTypes = allSamplingTypes.Where(t => 
                        t == SamplingType.Purposive || 
                        t == SamplingType.Snowball || 
                        t == SamplingType.Quota || 
                        t == SamplingType.Convenience || 
                        t == SamplingType.Theoretical ||
                        t == SamplingType.None
                    ).ToList();
                    break;

                case StudyType.Mixed:
                default:
                    filteredTypes = allSamplingTypes;
                    break;
            }

            SamplingTypeComboBox.ItemsSource = filteredTypes;
            SamplingTypeComboBox.SelectedIndex = 0;
        }

        private void EpidemiologyTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CalculatorSelector == null) return;

            var epiType = (EpidemiologicalStudyType)EpidemiologyTypeComboBox.SelectedItem;
            
            // Suggest calculator based on study type (does not enforce, just selects)
            switch (epiType)
            {
                case EpidemiologicalStudyType.CrossSectionalDescriptive:
                case EpidemiologicalStudyType.Ecological:
                    SelectCalculator("Cochran");
                    break;

                case EpidemiologicalStudyType.LongitudinalIncidence:
                case EpidemiologicalStudyType.CohortProspective:
                case EpidemiologicalStudyType.CohortRetrospective:
                case EpidemiologicalStudyType.CaseControl:
                case EpidemiologicalStudyType.RandomizedControlledTrial:
                case EpidemiologicalStudyType.QuasiExperimental:
                case EpidemiologicalStudyType.BeforeAfterStudy:
                    SelectCalculator("Comparison");
                    break;
                
                default:
                    SelectCalculator("None");
                    break;
            }

            // Refresh Quality Audit (e.g. switch to CONSORT if RCT is selected)
            if (StudyTypeComboBox != null) UpdateQualityGuidelines((StudyType)StudyTypeComboBox.SelectedItem);
        }

        private void SelectCalculator(string tag)
        {
            foreach (System.Windows.Controls.ComboBoxItem item in CalculatorSelector.Items)
            {
                if (item.Tag.ToString() == tag)
                {
                    CalculatorSelector.SelectedItem = item;
                    break;
                }
            }
        }

        private void CalculatorSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PanelCochran == null || PanelComparison == null) return;
            
            PanelCochran.Visibility = Visibility.Collapsed;
            PanelComparison.Visibility = Visibility.Collapsed;

            if (CalculatorSelector.SelectedItem is System.Windows.Controls.ComboBoxItem item)
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
                
                // Population Totale (Schwartz)
                double N_pop = 0;
                if (!string.IsNullOrWhiteSpace(Calc_Cochran_N.Text))
                {
                    double.TryParse(Calc_Cochran_N.Text, out N_pop);
                }

                var selectedItem = (System.Windows.Controls.ComboBoxItem)Calc_Cochran_Z.SelectedItem;
                double z = double.Parse(selectedItem.Tag?.ToString() ?? "1.96", System.Globalization.CultureInfo.InvariantCulture);

                // 1. Calcul de base (Cochran / Population Infinie)
                // n0 = (Z² * p * q) / d²
                double n0 = (Math.Pow(z, 2) * p * (1 - p)) / Math.Pow(d, 2);
                
                double nFinal = n0;
                string formulaUsed = "Cochran (population infinie)";

                // 2. Correction si Population Finie (Schwartz)
                if (N_pop > 0)
                {
                    // n = n0 / (1 + (n0 - 1) / N)
                    nFinal = n0 / (1 + ((n0 - 1) / N_pop));
                    formulaUsed = $"Schwartz (population finie N={N_pop})";
                }

                int nCeiling = (int)Math.Ceiling(nFinal);

                ResultCochran.Text = $"N requis = {nCeiling} sujets";
                
                // Injection
                SamplingTextBox.Text = $"La taille d'échantillon minimal a été calculée selon la formule de {formulaUsed}. " +
                                       $"Avec une prévalence attendue de {p*100}%, une marge d'erreur de {d*100}% et un niveau de confiance de 95% (Z={z}), " +
                                       $"le nombre de sujets requis est de {nCeiling}.";
            }
            catch { MessageBox.Show("Vérifiez vos valeurs numériques (utilisez des points ou virgules)."); }
        }

        private void CalculateComparison_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double p1 = double.Parse(Calc_Comp_P1.Text) / 100.0;
                double p2 = double.Parse(Calc_Comp_P2.Text) / 100.0;
                
                var itemAlpha = (System.Windows.Controls.ComboBoxItem)Calc_Comp_Alpha.SelectedItem;
                double zAlpha = double.Parse(itemAlpha.Tag?.ToString() ?? "1.96", System.Globalization.CultureInfo.InvariantCulture); 
                
                var itemBeta = (System.Windows.Controls.ComboBoxItem)Calc_Comp_Power.SelectedItem;
                double zBeta = double.Parse(itemBeta.Tag?.ToString() ?? "1.28", System.Globalization.CultureInfo.InvariantCulture);

                bool isMatched = Chk_Matched.IsChecked == true;
                double nFinal = 0;
                string resultText = "";
                string formulaDesc = "";

                if (!isMatched)
                {
                    // --- INDÉPENDANTS (Formule Classique Fleiss/Casalegno) ---
                    // n = [ (Zalpha + Zbeta)² * (p1(1-p1) + p2(1-p2)) ] / (p1 - p2)²
                    double numerator = Math.Pow(zAlpha + zBeta, 2) * ( (p1*(1-p1)) + (p2*(1-p2)) );
                    double denominator = Math.Pow(p1 - p2, 2);
                    
                    if (denominator == 0) { MessageBox.Show("P1 et P2 ne peuvent pas être identiques."); return; }

                    nFinal = Math.Ceiling(numerator / denominator);
                    resultText = $"N = {nFinal} par groupe (Total: {nFinal*2})";
                    formulaDesc = $"comparaison de proportions (groupes indépendants)";
                     
                    SamplingTextBox.Text = $"Pour mettre en évidence une différence entre P1={Calc_Comp_P1.Text}% et P2={Calc_Comp_P2.Text}% " +
                                           $"avec une puissance de {(itemBeta.Content?.ToString()?.Contains("80") == true ? "80" : "90")}% et un risque alpha de 5%, " +
                                           $"la taille d'échantillon requise est de {nFinal} sujets par groupe (Total: {nFinal*2}).";
                }
                else
                {
                    // --- APPARIÉS (Formule Connor / Schlesselman pour Paires) ---
                    // N_pairs = [ (Zalpha + Zbeta)² * ( p1(1-p1) + p2(1-p2) - 2*phi*sqrt(...) ) ] / (p1 - p2)²
                    // phi (correlation) implicite approx 0.2 pour design apparié standard
                    double phi = 0.2; 
                    
                    double term1 = p1 * (1 - p1);
                    double term2 = p2 * (1 - p2);
                    double termCorr = 2 * phi * Math.Sqrt(term1 * term2);
                    
                    double numerator = Math.Pow(zAlpha + zBeta, 2) * (term1 + term2 - termCorr); // Moins de variance car correlation
                    double denominator = Math.Pow(p1 - p2, 2);

                    if (denominator == 0) { MessageBox.Show("P1 et P2 ne peuvent pas être identiques."); return; }

                    nFinal = Math.Ceiling(numerator / denominator);
                    resultText = $"N = {nFinal} PAIRES (Total sujets: {nFinal*2})";
                    formulaDesc = $"comparaison de proportions APPARIÉES (McNemar/Connor, corrélation estimée 0.2)";

                    SamplingTextBox.Text = $"S'agissant d'une étude appariée (Matched Case-Control), la formule utilisée prend en compte la corrélation intra-paire. " +
                                           $"Avec P1={Calc_Comp_P1.Text}% et P2={Calc_Comp_P2.Text}%, " +
                                           $"il est nécessaire d'inclure {nFinal} PAIRES de sujets (soit {nFinal} Cas et {nFinal} Témoins).";
                }

                ResultComparison.Text = resultText;
            }
            catch { MessageBox.Show("Erreur de calcul. Vérifiez les entrées."); }
        }

        private void AddCoAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CoAuthLast.Text) || string.IsNullOrWhiteSpace(CoAuthFirst.Text))
            {
                MessageBox.Show("Veuillez au moins entrer un Nom et un Prénom.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var author = new AdRev.Domain.Models.Author
            {
                FirstName = CoAuthFirst.Text,
                LastName = CoAuthLast.Text,
                Institution = CoAuthInst.Text,
                Title = "Co-Aut.", 
                Email = CoAuthEmail.Text
            };

            _tempCoAuthors.Add(author);

            // Reset fields
            CoAuthFirst.Text = "";
            CoAuthLast.Text = "";
            CoAuthInst.Text = "";
            CoAuthEmail.Text = "";
            CoAuthFirst.Focus();
        }

        private void RemoveCoAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (CoAuthorsListBox.SelectedItem is AdRev.Domain.Models.Author selected)
            {
                _tempCoAuthors.Remove(selected);
            }
        }

        // Gestionnaires pour les études multicentriques
        private void IsMulticentricCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (MulticentricDetailsPanel != null)
                MulticentricDetailsPanel.Visibility = Visibility.Visible;
        }

        private void IsMulticentricCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (MulticentricDetailsPanel != null)
                MulticentricDetailsPanel.Visibility = Visibility.Collapsed;
        }

        // Gestionnaires pour l'échantillonnage avancé
        private void SamplingTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (StratificationPanel == null || ClusterPanel == null) return;

            var samplingType = (SamplingType)SamplingTypeComboBox.SelectedItem;
            
            // Réinitialiser tous les panneaux
            StratificationPanel.Visibility = Visibility.Collapsed;
            ClusterPanel.Visibility = Visibility.Collapsed;

            switch (samplingType)
            {
                case SamplingType.Stratified:
                case SamplingType.StratifiedCluster:
                    StratificationPanel.Visibility = Visibility.Visible;
                    if (samplingType == SamplingType.StratifiedCluster)
                        ClusterPanel.Visibility = Visibility.Visible;
                    break;

                case SamplingType.ClusterSampling:
                case SamplingType.MultiStage:
                    ClusterPanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void IsStratifiedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (StratificationDetailsPanel != null)
                StratificationDetailsPanel.Visibility = Visibility.Visible;
        }

        private void IsStratifiedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (StratificationDetailsPanel != null)
                StratificationDetailsPanel.Visibility = Visibility.Collapsed;
        }

        private void GenerateProtocolPlan_Click(object sender, RoutedEventArgs e)
        {
            // 1. Get Objectives
            string specObjs = SpecificObjectivesTextBox.Text;
            if (string.IsNullOrWhiteSpace(specObjs))
            {
                MessageBox.Show("Veuillez d'abord définir vos Objectifs Spécifiques (Étape 2).");
                return;
            }

            var studyType = (StudyType)StudyTypeComboBox.SelectedItem;
            var textPlan = new System.Text.StringBuilder();

            if (studyType == StudyType.Qualitative)
            {
                GenerateThematicAnalysisPlan(textPlan, specObjs);
            }
            else
            {
                GenerateStatisticalAnalysisPlan(textPlan, specObjs);
            }

            DataAnalysisTextBox.Text = textPlan.ToString();
            
            string msg = studyType == StudyType.Qualitative 
                ? "Plan d'Analyse Thématique (Cadre de Braun & Clarke) généré avec succès !" 
                : "Plan d'Analyse Statistique (SAP) généré avec succès !";
                
            MessageBox.Show(msg + " Vous pouvez maintenant l'adapter.", "Génération Terminée");
        }

        private void GenerateThematicAnalysisPlan(System.Text.StringBuilder sb, string specObjs)
        {
            sb.AppendLine("PLAN D'ANALYSE THÉMATIQUE (TAP)");
            sb.AppendLine("Cadre de référence : Braun & Clarke (2006)");
            sb.AppendLine("==============================================");
            sb.AppendLine();
            sb.AppendLine("L'analyse sera menée selon l'approche thématique réflexive de Braun & Clarke, suivant un processus récursif en 6 étapes :");
            sb.AppendLine();
            sb.AppendLine("1. FAMILIARISATION AVEC LES DONNÉES");
            sb.AppendLine("   - Transcription intégrale des entretiens/focus groups.");
            sb.AppendLine("   - Lectures répétées et prise de notes initiales pour s'imprégner du corpus.");
            sb.AppendLine();
            sb.AppendLine("2. CODAGE INITIAL");
            sb.AppendLine("   - Identification systématique d'unités de sens à travers tout le corpus.");
            sb.AppendLine("   - Attribution de codes sémantiques ou latents aux segments de texte pertinents.");
            sb.AppendLine();
            sb.AppendLine("3. RECHERCHE DE THÈMES");
            sb.AppendLine("   - Regroupement des codes en thèmes candidats plus larges.");
            sb.AppendLine("   - Organisation des relations entre les codes et les thèmes.");
            sb.AppendLine();
            sb.AppendLine("4. REVUE DES THÈMES");
            sb.AppendLine("   - Vérification de la cohérence des thèmes par rapport aux codes extraits.");
            sb.AppendLine("   - Raffinement de la 'carte thématique' pour s'assurer qu'elle reflète fidèlement le corpus.");
            sb.AppendLine();
            sb.AppendLine("5. DÉFINITION ET NOMMAGE DES THÈMES");
            sb.AppendLine("   - Analyse approfondie de chaque thème pour en extraire l'essence (le 'coeur' du thème).");
            sb.AppendLine("   - Attribution de noms concis et explicites pour chaque catégorie finale.");
            sb.AppendLine();
            sb.AppendLine("6. PRODUCTION DU RAPPORT");
            sb.AppendLine("   - Sélection des verbatims les plus illustratifs pour chaque thème.");
            sb.AppendLine("   - Liaison des thèmes avec la question de recherche et la littérature existante.");
            sb.AppendLine();
            sb.AppendLine("--- ANALYSES SPÉCIFIQUES ---");

            var lines = specObjs.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int idx = 1;
            foreach (var line in lines)
            {
                var l = line.Trim();
                if (string.IsNullOrWhiteSpace(l) || l.StartsWith("OS")) continue;

                sb.AppendLine($"{idx}. Pour l'objectif : \"{l}\"");
                sb.AppendLine("   -> L'analyse thématique cherchera à explorer les représentations et les expériences liées à ce point spécifique.");
                idx++;
            }
        }

        private void GenerateStatisticalAnalysisPlan(System.Text.StringBuilder textPlan, string specObjs)
        {
            textPlan.AppendLine("PLAN D'ANALYSE STATISTIQUE (SAP)");
            textPlan.AppendLine("==================================");
            
            // 2. Standard Descriptives
            textPlan.AppendLine("1. ANALYSE DESCRIPTIVE");
            textPlan.AppendLine("- Variables Qualitatives : Fréquences et Pourcentages.");
            textPlan.AppendLine("- Variables Quantitatives : Moyenne +/- Ecart-Type (si distribution normale) ou Médiane (IQR).");
            textPlan.AppendLine();

            // 3. Parse and Map Objectives
            textPlan.AppendLine("2. ANALYSES INFERENTIELLES (répondant aux objectifs spécifiques)");
            
            var lines = specObjs.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int idx = 1;
            foreach(var line in lines)
            {
                var l = line.Trim();
                if (string.IsNullOrWhiteSpace(l) || l.StartsWith("OS")) continue;

                textPlan.AppendLine($"2.{idx} Pour l'objectif : \"{l}\"");
                
                string lower = l.ToLower();
                if (lower.Contains("comparer") || lower.Contains("différence"))
                {
                    textPlan.AppendLine("   -> Test suggéré : Comparaison de Moyennes (T-Test) ou ANOVA.");
                    textPlan.AppendLine("   -> Variables : Variable Groupe (X) vs Variable Dépendante (Y).");
                }
                else if (lower.Contains("associer") || lower.Contains("lien") || lower.Contains("corréler") || lower.Contains("facteur"))
                {
                    textPlan.AppendLine("   -> Test suggéré : Test du Chi2 (Quali vs Quali) ou Corrélation (Quanti vs Quanti).");
                }
                else if (lower.Contains("déterminer") || lower.Contains("prévalence") || lower.Contains("décrire"))
                {
                     textPlan.AppendLine("   -> Test suggéré : Estimation avec Intervalle de Confiance à 95%.");
                }
                else
                {
                    textPlan.AppendLine("   -> Test suggéré : A définir selon la nature des variables.");
                }
                idx++;
            }
            
            textPlan.AppendLine();
            textPlan.AppendLine("3. SEUIL DE SIGNIFICATIVITÉ");
            textPlan.AppendLine("- P-value < 0.05 considérée comme statistiquement significative.");
        }

        private void IsClusterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ClusterDetailsPanel != null)
                ClusterDetailsPanel.Visibility = Visibility.Visible;
        }

        private void IsClusterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ClusterDetailsPanel != null)
                ClusterDetailsPanel.Visibility = Visibility.Collapsed;
        }
        
        private readonly ProtocolValidator _validator = new ProtocolValidator();

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        // Permettre le déplacement de la fenêtre sans barre de titre
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void GeneralObjective_LostFocus(object sender, RoutedEventArgs e)
        {
            // Auto-génération des puces pour les objectifs spécifiques
            if (!string.IsNullOrWhiteSpace(GeneralObjectiveTextBox.Text) && string.IsNullOrWhiteSpace(SpecificObjectivesTextBox.Text))
            {
                var lines = GeneralObjectiveTextBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                int count = Math.Max(1, lines.Length);
                
                var builder = new System.Text.StringBuilder();
                for (int i = 0; i < count; i++)
                {
                    builder.AppendLine("• [Objectif Spécifique 1]");
                    builder.AppendLine("• [Objectif Spécifique 2]");
                    if (count > 1 && i < count - 1) builder.AppendLine("---");
                }
                SpecificObjectivesTextBox.Text = builder.ToString();
            }
        }

        private int _currentStep = 1;
        private const int TotalSteps = 14;

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

        private void UpdateView()
        {
            // Reset all to Collapsed
            if (View_Step1 != null) View_Step1.Visibility = Visibility.Collapsed;
            if (View_Introduction != null) View_Introduction.Visibility = Visibility.Collapsed;
            if (View_Objectives != null) View_Objectives.Visibility = Visibility.Collapsed;
            if (View_Step3 != null) View_Step3.Visibility = Visibility.Collapsed;
            if (View_Methodology_Design != null) View_Methodology_Design.Visibility = Visibility.Collapsed;
            if (View_Methodology_Population != null) View_Methodology_Population.Visibility = Visibility.Collapsed;
            if (View_Methodology_Sampling != null) View_Methodology_Sampling.Visibility = Visibility.Collapsed;
            if (View_Methodology_Data != null) View_Methodology_Data.Visibility = Visibility.Collapsed;
            if (View_Budget != null) View_Budget.Visibility = Visibility.Collapsed;
            if (View_Step10 != null) View_Step10.Visibility = Visibility.Collapsed;
            if (View_DataAnalysis != null) View_DataAnalysis.Visibility = Visibility.Collapsed;
            if (View_Results != null) View_Results.Visibility = Visibility.Collapsed;
            if (View_Conclusion != null) View_Conclusion.Visibility = Visibility.Collapsed;
            if (View_References != null) View_References.Visibility = Visibility.Collapsed;

            // Sync Side Menu if needed (avoids loop if already selected)
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
                case 4: if (View_Step3 != null) View_Step3.Visibility = Visibility.Visible; break;
                case 5: 
                    if (View_Methodology_Design != null) View_Methodology_Design.Visibility = Visibility.Visible;
                    // Mise à jour de l'audit qualité à l'affichage de l'étape
                    if (StudyTypeComboBox != null) UpdateQualityGuidelines((StudyType)StudyTypeComboBox.SelectedItem);
                    break;
                case 6: if (View_Methodology_Population != null) View_Methodology_Population.Visibility = Visibility.Visible; break;
                case 7: if (View_Methodology_Sampling != null) View_Methodology_Sampling.Visibility = Visibility.Visible; break;
                case 8: if (View_Methodology_Data != null) View_Methodology_Data.Visibility = Visibility.Visible; break;
                case 9: 
                    if (View_Budget != null) View_Budget.Visibility = Visibility.Visible; 
                    UpdateBudgetView();
                    break;
                case 10: if (View_Step10 != null) View_Step10.Visibility = Visibility.Visible; break;
                case 11: if (View_DataAnalysis != null) View_DataAnalysis.Visibility = Visibility.Visible; break;
                case 12: if (View_Results != null) View_Results.Visibility = Visibility.Visible; break;
                case 13: if (View_Conclusion != null) View_Conclusion.Visibility = Visibility.Visible; break;
                case 14: if (View_References != null) View_References.Visibility = Visibility.Visible; break;
            }

            // Set Read-Only based on step-linked access
            bool canEdit = CanUserEditStep(_currentStep);
            SetProtocolReadOnly(!canEdit);
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

                // Check intervention
                var epiType = (EpidemiologicalStudyType)EpidemiologyTypeComboBox.SelectedItem;
                if (epiType == EpidemiologicalStudyType.RandomizedControlledTrial || epiType == EpidemiologicalStudyType.QuasiExperimental)
                {
                    suggestions += "\n\nSpécifique Essai Clinique/Intervention :\n" +
                                   "• Achat des produits/médicaments\n" +
                                   "• Assurance responsabilité civile\n" +
                                   "• Examens biologiques/cliniques de suivi...";
                }
            }
            else // Mixed
            {
                typeDesc = "MÉTHODE MIXTE";
                suggestions = "• Combinaison des coûts Quantitatifs et Qualitatifs\n" +
                              "• Réunions de coordination pour l'intégration des données...";
            }

            BudgetSuggestionType.Text = $"({typeDesc})";
            BudgetSuggestionsText.Text = suggestions;
        }

        private void CreateProtocol_Click(object sender, RoutedEventArgs e)
        {
            ValidationMessagesListBox.Items.Clear();

            var protocol = BuildProtocolFromUI();

            var validationResult = _validator.Validate(protocol);

            // Affichage du score et des messages
            if (validationResult.Score == 100)
            {
                 ValidationMessagesListBox.Items.Add($"🌟 EXCELLENT TRAVAIL ! Score Qualité : 100/100");
                 ValidationMessagesListBox.Items.Add("Le protocole respecte tous les critères de qualité.");
            }
            else
            {
                ValidationMessagesListBox.Items.Add($"📊 Score Qualité : {validationResult.Score}/100");
                
                foreach (var err in validationResult.Errors)
                    ValidationMessagesListBox.Items.Add(err);

                foreach (var sug in validationResult.Suggestions)
                    ValidationMessagesListBox.Items.Add(sug);
            }

            // Sauvegarde
            if (validationResult.Errors.Count == 0)
            {
                 string pName = _project != null ? _project.Title : ("Projet_Sans_Titre_" + DateTime.Now.ToString("yyyyMMdd"));
                 _service.Create(protocol, pName);
                 
                 // Sync with Project
                 if (_project != null)
                 {
                     _project.GeneralObjective = protocol.GeneralObjective;
                     _project.SpecificObjectives = protocol.SpecificObjectives;
                     _project.DataAnalysisPlan = protocol.DataAnalysis; // Sync SAP
                 }

                 ValidationMessagesListBox.Items.Add($"💾 Protocole enregistré avec succès (ID: {protocol.Id})");
            }
            else
            {
                 ValidationMessagesListBox.Items.Add("🛑 Veuillez corriger les erreurs critiques (❌) avant l'enregistrement.");
            }
        }

        private ResearchProtocol BuildProtocolFromUI()
        {
            var protocol = new ResearchProtocol
            {
                Id = Guid.NewGuid().ToString(),
                ProjectId = _project?.Id,
                Title = TitleTextBox.Text,
                StudyType = StudyTypeComboBox.SelectedItem != null ? (StudyType)StudyTypeComboBox.SelectedItem : StudyType.Quantitative,
                Domain = DomainComboBox.SelectedItem != null ? (ScientificDomain)DomainComboBox.SelectedItem : ScientificDomain.Biomedical,
                QualitativeApproach = (StudyTypeComboBox.SelectedItem is StudyType st && st == StudyType.Qualitative && QualitativeApproachComboBox.SelectedItem != null) 
                    ? (QualitativeApproach)QualitativeApproachComboBox.SelectedItem 
                    : QualitativeApproach.Inductive, 
                EpidemiologyType = EpidemiologyTypeComboBox.SelectedItem != null ? (EpidemiologicalStudyType)EpidemiologyTypeComboBox.SelectedItem : EpidemiologicalStudyType.CrossSectionalDescriptive,
                
                PrincipalAuthor = new AdRev.Domain.Models.Author 
                {
                    FirstName = AuthorFirstNameBox.Text,
                    LastName = AuthorLastNameBox.Text,
                    Title = AuthorTitleBox.SelectedItem is System.Windows.Controls.ComboBoxItem cbi ? cbi.Content?.ToString() ?? "" : "", 
                    Institution = AuthorInstitutionBox.Text,
                    Email = AuthorEmailBox.Text
                },
                
                Context = ContextTextBox.Text,
                ProblemJustification = ProblemTextBox.Text,
                ResearchQuestion = ResearchQuestionTextBox.Text,
                Hypotheses = HypothesesTextBox.Text,
                GeneralObjective = GeneralObjectiveTextBox.Text,
                SpecificObjectives = SpecificObjectivesTextBox.Text,
                ConceptDefinitions = ConceptsTextBox.Text,
                
                StudySetting = StudySettingTextBox.Text,
                ConceptualModel = ConceptualModelTextBox.Text,
                
                IsMulticentric = IsMulticentricCheckBox.IsChecked == true,
                StudyCenters = StudyCentersTextBox?.Text ?? string.Empty,
                
                StudyPopulation = PopulationTextBox.Text,
                InclusionCriteria = InclusionTextBox.Text,
                ExclusionCriteria = ExclusionTextBox.Text,
                
                SamplingType = SamplingTypeComboBox.SelectedItem != null ? (SamplingType)SamplingTypeComboBox.SelectedItem : SamplingType.None,
                IsStratified = IsStratifiedCheckBox.IsChecked == true,
                StratificationCriteria = StratificationCriteriaTextBox?.Text ?? string.Empty,
                IsClusterSampling = IsClusterCheckBox.IsChecked == true,
                ClusterSize = int.TryParse(ClusterSizeTextBox?.Text ?? string.Empty, out int clusterSize) ? clusterSize : 0,
                DesignEffect = double.TryParse(DesignEffectTextBox?.Text ?? string.Empty, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double deff) ? deff : 1.0,
                ExpectedLossRate = double.TryParse(ExpectedLossRateTextBox?.Text ?? string.Empty, out double lossRate) ? lossRate : 0.0,
                
                SamplingMethod = SamplingTextBox.Text,
                DataCollection = DataCollectionTextBox.Text,
                DataAnalysis = DataAnalysisTextBox.Text,
                
                // DataAnalysis duplicate removed
                
                DiscussionPlan = _discussionPlan,
                StudyLimitations = _limitations,
                
                Budget = BudgetTextBox?.Text ?? "",

                Ethics = EthicsTextBox.Text,
                ExpectedResults = ExpectedResultsTextBox.Text,
                Conclusion = ConclusionTextBox.Text,
                References = ReferencesTextBox.Text,
                
                ReferenceStyle = RefStyleComboStep1.SelectedItem != null ? (ReferenceStyle)RefStyleComboStep1.SelectedItem : ReferenceStyle.Vancouver,
                Citations = _citations
            };

            foreach(var auth in _tempCoAuthors) protocol.CoAuthors.Add(auth);
            protocol.Variables = _tempVariables;

            return protocol;
        }

        private void RefreshAudit_LostFocus(object sender, RoutedEventArgs e)
        {
             if (StudyTypeComboBox != null && PanelQualityGuidelines != null && PanelQualityGuidelines.Visibility == Visibility.Visible)
             {
                 UpdateQualityGuidelines((StudyType)StudyTypeComboBox.SelectedItem);
             }
        }

        private void SideMenu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SideMenuListBox.SelectedItem is System.Windows.Controls.ListBoxItem item && item.Tag != null)
            {
                if (int.TryParse(item.Tag.ToString(), out int step))
                {
                    _currentStep = step;
                    UpdateView();
                }
            }
        }

        // --- Menu Actions ---

        private void MenuFileNew_Click(object sender, RoutedEventArgs e)
        {
             // Reset UI or logic
        }

        private void MenuDiscussion_Click(object sender, RoutedEventArgs e)
        {
             var type = StudyTypeComboBox.SelectedItem != null ? (StudyType)StudyTypeComboBox.SelectedItem : StudyType.Quantitative;
             var obj = SpecificObjectivesTextBox.Text;
             var style = RefStyleComboStep1.SelectedItem as ReferenceStyle? ?? ReferenceStyle.Vancouver;
             int nextIndex = _citations.Count + 1;
             
             var win = new DiscussionWindow(type, obj, _discussionPlan, _limitations, nextIndex, style);
             if (win.ShowDialog() == true)
             {
                 _discussionPlan = win.DiscussionPlan;
                 _limitations = win.Limitations;
                 
                 // Process new citations from Discussion
                 if (win.NewCitations.Any())
                 {
                     _citations.AddRange(win.NewCitations);
                     RegenerateReferencesList();
                 }
                 
                 ValidationMessagesListBox.Items.Add("✅ Discussion mise à jour ! N'oubliez pas d'enregistrer le protocole.");
             }
        }
        // --- Gestion des Références ---
        
        private List<AdRev.Domain.Models.Citation> _citations = new List<AdRev.Domain.Models.Citation>();
        private ReferenceStyle _lastStyle = ReferenceStyle.Vancouver; // Default start style
        
        // Liste temporaire supprimée car gérée par CitationEntryWindow

        private bool _isSyncingStyle = false;
        private System.Windows.Controls.TextBox? _targetCitationTextBox; // The textbox where citation will be inserted

        private void RefStyleCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isSyncingStyle) return;
            
            if (sender is System.Windows.Controls.ComboBox cb && cb.SelectedItem is ReferenceStyle style)
            {
                _isSyncingStyle = true;
                if (RefStyleComboStep1 != null && RefStyleComboStep1 != cb) RefStyleComboStep1.SelectedItem = style;
                if (RefStyleComboStep13 != null && RefStyleComboStep13 != cb) RefStyleComboStep13.SelectedItem = style;
                
                // Update Text Content Dynamically
                UpdateTextCitations(_lastStyle, style);
                _lastStyle = style;
                
                _isSyncingStyle = false;

                RegenerateReferencesList();
            }
        }
        
        private void UpdateTextCitations(ReferenceStyle oldStyle, ReferenceStyle newStyle)
        {
            // List of all textboxes that support citations
            var boxes = new List<System.Windows.Controls.TextBox> 
            {
                ContextTextBox, ProblemTextBox, ResearchQuestionTextBox, HypothesesTextBox,
                GeneralObjectiveTextBox, SpecificObjectivesTextBox,
                ConceptsTextBox, ConceptualModelTextBox, StudySettingTextBox,
                PopulationTextBox, StudyCentersTextBox, InclusionTextBox, ExclusionTextBox,
                SamplingTextBox, DataCollectionTextBox, DataAnalysisTextBox,
                BudgetTextBox, EthicsTextBox, ExpectedResultsTextBox, ConclusionTextBox
            };

            foreach (var box in boxes)
            {
                if (box == null || string.IsNullOrEmpty(box.Text)) continue;

                string text = box.Text;

                for (int i = 0; i < _citations.Count; i++)
                {
                    var cit = _citations[i];
                    int index = i + 1;
                    
                    string oldMarker = "";
                    string newMarker = "";

                    // Construct Old Marker regex pattern or string
                    if (oldStyle == ReferenceStyle.Vancouver) oldMarker = $"[{index}]";
                    else oldMarker = $"({cit.Authors}, {cit.Year})";
                    
                    // Construct New Marker
                    if (newStyle == ReferenceStyle.Vancouver) newMarker = $"[{index}]";
                    else newMarker = $"({cit.Authors}, {cit.Year})";
                    
                    if (!string.IsNullOrEmpty(oldMarker) && text.Contains(oldMarker))
                    {
                        text = text.Replace(oldMarker, newMarker);
                    }
                }
                box.Text = text;
            }
            
            // Also update hidden Discussion strings
            if (!string.IsNullOrEmpty(_discussionPlan)) _discussionPlan = UpdateStringCitations(_discussionPlan, oldStyle, newStyle);
            if (!string.IsNullOrEmpty(_limitations)) _limitations = UpdateStringCitations(_limitations, oldStyle, newStyle);
        }

        private string UpdateStringCitations(string text, ReferenceStyle oldStyle, ReferenceStyle newStyle)
        {
            if (string.IsNullOrEmpty(text)) return text;
             for (int i = 0; i < _citations.Count; i++)
            {
                var cit = _citations[i];
                int index = i + 1;
                
                string oldMarker = GetCitationMarker(oldStyle, cit, index);
                string newMarker = GetCitationMarker(newStyle, cit, index);
                
                if (!string.IsNullOrEmpty(oldMarker) && text.Contains(oldMarker))
                {
                    text = text.Replace(oldMarker, newMarker);
                }
            }
            return text;
        }

        private bool IsNumericStyle(ReferenceStyle style)
        {
            return style == ReferenceStyle.Vancouver || style == ReferenceStyle.IEEE || 
                   style == ReferenceStyle.AMA || style == ReferenceStyle.Nature || 
                   style == ReferenceStyle.Science || style == ReferenceStyle.ISO690_Numeric ||
                   style == ReferenceStyle.ACS;
        }

        private string GetCitationMarker(ReferenceStyle style, AdRev.Domain.Models.Citation cit, int index)
        {
            if (IsNumericStyle(style)) return $"[{index}]";
            
            if (style == ReferenceStyle.Chicago) return $"({cit.Authors} {cit.Year})";
            if (style == ReferenceStyle.MLA) return $"({cit.Authors})"; // Simplified
            if (style == ReferenceStyle.MHRA) return $"[{index}]"; // MHRA uses notes, simplified to Ref Number here for text flow
            if (style == ReferenceStyle.Elsevier) return $"({cit.Authors}, {cit.Year})";
            
            // Default APA, Harvard
            return $"({cit.Authors}, {cit.Year})";
        }

        private string GetBibliographicEntry(ReferenceStyle style, AdRev.Domain.Models.Citation cit, int index)
        {
            string authors = cit.Authors;
            string year = cit.Year;
            string title = cit.Title;
            string source = cit.Source;

            switch (style)
            {
                case ReferenceStyle.Vancouver:
                    return $"{index}. {authors}. {title}. {source}; {year}.";
                case ReferenceStyle.APA:
                    return $"{authors} ({year}). {title}. {source}.";
                case ReferenceStyle.Harvard:
                    return $"{authors} ({year}) '{title}', {source}.";
                case ReferenceStyle.Chicago:
                    return $"{authors}. {year}. \"{title}.\" {source}.";
                case ReferenceStyle.MLA:
                    return $"{authors}. \"{title}.\" {source}, {year}.";
                case ReferenceStyle.IEEE:
                    return $"[{index}] {authors}, \"{title},\" {source}, {year}.";
                case ReferenceStyle.AMA:
                    return $"{index}. {authors}. {title}. {source}. {year}.";
                case ReferenceStyle.Nature:
                    return $"{index}. {authors} {title}. {source} ({year}).";
                case ReferenceStyle.Science:
                    return $"{index}. {authors}, {title}. {source} ({year}).";
                case ReferenceStyle.ACS:
                     // ACS: (1) Author. Title. Journal Year, Vol, Page.
                    return $"({index}) {authors}. {title}. {source} {year}."; 
                case ReferenceStyle.MHRA:
                    // MHRA: Author, Title (PubData), Year.
                    // Simplified: Author, Title, Source, Year.
                    return $"{authors}, {title}, {source}, {year}.";
                case ReferenceStyle.Elsevier:
                     // Elsevier: Author, Year. Title. Container Volume, Page.
                     return $"{authors}, {year}. {title}. {source}.";
                case ReferenceStyle.ISO690_Numeric:
                    return $"{index}. {authors.ToUpper()}. {title}. {source}, {year}.";
                default:
                    return $"[{index}] {authors}, {title} ({year})";
            }
        }

        private void AddCitation_Click(object sender, RoutedEventArgs e)
        {
            // Determine target textbox
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string boxName)
            {
                _targetCitationTextBox = this.FindName(boxName) as System.Windows.Controls.TextBox;
            }
            
            // Open Citation Entry Window
            var dialog = new AdRev.Desktop.CitationEntryWindow();
            if (dialog.ShowDialog() == true && dialog.ResultCitations.Any())
            {
                var newCitations = dialog.ResultCitations;
                List<int> addedIndices = new List<int>();

                // Add to main list and track indices
                foreach (var cit in newCitations)
                {
                    _citations.Add(cit);
                    addedIndices.Add(_citations.Count);
                }

                // Insert into Text
                if (_targetCitationTextBox != null)
                {
                   int caretIndex = _targetCitationTextBox.CaretIndex;
                   string marker = "";
                   var style = RefStyleComboStep1.SelectedItem as ReferenceStyle? ?? ReferenceStyle.Vancouver;

                   if (IsNumericStyle(style) || style == ReferenceStyle.MHRA)
                   {
                       if (addedIndices.Count == 1) marker = $" [{addedIndices[0]}]";
                       else marker = $" [{string.Join(", ", addedIndices)}]";
                   }
                   else if (style == ReferenceStyle.Chicago)
                   {
                        var parts = newCitations.Select(c => $"{c.Authors} {c.Year}");
                        marker = $" ({string.Join("; ", parts)})";
                   }
                   else if (style == ReferenceStyle.MLA)
                   {
                        var parts = newCitations.Select(c => $"{c.Authors}");
                        marker = $" ({string.Join("; ", parts)})";
                   }
                   else if (style == ReferenceStyle.MHRA)
                   {
                        marker = $" [{string.Join(", ", addedIndices)}]";
                   }
                   else if (style == ReferenceStyle.Elsevier)
                   {
                        var parts = newCitations.Select(c => $"{c.Authors}, {c.Year}");
                        marker = $" ({string.Join("; ", parts)})";
                   }
                   else
                   {
                        // APA, Harvard default
                        var parts = newCitations.Select(c => $"{c.Authors}, {c.Year}");
                        marker = $" ({string.Join("; ", parts)})";
                   }

                   _targetCitationTextBox.Text = _targetCitationTextBox.Text.Insert(caretIndex, marker);
                   _targetCitationTextBox.CaretIndex = caretIndex + marker.Length;
                   _targetCitationTextBox.Focus();
                }

                RegenerateReferencesList();
            }
        }

        private void RegenerateReferences_Click(object sender, RoutedEventArgs e)
        {
            RegenerateReferencesList();
        }

        private void RegenerateReferencesList()
        {
            if (ReferencesTextBox == null || RefStyleComboStep1 == null) return;
            
            var style = (ReferenceStyle)RefStyleComboStep1.SelectedItem;
            var sb = new System.Text.StringBuilder();

            int index = 1;
            foreach (var cit in _citations)
            {
                sb.AppendLine(GetBibliographicEntry(style, cit, index));
                index++;
            }
            
            ReferencesTextBox.Text = sb.ToString();
        }

        private void ExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var protocol = BuildProtocolForWordExport();
                
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Document Word (*.docx)|*.docx";
                
                // Safe Title for filename
                string safeTitle = string.Join("_", protocol.Title.Split(System.IO.Path.GetInvalidFileNameChars()));
                if(string.IsNullOrWhiteSpace(safeTitle)) safeTitle = "Protocole";

                saveFileDialog.FileName = $"Protocole_{safeTitle}_{DateTime.Now:yyyyMMdd}.docx";
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    _wordExportService.ExportProtocolToWord(protocol, saveFileDialog.FileName);
                    MessageBox.Show("Exportation terminée avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exportation : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private AdRev.Domain.Protocols.ResearchProtocol BuildProtocolForWordExport()
        {
            var p = new AdRev.Domain.Protocols.ResearchProtocol();
            p.Title = TitleTextBox.Text;
            
            // Authors
            p.PrincipalAuthor = new AdRev.Domain.Models.Author 
            { 
               LastName = AuthorLastNameBox.Text, 
               FirstName = AuthorFirstNameBox.Text,
               Institution = AuthorInstitutionBox.Text,
               Email = AuthorEmailBox.Text
            };
            
            p.CoAuthors = _tempCoAuthors.ToList();

            // Intro
            p.Context = ContextTextBox.Text;
            p.ProblemJustification = ProblemTextBox.Text;
            p.ResearchQuestion = ResearchQuestionTextBox.Text;
            p.Hypotheses = HypothesesTextBox.Text;
            
            // Obj
            p.GeneralObjective = GeneralObjectiveTextBox.Text;
            p.SpecificObjectives = SpecificObjectivesTextBox.Text;
            
            // Methodo
            if (StudyTypeComboBox.SelectedItem is StudyType st) p.StudyType = st;
            if (StudySettingTextBox != null) p.StudySetting = StudySettingTextBox.Text;
            if (PopulationTextBox != null) p.StudyPopulation = PopulationTextBox.Text;
            if (InclusionTextBox != null) p.InclusionCriteria = InclusionTextBox.Text;
            if (ExclusionTextBox != null) p.ExclusionCriteria = ExclusionTextBox.Text;
            
            if (SamplingTextBox != null) p.SamplingMethod = SamplingTextBox.Text;
            if (DataCollectionTextBox != null) p.DataCollection = DataCollectionTextBox.Text;
            if (DataAnalysisTextBox != null) p.DataAnalysis = DataAnalysisTextBox.Text;
            if (EthicsTextBox != null) p.Ethics = EthicsTextBox.Text;
            
            // Results/Discussion
            if (ExpectedResultsTextBox != null) p.ExpectedResults = ExpectedResultsTextBox.Text;
            p.StudyLimitations = _limitations; 
            if (ConclusionTextBox != null) p.Conclusion = ConclusionTextBox.Text;
            
            if (ReferencesTextBox != null) p.References = ReferencesTextBox.Text;

            return p;
        }
    }
}
