using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;
using AdRev.Domain.Enums;
using AdRev.Domain.Variables;
using AdRev.Domain.Models;
using Microsoft.Win32;
using AdRev.Desktop.Helpers;

namespace AdRev.Desktop.Views.Project
{
    public partial class DataEntryView : UserControl
    {
        [DllImport("winmm.dll")]
        private static extern long record(string command, string returnString, int returnLength, int hwndCallback);

        private ResearchProject? _project;
        private bool _isRecording = false;
        private System.Windows.Threading.DispatcherTimer? _recordingTimer;
        private DateTime _recordingStartTime;
        private TextBlock? _recordingStatusText;
        private TextBlock? _recordingTimerText;
        
        // Auto-Save & Validation
        private System.Windows.Threading.DispatcherTimer? _autoSaveTimer;
        private bool _isDirty = false;
        private readonly AdRev.Core.Services.AuditService _auditService = new AdRev.Core.Services.AuditService(new AdRev.Core.Services.ResearcherProfileService());

        // Local Storage Model for Demo Purpose
        public class DataEntryRecord
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string RecordName { get; set; } = string.Empty;
            public string Timestamp { get; set; } = string.Empty;
            public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
        }

        private Dictionary<string, UIElement> _controlMap = new Dictionary<string, UIElement>();
        private DataEntryRecord? _currentEditingRecord = null;

        public DataEntryView()
        {
            InitializeComponent();
            SetupAutoSave();
        }

        private void SetupAutoSave()
        {
            _autoSaveTimer = new System.Windows.Threading.DispatcherTimer();
            _autoSaveTimer.Interval = TimeSpan.FromSeconds(30); // Auto-save every 30s
            _autoSaveTimer.Tick += AutoSave_Tick;
            _autoSaveTimer.Start();
        }

        private async void AutoSave_Tick(object? sender, EventArgs e)
        {
            if (_isDirty && _currentEditingRecord != null)
            {
                await SaveRecordInternal(true);
            }
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
            GenerateDataEntryForm();
            LoadRecords();
        }

        private void LoadRecords()
        {
            RecordsListBox.Items.Clear();
            
            if (_project != null && _project.DataRows != null)
            {
                foreach (var row in _project.DataRows)
                {
                    // Basic mapping back to DataEntryRecord for UI
                    // Note: We need a unique ID or just use index. For now simple mapping.
                    var recordName = row.ContainsKey("RecordName") ? row["RecordName"].ToString() : "Sans Nom";
                    var timestamp = row.ContainsKey("Timestamp") ? row["Timestamp"].ToString() : "";
                    
                    var entry = new DataEntryRecord 
                    { 
                        RecordName = recordName!,
                        Timestamp = timestamp!,
                        Values = row
                    };
                    RecordsListBox.Items.Add(entry);
                }
                TxtRecordCount.Text = $"{RecordsListBox.Items.Count} entrée(s) enregistrée(s)";
            }
        }

        private void GenerateDataEntryForm()
        {
            DataEntryFormPanel.Children.Clear();
            _controlMap.Clear();

            if (_project != null && (_project.StudyType == StudyType.Qualitative || _project.StudyType == StudyType.Mixed))
            {
                RenderAudioRecorder();
            }

            if (_project == null || _project.Variables.Count == 0)
            {
                DataEntryFormPanel.Children.Add(new TextBlock 
                { 
                    Text = "Aucune variable définie. Utilisez le 'Concepteur de Variables' pour créer votre formulaire.",
                    Foreground = (Brush)Application.Current.FindResource("MaterialDesignBody"),
                    FontStyle = FontStyles.Italic
                });
                return;
            }

            var groups = _project.Variables.GroupBy(v => v.GroupName);

            foreach (var group in groups)
            {
                // Group Header
                var groupCard = new MaterialDesignThemes.Wpf.Card
                {
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 8, 0, 16),
                    UniformCornerRadius = 6
                };

                var groupStack = new StackPanel();
                groupCard.Content = groupStack;

                groupStack.Children.Add(new TextBlock { 
                    Text = string.IsNullOrWhiteSpace(group.Key) ? "Informations Générales" : group.Key, 
                    Style = (Style)Application.Current.FindResource("MaterialDesignTitleMedium"),
                    Margin = new Thickness(0, 0, 0, 16),
                    Foreground = (Brush)Application.Current.FindResource("PrimaryHueMidBrush")
                });
                
                DataEntryFormPanel.Children.Add(groupCard);

                foreach (var variable in group)
                {
                    var fieldPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
                    
                    var labelText = variable.Prompt;
                    if (variable.IsRequired) labelText += " *";
                    
                    bool useFloatingLabel = true;
                    UIElement? inputControl = null;

                    switch (variable.Type)
                    {
                        case VariableType.Text:
                        case VariableType.QuantitativeDiscrete:
                        case VariableType.QuantitativeContinuous:
                             var tb = new TextBox { 
                                 
                             };
                             MaterialDesignThemes.Wpf.HintAssist.SetHint(tb, labelText);
                             tb.Style = (Style)Application.Current.FindResource("MaterialDesignFloatingHintTextBox");
                             inputControl = tb;
                             break;

                        case VariableType.Memo:
                             var memo = new TextBox { 
                                 TextWrapping = TextWrapping.Wrap, 
                                 AcceptsReturn = true, 
                                 VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                                 Height = 80
                             };
                             MaterialDesignThemes.Wpf.HintAssist.SetHint(memo, labelText);
                             memo.Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedTextBox"); 
                             inputControl = memo;
                             break;

                        case VariableType.QualitativeBinary:
                            useFloatingLabel = false; // Radio buttons need explicit label
                            var spYesNo = new StackPanel { Orientation = Orientation.Horizontal };
                            var rbYes = new RadioButton { Content = "Oui", Margin = new Thickness(0,0,16,0), GroupName = variable.Name + "_" + Guid.NewGuid() };
                            var rbNo = new RadioButton { Content = "Non", GroupName = rbYes.GroupName };
                            spYesNo.Children.Add(rbYes);
                            spYesNo.Children.Add(rbNo);
                            inputControl = spYesNo;
                            break;

                        case VariableType.QualitativeNominal:
                        case VariableType.QualitativeOrdinal:
                            var comboBox = new ComboBox { IsEditable = false };
                            MaterialDesignThemes.Wpf.HintAssist.SetHint(comboBox, labelText);
                            comboBox.Style = (Style)Application.Current.FindResource("MaterialDesignFloatingHintComboBox");
                            
                            if (!string.IsNullOrWhiteSpace(variable.ChoiceOptions))
                            {
                                var options = variable.ChoiceOptions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var opt in options)
                                {
                                    comboBox.Items.Add(opt.Trim());
                                }
                            }
                            else 
                            { 
                                comboBox.Items.Add("Aucune option définie");
                                comboBox.IsEnabled = false;
                            }
                            inputControl = comboBox;
                            break;

                        case VariableType.MultipleChoice:
                            useFloatingLabel = false;
                            var spMulti = new StackPanel();
                            if (!string.IsNullOrWhiteSpace(variable.ChoiceOptions))
                            {
                                var options = variable.ChoiceOptions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var opt in options)
                                {
                                    spMulti.Children.Add(new CheckBox { Content = opt.Trim(), Margin = new Thickness(0, 4, 0, 4) });
                                }
                            }
                            else 
                            { 
                                spMulti.Children.Add(new TextBlock { Text = "Aucune option définie", FontStyle = FontStyles.Italic, Foreground = Brushes.Gray });
                            }
                            inputControl = spMulti;
                            break;

                        case VariableType.QuantitativeTemporal:
                            var dp = new DatePicker { };
                            MaterialDesignThemes.Wpf.HintAssist.SetHint(dp, labelText);
                            dp.Style = (Style)Application.Current.FindResource("MaterialDesignFloatingHintDatePicker");
                            inputControl = dp;
                            break;

                        case VariableType.LabelOnly:
                             useFloatingLabel = false;
                             inputControl = new Separator { Margin = new Thickness(0, 8, 0, 8) };
                             break;

                        default:
                            var defTb = new TextBox { };
                            MaterialDesignThemes.Wpf.HintAssist.SetHint(defTb, labelText);
                            defTb.Style = (Style)Application.Current.FindResource("MaterialDesignFloatingHintTextBox");
                            inputControl = defTb;
                            break;
                    }
                    
                    // Add Label if NOT floating
                    if (!useFloatingLabel && variable.Type != VariableType.LabelOnly)
                    {
                        var label = new TextBlock
                        {
                            Text = labelText,
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 0, 0, 8),
                            Opacity = 0.7
                        };
                        fieldPanel.Children.Add(label);
                    }

                    if (inputControl != null)
                    {
                        fieldPanel.Children.Add(inputControl);
                        // Register control for mapping
                        if (!_controlMap.ContainsKey(variable.Name))
                            _controlMap[variable.Name] = inputControl;

                        // Hook events for Dirty tracking
                        if (inputControl is TextBox t) t.TextChanged += (s, e) => _isDirty = true;
                        if (inputControl is ComboBox c) c.SelectionChanged += (s, e) => _isDirty = true;
                        if (inputControl is DatePicker d) d.SelectedDateChanged += (s, e) => _isDirty = true;
                        if (inputControl is StackPanel s) 
                        {
                            foreach(var child in s.Children)
                            {
                                if (child is RadioButton r) r.Checked += (x, y) => _isDirty = true;
                                if (child is CheckBox k) k.Checked += (x, y) => _isDirty = true;
                                if (child is CheckBox k2) k2.Unchecked += (x, y) => _isDirty = true;
                            }
                        }
                    }

                    groupStack.Children.Add(fieldPanel);
                }
            }
        }

        private void RenderAudioRecorder()
        {
            var recorderBorder = new Border
            {
                Background = (Brush)Application.Current.FindResource("RecorderBackgroundBrush"),
                BorderBrush = (Brush)Application.Current.FindResource("RecorderBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerStack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15) };
            headerStack.Children.Add(new TextBlock { Text = "🎙️", FontSize = 24, Margin = new Thickness(0, 0, 10, 0) });
            var titleStack = new StackPanel();
            titleStack.Children.Add(new TextBlock { Text = "Dictaphone Numérique", FontWeight = FontWeights.Bold, FontSize = 16, Foreground = (Brush)Application.Current.FindResource("RecorderForegroundBrush") });
            headerStack.Children.Add(titleStack);
            grid.Children.Add(headerStack);

            var controlsStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            Grid.SetRow(controlsStack, 1);
            
            var btnRecord = new Button { Content = "Enregistrer", Padding = new Thickness(15, 8, 15, 8) };
            _recordingStatusText = new TextBlock { Text = "Prêt", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10,0,0,0) };
            _recordingTimerText = new TextBlock { Text = "00:00", VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Collapsed, Margin = new Thickness(10,0,0,0) };

            btnRecord.Click += (s, e) => ToggleRecording(btnRecord);

            controlsStack.Children.Add(btnRecord);
            controlsStack.Children.Add(_recordingStatusText);
            controlsStack.Children.Add(_recordingTimerText);

            grid.Children.Add(controlsStack);
            recorderBorder.Child = grid;

            DataEntryFormPanel.Children.Add(recorderBorder);
        }

        private void ToggleRecording(Button btn)
        {
            if (!_isRecording)
            {
                record("open new type waveaudio alias recsound", "", 0, 0);
                record("record recsound", "", 0, 0);
                _isRecording = true;
                _recordingStartTime = DateTime.Now;
                btn.Content = "Arrêter & Sauvegarder";
                _recordingStatusText!.Text = "Enregistrement...";
                _recordingTimerText!.Visibility = Visibility.Visible;
                
                if (_recordingTimer == null)
                {
                    _recordingTimer = new System.Windows.Threading.DispatcherTimer();
                    _recordingTimer.Interval = TimeSpan.FromSeconds(1);
                    _recordingTimer.Tick += (s, args) => {
                        var ts = DateTime.Now - _recordingStartTime;
                        _recordingTimerText.Text = $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00}";
                    };
                }
                _recordingTimer.Start();
            }
            else
            {
                var saveDialog = new SaveFileDialog { Filter = "Fichier Audio (*.wav)|*.wav" };
                if (saveDialog.ShowDialog() == true)
                {
                    record("save recsound " + saveDialog.FileName, "", 0, 0);
                    record("close recsound", "", 0, 0);
                }
                else record("close recsound", "", 0, 0);

                _isRecording = false;
                _recordingTimer?.Stop();
                btn.Content = "Enregistrer";
                _recordingStatusText!.Text = "Prêt";
                _recordingTimerText!.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnNewRecord_Click(object sender, RoutedEventArgs e)
        {
            _currentEditingRecord = null;
            TxtRecordName.Text = string.Empty;
            BtnSaveRecordActual.Content = "💾 Sauvegarder l'entrée";
            
            // Clear all fields
            foreach (var kvp in _controlMap) CleanControl(kvp.Value);
            
            RecordsListBox.SelectedItem = null;
            _isDirty = false;
        }

        private void CleanControl(UIElement control)
        {
            if (control is TextBox tb) tb.Text = "";
            else if (control is ComboBox cb) cb.SelectedIndex = -1;
            else if (control is DatePicker dp) dp.SelectedDate = null;
            else if (control is StackPanel sp) // Radio or Checkbox groups
            {
                foreach (var child in sp.Children)
                {
                    if (child is RadioButton rb) rb.IsChecked = false;
                    if (child is CheckBox chk) chk.IsChecked = false;
                }
            }
        }

        private async void BtnSaveRecord_Click(object sender, RoutedEventArgs e)
        {
            await SaveRecordInternal(false);
        }

        private async Task SaveRecordInternal(bool isAutoSave)
        {
            if (string.IsNullOrWhiteSpace(TxtRecordName.Text)) 
            {
                if (!isAutoSave) await DialogHelper.ShowWarning("Veuillez donner un nom à cet enregistrement.", "Nom manquant");
                return;
            }

            // 1. Validation
            var missingFields = new List<string>();
            foreach (var kvp in _controlMap)
            {
                var variable = _project?.Variables.FirstOrDefault(v => v.Name == kvp.Key);
                if (variable?.IsRequired == true)
                {
                    var val = GetValueFromControl(kvp.Value)?.ToString();
                    if (string.IsNullOrWhiteSpace(val)) missingFields.Add(variable.Prompt);
                }
            }

            if (missingFields.Any())
            {
                if (!isAutoSave) 
                {
                    await DialogHelper.ShowWarning($"Veuillez remplir les champs obligatoires :\n- {string.Join("\n- ", missingFields)}", "Validation");
                }
                return;
            }

            // Capture Data
            var values = new Dictionary<string, object>();
            foreach (var kvp in _controlMap)
            {
                values[kvp.Key] = GetValueFromControl(kvp.Value);
            }
            
            // Add metadata
            values["RecordName"] = TxtRecordName.Text;
            values["Timestamp"] = DateTime.Now.ToString("g");

            if (_currentEditingRecord != null)
            {
                // Update UI Record
                _currentEditingRecord.RecordName = TxtRecordName.Text;
                _currentEditingRecord.Timestamp = DateTime.Now.ToString("g") + (isAutoSave ? " (Auto)" : " (Modifié)");
                _currentEditingRecord.Values = values;

                // Update Domain Project directly (or via service)
                var existingRow = _project?.DataRows.FirstOrDefault(r => r.ContainsKey("RecordName") && r["RecordName"].ToString() == _currentEditingRecord.RecordName);
                if (existingRow != null)
                {
                    foreach (var k in values.Keys) existingRow[k] = values[k];
                    _auditService.LogAction(_project!, isAutoSave ? "Auto-Update" : "Update", "DataEntryRecord", _currentEditingRecord.RecordName, $"Record '{_currentEditingRecord.RecordName}' updated.");
                }

                // Force refresh list UI
                RecordsListBox.Items.Refresh();
                
                if (!isAutoSave) await DialogHelper.ShowMessage("Enregistrement mis à jour avec succès.", "Succès");
            }
            else
            {
                // Create New
                var newRecord = new DataEntryRecord 
                { 
                    RecordName = TxtRecordName.Text, 
                    Timestamp = DateTime.Now.ToString("g"),
                    Values = values
                };
                RecordsListBox.Items.Insert(0, newRecord);
                TxtRecordCount.Text = $"{RecordsListBox.Items.Count} entrée(s) enregistrée(s)";
                
                // Sync Project DataRows
                SyncProjectDataRows();

                _currentEditingRecord = newRecord;
                RecordsListBox.SelectedItem = newRecord;
                // Sync Project DataRows
                SyncProjectDataRows();

                _auditService.LogAction(_project!, "Create", "DataEntryRecord", TxtRecordName.Text, $"New record '{TxtRecordName.Text}' created.");
                
                if (!isAutoSave) await DialogHelper.ShowMessage("Nouvel enregistrement ajouté.", "Succès");
            }

            _isDirty = false;
        }

        private void SyncProjectDataRows()
        {
            if (_project == null) return;
            _project.DataRows.Clear();
            foreach (DataEntryRecord rec in RecordsListBox.Items)
            {
                _project.DataRows.Add(rec.Values);
            }
        }

        private object GetValueFromControl(UIElement control)
        {
            if (control is TextBox tb) return tb.Text;
            if (control is ComboBox cb) return cb.SelectedItem?.ToString() ?? "";
            if (control is DatePicker dp) return dp.SelectedDate?.ToString("d") ?? "";
            if (control is StackPanel sp)
            {
                // Checkbox List (Multi)
                var checks = sp.Children.OfType<CheckBox>().Where(c => c.IsChecked == true).Select(c => c.Content.ToString()).ToList();
                if (checks.Any()) return string.Join(";", checks);

                // Radio List (Binary)
                var radio = sp.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true);
                return radio?.Content?.ToString() ?? "";
            }
            return "";
        }

        private void SetValueToControl(UIElement control, object value)
        {
            if (value == null) return;
            string valStr = value?.ToString() ?? string.Empty;

            if (control is TextBox tb) tb.Text = valStr;
            else if (control is ComboBox cb) cb.SelectedItem = valStr;
            else if (control is DatePicker dp) 
            {
                if (DateTime.TryParse(valStr, out DateTime d)) dp.SelectedDate = d;
            }
            else if (control is StackPanel sp)
            {
                // Try Radio
                var radio = sp.Children.OfType<RadioButton>().FirstOrDefault(r => r.Content.ToString() == valStr);
                if (radio != null) radio.IsChecked = true;
                
                // Try Checkboxes (split by ;)
                var parts = valStr.Split(';');
                foreach (var chk in sp.Children.OfType<CheckBox>())
                {
                    chk.IsChecked = parts.Contains(chk.Content.ToString());
                }
            }
        }

        private void TxtSearchRecords_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Filter logic would go here
        }

        private void RecordsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecordsListBox.SelectedItem is DataEntryRecord record)
            {
                _currentEditingRecord = record;
                TxtRecordName.Text = record.RecordName;
                BtnSaveRecordActual.Content = "💾 Mettre à jour";

                // Populate Form
                foreach (var kvp in _controlMap)
                {
                    if (record.Values.ContainsKey(kvp.Key))
                    {
                        SetValueToControl(kvp.Value, record.Values[kvp.Key]);
                    }
                    else
                    {
                        CleanControl(kvp.Value);
                    }
                }
                _isDirty = false;
            }
        }

        private async void OpenVariableDesigner_Click(object sender, RoutedEventArgs e)
        {
            if (_project == null)
            {
                 await DialogHelper.ShowWarning("Veuillez d'abord créer ou ouvrir un projet.", "Aucun Projet");
                 return;
            }

            var designer = new VariableDesignerWindow(_project.Variables, _project.Title, 
                                                      _project.StudyType == StudyType.Qualitative || _project.StudyType == StudyType.Mixed);
            designer.Owner = Application.Current.MainWindow;
            
            if (designer.ShowDialog() == true)
            {
                // Sync changes (Additions/Deletions) back to project
                _project.Variables = designer.Variables.ToList();

                // Refresh Form
                GenerateDataEntryForm();
            }
        }

        private void CloseDataEntryMap_Click(object sender, RoutedEventArgs e)
        {
            MapPanel.Visibility = Visibility.Collapsed;
        }
        private async void BtnExportData_Click(object sender, RoutedEventArgs e)
        {
            if (_project == null || _project.DataRows.Count == 0)
            {
                await DialogHelper.ShowWarning("Aucune donnée à exporter.", "Exportation impossible");
                return;
            }

            var dialog = new AdRev.Desktop.Windows.ExportOptionsWindow();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                string format = dialog.SelectedFormat;
                bool anonymize = dialog.Anonymize;

                var saveFile = new Microsoft.Win32.SaveFileDialog();
                saveFile.Filter = format == "Excel" ? "Excel Files (*.xlsx)|*.xlsx" : "CSV Files (*.csv)|*.csv";
                saveFile.FileName = $"Export_{_project.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}";

                if (saveFile.ShowDialog() == true)
                {
                    try
                    {
                        var importService = new AdRev.Core.Services.DataImportService();
                        if (format == "Excel")
                        {
                            importService.ExportToExcel(_project.DataRows, _project.Variables, saveFile.FileName, anonymize);
                        }
                        else
                        {
                            importService.ExportToCsv(_project.DataRows, _project.Variables, saveFile.FileName, anonymize);
                        }

                        _auditService.LogAction(_project, "Export", "Data", format, $"Data exported to {format} (Anonymized: {anonymize})");
                        await DialogHelper.ShowMessage($"Données exportées avec succès vers {saveFile.FileName}", "Exportation réussie");
                    }
                    catch (Exception ex)
                    {
                        await DialogHelper.ShowError($"Erreur lors de l'exportation : {ex.Message}", "Erreur");
                    }
                }
            }
        }
    }
}
