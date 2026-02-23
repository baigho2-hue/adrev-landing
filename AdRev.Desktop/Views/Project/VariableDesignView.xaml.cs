using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AdRev.Domain.Enums;
using AdRev.Domain.Variables;
using AdRev.Domain.Models;

namespace AdRev.Desktop.Views.Project
{
    public partial class VariableDesignView : UserControl
    {
        public ObservableCollection<StudyVariable> Variables { get; private set; } = new ObservableCollection<StudyVariable>();
        private StudyVariable? _selectedVariable;
        private bool _isQualitativeDeductive;
        private ResearchProject? _project;

        public VariableDesignView()
        {
            InitializeComponent();
            VariablesListBox.ItemsSource = Variables;
            CmbType.ItemsSource = Enum.GetValues(typeof(VariableType));
            UpdateOptionPanelsVisibility();
        }

        public void LoadProject(ResearchProject project)
        {
            if (project == null) return;
            _project = project;
            LoadVariables(project.Variables ?? new List<StudyVariable>(), project.StudyType == StudyType.Qualitative);
        }

        public void LoadVariables(List<StudyVariable> existingVariables, bool isQualitativeDeductive = false)
        {
            _isQualitativeDeductive = isQualitativeDeductive;
            Variables.Clear();
            if (existingVariables != null)
            {
                foreach (var v in existingVariables) Variables.Add(v);
            }

            if (Variables.Count > 0)
                VariablesListBox.SelectedIndex = 0;
            
            UpdatePreview();
        }

        private void BtnAddVariable_Click(object sender, RoutedEventArgs e)
        {
            var newVar = new StudyVariable
            {
                Name = "VAR_" + (Variables.Count + 1),
                Prompt = "Nouvelle variable à définir",
                Type = VariableType.Text
            };
            Variables.Add(newVar);
            VariablesListBox.SelectedItem = newVar;
        }

        private void BtnOpenFullDesigner_Click(object sender, RoutedEventArgs e)
        {
            var win = new VariableDesignerWindow(Variables.ToList(), _project?.Title ?? "Nouveau Projet", _isQualitativeDeductive);
            if (win.ShowDialog() == true)
            {
                var newVars = win.Variables.ToList();
                if (_project != null) _project.Variables = newVars;
                
                // Diffuser la mise à jour à toutes les vues (dont celle-ci)
                var parent = Window.GetWindow(this) as AdRev.Desktop.Windows.ProjectWindow;
                parent?.SyncVariablesToAllViews();
            }
        }

        private void BtnDeleteVariable_Click(object sender, RoutedEventArgs e)
        {
            if (VariablesListBox.SelectedItem is StudyVariable selected)
            {
                Variables.Remove(selected);
                UpdatePreview();
            }
        }

        private void VariablesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedVariable = VariablesListBox.SelectedItem as StudyVariable;
            EditorPanel.IsEnabled = (_selectedVariable != null);

            if (_selectedVariable != null)
            {
                TxtPrompt.Text = _selectedVariable.Prompt;
                TxtName.Text = _selectedVariable.Name;
                CmbType.SelectedItem = _selectedVariable.Type;
                TxtGroup.Text = _selectedVariable.GroupName;
                ChkRequired.IsChecked = _selectedVariable.IsRequired;
                TxtCondition.Text = _selectedVariable.VisibilityCondition;
                TxtSkipLogic.Text = _selectedVariable.SkipLogic;
                TxtChoices.Text = _selectedVariable.ChoiceOptions;
                TxtMin.Text = _selectedVariable.MinValue?.ToString() ?? "";
                TxtMax.Text = _selectedVariable.MaxValue?.ToString() ?? "";
                TxtFormula.Text = _selectedVariable.CalculationFormula;
                TxtTheme.Text = _selectedVariable.Theme;
                TxtSubTheme.Text = _selectedVariable.SubTheme;

                UpdateOptionPanelsVisibility();
            }
            UpdatePreview();
        }

        private void UpdateOptionPanelsVisibility()
        {
            if (PanelChoices == null) return;

            PanelChoices.Visibility = Visibility.Collapsed;
            PanelNumeric.Visibility = Visibility.Collapsed;
            PanelCalculated.Visibility = Visibility.Collapsed;
            PanelQualitativeDeductive.Visibility = Visibility.Collapsed;

            if (_selectedVariable == null) return;

            switch (_selectedVariable.Type)
            {
                case VariableType.QualitativeNominal:
                case VariableType.MultipleChoice:
                case VariableType.QualitativeBinary:
                    PanelChoices.Visibility = Visibility.Visible;
                    break;
                case VariableType.QuantitativeContinuous:
                case VariableType.QuantitativeDiscrete:
                    PanelNumeric.Visibility = Visibility.Visible;
                    break;
                case VariableType.Calculated:
                    PanelCalculated.Visibility = Visibility.Visible;
                    break;
                case VariableType.Memo:
                    if (_isQualitativeDeductive)
                        PanelQualitativeDeductive.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void UpdatePreview()
        {
            if (TxtPreview == null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("DICTIONNAIRE DE VARIABLES");
            sb.AppendLine("=========================");
            sb.AppendLine();

            foreach (var v in Variables)
            {
                sb.AppendLine($"{v.Name} : {v.Prompt}");
                sb.AppendLine($"  Type: {v.Type}");
                if (!string.IsNullOrEmpty(v.GroupName)) sb.AppendLine($"  Groupe: {v.GroupName}");
                if (!string.IsNullOrEmpty(v.ChoiceOptions)) sb.AppendLine($"  Options: {v.ChoiceOptions}");
                sb.AppendLine();
            }

            TxtPreview.Text = sb.ToString();
        }

        private void TxtPrompt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null) { _selectedVariable.Prompt = TxtPrompt.Text; UpdatePreview(); }
        }

        private void TxtGroup_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null) { _selectedVariable.GroupName = TxtGroup.Text; UpdatePreview(); }
        }

        private void CmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedVariable != null && CmbType.SelectedItem is VariableType type)
            {
                _selectedVariable.Type = type;
                UpdateOptionPanelsVisibility();
                UpdatePreview();
            }
        }

        private void TxtChoices_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null)
            {
                _selectedVariable.ChoiceOptions = TxtChoices.Text;
                UpdatePreview();
            }
        }

        private void TxtMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null && double.TryParse(TxtMin.Text, out double min)) _selectedVariable.MinValue = min;
        }

        private void TxtMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null && double.TryParse(TxtMax.Text, out double max)) _selectedVariable.MaxValue = max;
        }

        private void ChkRequired_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVariable != null) _selectedVariable.IsRequired = ChkRequired.IsChecked ?? false;
        }

        private void TxtCondition_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null) _selectedVariable.VisibilityCondition = TxtCondition.Text;
        }

        private void TxtSkipLogic_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null) _selectedVariable.SkipLogic = TxtSkipLogic.Text;
        }

        private void TxtFormula_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null) _selectedVariable.CalculationFormula = TxtFormula.Text;
        }

        private void TxtTheme_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null) _selectedVariable.Theme = TxtTheme.Text;
        }

        private void TxtSubTheme_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedVariable != null) _selectedVariable.SubTheme = TxtSubTheme.Text;
        }
    }
}
