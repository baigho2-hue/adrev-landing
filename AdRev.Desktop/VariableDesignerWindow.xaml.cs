using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AdRev.Domain.Enums;
using AdRev.Domain.Variables;

namespace AdRev.Desktop
{
    public partial class VariableDesignerWindow : Window
    {
        // Collection observable pour lier à l'interface (mise à jour automatique)
        public ObservableCollection<StudyVariable> Variables { get; set; }
        private StudyVariable? _currentVariable;
        private bool _isUpdating = false;
        private bool _isQualitativeDeductive = false;

        public VariableDesignerWindow(List<StudyVariable> existingVariables, string projectTitle, bool isQualitativeDeductive = false)
        {
            InitializeComponent();
            _isQualitativeDeductive = isQualitativeDeductive;
            TxtProjectTitle.Text = projectTitle;

            if (_isQualitativeDeductive)
            {
                PanelQualitativeDeductive.Visibility = Visibility.Visible;
            }
            else
            {
                PanelQualitativeDeductive.Visibility = Visibility.Collapsed;
            }
            
            // Initialisation de la liste
            Variables = new ObservableCollection<StudyVariable>(existingVariables ?? new List<StudyVariable>());
            VariablesListBox.ItemsSource = Variables; 

            // Configurer le regroupement
            System.Windows.Data.CollectionView view = (System.Windows.Data.CollectionView)System.Windows.Data.CollectionViewSource.GetDefaultView(VariablesListBox.ItemsSource);
            if (view != null)
            {
                view.GroupDescriptions.Clear();
                view.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("GroupName"));
            }

            // Remplir le ComboBox des types
            CmbType.ItemsSource = Enum.GetValues(typeof(VariableType));
            
            UpdatePreview(); // Premier aperçu
        }

        private void BtnAddVariable_Click(object sender, RoutedEventArgs e)
        {
            // Trouver le prochain numéro disponible Q1, Q2...
            int nextNum = 1;
            var qVars = Variables.Where(v => v.Name.StartsWith("Q") && int.TryParse(v.Name.Substring(1), out _))
                                 .Select(v => int.Parse(v.Name.Substring(1)))
                                 .ToList();
            
            if (qVars.Any())
            {
                nextNum = qVars.Max() + 1;
            }

            // Créer une nouvelle variable par défaut
            var newVar = new StudyVariable
            {
                Prompt = "Nouvelle variable à définir",
                Name = $"Q{nextNum}",
                Type = VariableType.Text,
                GroupName = "SECTION 1" // Groupe par défaut
            };

            Variables.Add(newVar);
            VariablesListBox.SelectedItem = newVar; 
            VariablesListBox.ScrollIntoView(newVar);
            UpdatePreview();
        }

        private void BtnDeleteVariable_Click(object sender, RoutedEventArgs e)
        {
            if (VariablesListBox.SelectedItem is StudyVariable selected)
            {
                if (MessageBox.Show($"Voulez-vous vraiment supprimer '{selected.Prompt}' ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Variables.Remove(selected);
                    UpdatePreview();
                }
            }
        }

        // Mise à jour de l'aperçu en temps réel
        private void UpdatePreview()
        {
            if (TxtPreview == null) return;

            try
            {
                var tempWithRef = new AdRev.Domain.Protocols.ResearchProtocol
                {
                    Title = TxtProjectTitle.Text,
                    Variables = new List<StudyVariable>(Variables)
                };

                var gen = new AdRev.Core.Services.QuestionnaireGenerator();
                TxtPreview.Text = gen.GenerateMarkdownQuestionnaire(tempWithRef);
            }
            catch
            {
                // Ignorer erreurs pendant la frappe
            }
        }

        // Quand on sélectionne une variable dans la liste
        private void VariablesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VariablesListBox.SelectedItem is StudyVariable selected)
            {
                _currentVariable = selected;
                EditorPanel.IsEnabled = true;
                EditorPanel.DataContext = selected;
                _isUpdating = true; 

                // Remplir le formulaire
                TxtPrompt.Text = selected.Prompt;
                TxtName.Text = selected.Name;
                CmbType.SelectedItem = selected.Type;
                TxtGroup.Text = selected.GroupName;
                TxtCondition.Text = selected.VisibilityCondition; 
                
                TxtChoices.Text = selected.ChoiceOptions;
                TxtSkipLogic.Text = selected.SkipLogic;         
                TxtFormula.Text = selected.CalculationFormula;   

                TxtMin.Text = selected.MinValue?.ToString();
                TxtMax.Text = selected.MaxValue?.ToString();
                TxtTheme.Text = selected.Theme;
                TxtSubTheme.Text = selected.SubTheme;
                ChkRequired.IsChecked = selected.IsRequired;
                
                UpdateOptionPanelsVisibility(selected.Type);

                _isUpdating = false;
            }
            else
            {
                EditorPanel.IsEnabled = false;
                _currentVariable = null;
            }
        }

        private void TxtPrompt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating || _currentVariable == null) return;
            _currentVariable.Prompt = TxtPrompt.Text;
            
            if (string.IsNullOrWhiteSpace(TxtName.Text) || TxtName.Text.StartsWith("VAR_"))
            {
                string suggestedName = GenerateSlug(TxtPrompt.Text);
                if (suggestedName.Length > 12) suggestedName = suggestedName.Substring(0, 12);
                TxtName.Text = suggestedName.ToUpper();
                if (_currentVariable != null) _currentVariable.Name = TxtName.Text;
            }
            UpdatePreview();
        }

        private void TxtGroup_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating || _currentVariable == null) return;
            _currentVariable.GroupName = TxtGroup.Text;
            
            System.Windows.Data.CollectionView view = (System.Windows.Data.CollectionView)System.Windows.Data.CollectionViewSource.GetDefaultView(VariablesListBox.ItemsSource);
            view?.Refresh();
            
            UpdatePreview();
        }

        private void CmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _currentVariable == null) return;
            
            if (CmbType.SelectedItem is VariableType type)
            {
                _currentVariable.Type = type;
                UpdateOptionPanelsVisibility(type);
                UpdatePreview();
            }
        }

        private void TxtTheme_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating || _currentVariable == null) return;
            _currentVariable.Theme = TxtTheme.Text;
            UpdatePreview();
        }

        private void TxtSubTheme_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating || _currentVariable == null) return;
            _currentVariable.SubTheme = TxtSubTheme.Text;
            UpdatePreview();
        }

        private void UpdateOptionPanelsVisibility(VariableType type)
        {
            PanelChoices.Visibility = Visibility.Collapsed;
            PanelNumeric.Visibility = Visibility.Collapsed;
            PanelCalculated.Visibility = Visibility.Collapsed;

            switch (type)
            {
                case VariableType.QualitativeNominal:
                case VariableType.QualitativeOrdinal:
                case VariableType.MultipleChoice:
                case VariableType.QualitativeBinary: 
                    PanelChoices.Visibility = Visibility.Visible;
                    break;
                case VariableType.QuantitativeDiscrete:
                case VariableType.QuantitativeContinuous:
                    PanelNumeric.Visibility = Visibility.Visible;
                    break;
                case VariableType.Calculated:
                    PanelCalculated.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void TxtChoices_TextChanged(object sender, TextChangedEventArgs e)
        {
             if (_isUpdating || _currentVariable == null) return;
             _currentVariable.ChoiceOptions = TxtChoices.Text;
             UpdatePreview();
        }

        private void TxtSkipLogic_TextChanged(object sender, TextChangedEventArgs e)
        {
             if (_isUpdating || _currentVariable == null) return;
             _currentVariable.SkipLogic = TxtSkipLogic.Text;
             UpdatePreview();
        }

        private void TxtFormula_TextChanged(object sender, TextChangedEventArgs e)
        {
             if (_isUpdating || _currentVariable == null) return;
             _currentVariable.CalculationFormula = TxtFormula.Text;
             UpdatePreview();
        }

        private void TxtMin_TextChanged(object sender, TextChangedEventArgs e)
        {
             if (_isUpdating || _currentVariable == null) return;
             if (double.TryParse(TxtMin.Text, out double val)) _currentVariable.MinValue = val;
             else _currentVariable.MinValue = null;
             UpdatePreview();
        }

        private void TxtMax_TextChanged(object sender, TextChangedEventArgs e)
        {
             if (_isUpdating || _currentVariable == null) return;
             if (double.TryParse(TxtMax.Text, out double val)) _currentVariable.MaxValue = val;
             else _currentVariable.MaxValue = null;
             UpdatePreview();
        }

        private void ChkRequired_Click(object sender, RoutedEventArgs e)
        {
             if (_isUpdating || _currentVariable == null) return;
             _currentVariable.IsRequired = ChkRequired.IsChecked ?? false;
             UpdatePreview();
        }

        private void TxtCondition_TextChanged(object sender, TextChangedEventArgs e)
        {
             if (_isUpdating || _currentVariable == null) return;
             _currentVariable.VisibilityCondition = TxtCondition.Text;
             UpdatePreview();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private string GenerateSlug(string text)
        {
             if (string.IsNullOrEmpty(text)) return "VAR";
             string normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
             var sb = new System.Text.StringBuilder();
             foreach (var c in normalizedString)
             {
                 if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                     sb.Append(c);
             }
             string cleanText = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
             var final = new string(cleanText.Where(c => char.IsLetterOrDigit(c)).ToArray());
             return string.IsNullOrWhiteSpace(final) ? "VAR" : final.ToUpper();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (Variables.Count == 0)
            {
                MessageBox.Show("Veuillez d'abord ajouter des variables.", "Export Impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var tempProtocol = new AdRev.Domain.Protocols.ResearchProtocol
                {
                    Title = TxtProjectTitle.Text,
                    Variables = new List<StudyVariable>(Variables)
                };

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Document Word (*.docx)|*.docx",
                    FileName = $"Fiche_Enquete_{DateTime.Now:yyyyMMdd_HHmmss}.docx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var exportService = new Services.WordExportService();
                    exportService.ExportVariableSheetToWord(tempProtocol, saveFileDialog.FileName);

                    MessageBox.Show("Fiche de conception exportée avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'export Word : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRecode_Click(object sender, RoutedEventArgs e)
        {
            if (_currentVariable == null) return;

            var dialog = new Windows.RecodeDialog(_currentVariable.RecodingInstructions);
            if (dialog.ShowDialog() == true)
            {
                _currentVariable.RecodingInstructions = dialog.ResultInstructions;
                MessageBox.Show("Règles de recodage enregistrées.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
