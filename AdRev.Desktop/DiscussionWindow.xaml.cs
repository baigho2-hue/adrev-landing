using System;
using System.Windows;
using System.Linq;
using AdRev.Domain.Enums;
using AdRev.Domain.Models;
using AdRev.Core.Services;
using System.Collections.Generic;

namespace AdRev.Desktop
{
    public partial class DiscussionWindow : Window
    {
        private readonly QualityService _qualityService = new QualityService();
        private readonly StudyType _studyType;
        private readonly string _objectives;
        public System.Collections.ObjectModel.ObservableCollection<ObjectiveItem> ObjectiveItems { get; set; } = new System.Collections.ObjectModel.ObservableCollection<ObjectiveItem>();

        public class ObjectiveItem : System.ComponentModel.INotifyPropertyChanged
        {
            public string ObjectiveTitle { get; set; } = "";
            private string _content = "";
            public string Content 
            { 
                get => _content; 
                set { _content = value; OnPropertyChanged(nameof(Content)); } 
            }

            public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        
        public string DiscussionPlan { get; private set; } = string.Empty;
        public string Limitations { get; private set; } = string.Empty;
        
        // Citation support
        public List<Citation> NewCitations { get; private set; } = new List<Citation>();
        private int _nextCitationIndex;
        private ReferenceStyle _currentStyle;
        private System.Windows.Controls.TextBox? _targetBox;

        public string ImageSource { get; set; } = "pack://application:,,,/AdRev.Desktop;component/Assets/Discussion/discussion_assistant_header_1768519440517.png";
        public string LimitationsImageSource { get; set; } = "pack://application:,,,/AdRev.Desktop;component/Assets/Discussion/discussion_limitations_icon_1768519561449.png";

        public DiscussionWindow(StudyType studyType, string specificObjectives, string currentPlan, string currentLimitations, int nextCitationIndex, ReferenceStyle style)
        {
            InitializeComponent();
            this.DataContext = this;
            _studyType = studyType;
            _objectives = specificObjectives;
            _nextCitationIndex = nextCitationIndex;
            _currentStyle = style;

            DiscussionPlanTextBox.Text = currentPlan;
            LimitationsTextBox.Text = currentLimitations;

            InitializeObjectiveItems();
            LoadSupportPath();
        }

        private void InitializeObjectiveItems()
        {
            if (string.IsNullOrWhiteSpace(_objectives)) return;

            var lines = _objectives.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string clean = line.Trim();
                if (clean.StartsWith("•") || clean.StartsWith("-")) clean = clean.Substring(1).Trim();
                if (clean.Length > 5)
                {
                    ObjectiveItems.Add(new ObjectiveItem { ObjectiveTitle = clean });
                }
            }
        }

        private void MergeObjectives_Click(object sender, RoutedEventArgs e)
        {
            var sb = new System.Text.StringBuilder(DiscussionPlanTextBox.Text);
            if (sb.Length > 0 && !sb.ToString().EndsWith("\n")) sb.AppendLine();

            foreach (var item in ObjectiveItems)
            {
                if (!string.IsNullOrWhiteSpace(item.Content))
                {
                    sb.AppendLine($"--- {item.ObjectiveTitle} ---");
                    sb.AppendLine(item.Content);
                    sb.AppendLine();
                }
            }
            DiscussionPlanTextBox.Text = sb.ToString();
            MessageBox.Show("Les interprétations par objectif ont été ajoutées à l'ébauche globale.", "Fusion réussie");
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }



        private void LoadSupportPath()
        {
            var dummyProject = new ResearchProject { StudyType = _studyType };
            var checklists = _qualityService.GetRecommendedChecklists(dummyProject);

            var sbNorms = new System.Text.StringBuilder();
            var sbExpert = new System.Text.StringBuilder();

            // Normes
            if (checklists != null)
            {
                foreach(var list in checklists)
                {
                    var discSection = list.Sections.FirstOrDefault(s => s.Name.Contains("Discussion") || s.Name.Contains("Reporting") || s.Name.Contains("Analyse"));
                    if (discSection != null)
                    {
                        sbNorms.AppendLine($"--- {list.Name} ---");
                        foreach(var item in discSection.Items)
                        {
                           if(item.Requirement.Contains("Résultat") || item.Requirement.Contains("Limitation") || item.Requirement.Contains("Interprétation"))
                                sbNorms.AppendLine($"• {item.Requirement}");
                        }
                    }
                }
            }
            if (sbNorms.Length == 0) sbNorms.Append("Aucune norme spécifique détectée.");

            // Idées Expert
            sbExpert.AppendLine("• Commencez par un résumé factuel.");
            if (_studyType == StudyType.Quantitative)
            {
                sbExpert.AppendLine("• Discutez la validité interne.");
                sbExpert.AppendLine("• Comparez avec la littérature.");
                sbExpert.AppendLine("• Généralisabilité.");
            }
            else if (_studyType == StudyType.Qualitative)
            {
                sbExpert.AppendLine("• Réflexivité du chercheur.");
                sbExpert.AppendLine("• Intégration théorique.");
                sbExpert.AppendLine("• Transférabilité.");
            }

            NormsText.Text = sbNorms.ToString();
            ExpertText.Text = sbExpert.ToString();
        }

        private void GeneratePlan_Click(object sender, RoutedEventArgs e)
        {
             var sb = new System.Text.StringBuilder();
             
             sb.AppendLine("1. RESUME DES RESULTATS");
             sb.AppendLine("- Rappel succinct du résultat principal.");
             sb.AppendLine("");

             sb.AppendLine("2. COMPARAISON AVEC LA LITTERATURE");
             if (!string.IsNullOrWhiteSpace(_objectives))
             {
                 var lines = _objectives.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                 foreach(var line in lines)
                 {
                     string clean = line.Replace("•", "").Replace("-", "").Trim();
                     if (clean.Length > 5)
                         sb.AppendLine($"- Point pour : {clean}");
                 }
             }
             else
             {
                 sb.AppendLine("- Discutez chaque objectif spécifique.");
             }
             sb.AppendLine("");

             sb.AppendLine("3. FORCES ET LIMITES");
             sb.AppendLine("- Forces méthodologiques.");
             sb.AppendLine("- Limites (Biais...)");
             sb.AppendLine("");

             sb.AppendLine("4. IMPLICATIONS");
             sb.AppendLine("- Recommandations.");

             DiscussionPlanTextBox.Text = sb.ToString();
        }

        private void AddCitation_Click(object sender, RoutedEventArgs e)
        {
             if (sender is System.Windows.Controls.Button btn && btn.Tag is string boxName)
            {
                _targetBox = this.FindName(boxName) as System.Windows.Controls.TextBox;
            }

            var dialog = new CitationEntryWindow();
            if (dialog.ShowDialog() == true && dialog.ResultCitations.Any())
            {
                var added = dialog.ResultCitations;
                NewCitations.AddRange(added);

                List<int> addedIndices = new List<int>();
                foreach(var c in added)
                {
                    addedIndices.Add(_nextCitationIndex++);
                }

                if (_targetBox != null)
                {
                   int caretIndex = _targetBox.CaretIndex;
                   string marker = "";

                   if (IsNumericStyle(_currentStyle) || _currentStyle == ReferenceStyle.MHRA)
                   {
                       if (addedIndices.Count == 1) marker = $" [{addedIndices[0]}]";
                       else marker = $" [{string.Join(", ", addedIndices)}]";
                   }
                   else if (_currentStyle == ReferenceStyle.Chicago)
                   {
                        var parts = added.Select(c => $"{c.Authors} {c.Year}");
                        marker = $" ({string.Join("; ", parts)})";
                   }
                   else if (_currentStyle == ReferenceStyle.MLA)
                   {
                        var parts = added.Select(c => $"{c.Authors}");
                        marker = $" ({string.Join("; ", parts)})";
                   }
                   else if (_currentStyle == ReferenceStyle.Elsevier)
                   {
                        var parts = added.Select(c => $"{c.Authors}, {c.Year}");
                        marker = $" ({string.Join("; ", parts)})";
                   }
                   else
                   {
                       var parts = added.Select(c => $"{c.Authors}, {c.Year}");
                       marker = $" ({string.Join("; ", parts)})";
                   }

                   _targetBox.Text = _targetBox.Text.Insert(caretIndex, marker);
                   _targetBox.CaretIndex = caretIndex + marker.Length;
                   _targetBox.Focus();
                }
            }
        }

        private bool IsNumericStyle(ReferenceStyle style)
        {
            return style == ReferenceStyle.Vancouver || style == ReferenceStyle.IEEE || 
                   style == ReferenceStyle.AMA || style == ReferenceStyle.Nature || 
                   style == ReferenceStyle.Science || style == ReferenceStyle.ISO690_Numeric ||
                   style == ReferenceStyle.ACS;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DiscussionPlan = DiscussionPlanTextBox.Text;
            Limitations = LimitationsTextBox.Text;
            DialogResult = true;
            Close();
        }
    }
}
