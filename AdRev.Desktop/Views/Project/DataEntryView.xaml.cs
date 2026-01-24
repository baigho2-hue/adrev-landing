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

        public DataEntryView()
        {
            InitializeComponent();
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
            // In a real app, load from project data
            TxtRecordCount.Text = "0 entrée(s) enregistrée(s)";
        }

        private void GenerateDataEntryForm()
        {
            DataEntryFormPanel.Children.Clear();

            if (_project != null && (_project.StudyType == StudyType.Qualitative || _project.StudyType == StudyType.Mixed))
            {
                RenderAudioRecorder();
            }

            if (_project == null || _project.Variables.Count == 0)
            {
                DataEntryFormPanel.Children.Add(new TextBlock 
                { 
                    Text = "Aucune variable définie. Utilisez le 'Concepteur de Variables' pour créer votre formulaire.",
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic
                });
                return;
            }

            var groups = _project.Variables.GroupBy(v => v.GroupName);

            foreach (var group in groups)
            {
                var groupBorder = new Border {
                    Background = new SolidColorBrush(Color.FromRgb(245, 247, 249)),
                    Padding = new Thickness(15, 10, 15, 10),
                    Margin = new Thickness(0, 15, 0, 15),
                    CornerRadius = new CornerRadius(6),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(224, 230, 237)),
                    BorderThickness = new Thickness(1)
                };
                
                groupBorder.Child = new TextBlock { 
                    Text = string.IsNullOrWhiteSpace(group.Key) ? "Informations Générales" : group.Key, 
                    FontWeight = FontWeights.Bold, 
                    Foreground = new SolidColorBrush(Color.FromRgb(30, 136, 229)),
                    FontSize = 14
                };
                
                DataEntryFormPanel.Children.Add(groupBorder);

                foreach (var variable in group)
                {
                    var fieldPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 20) };
                    var labelText = variable.Prompt;
                    if (variable.IsRequired) labelText += " *";
                    
                    var label = new TextBlock
                    {
                        Text = labelText,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = new SolidColorBrush(Color.FromRgb(55, 71, 79)),
                        Margin = new Thickness(0, 0, 0, 5),
                        FontSize = 13
                    };
                    fieldPanel.Children.Add(label);

                    UIElement? inputControl = null;
                    switch (variable.Type)
                    {
                        case VariableType.Text:
                            inputControl = new TextBox { Padding = new Thickness(5), Height = 30 };
                            break;
                        case VariableType.Memo:
                            inputControl = new TextBox { Padding = new Thickness(5), Height = 80, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true };
                            break;
                        case VariableType.QualitativeBinary:
                            var spYesNo = new StackPanel { Orientation = Orientation.Horizontal };
                            spYesNo.Children.Add(new RadioButton { Content = "Oui", Margin = new Thickness(0,0,15,0) });
                            spYesNo.Children.Add(new RadioButton { Content = "Non" });
                            inputControl = spYesNo;
                            break;
                        // Add more cases from the original code
                        default:
                            inputControl = new TextBox { Height = 30 };
                            break;
                    }
                    
                    if (inputControl != null)
                        fieldPanel.Children.Add(inputControl);

                    DataEntryFormPanel.Children.Add(fieldPanel);
                }
            }
        }

        private void RenderAudioRecorder()
        {
            var recorderBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 243, 224)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
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
            titleStack.Children.Add(new TextBlock { Text = "Dictaphone Numérique", FontWeight = FontWeights.Bold, FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0)) });
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
            TxtRecordName.Text = string.Empty;
            GenerateDataEntryForm();
        }

        private void BtnSaveRecord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtRecordName.Text)) return;
            var record = new { RecordName = TxtRecordName.Text, Timestamp = DateTime.Now.ToString() };
            RecordsListBox.Items.Insert(0, record);
            TxtRecordCount.Text = $"{RecordsListBox.Items.Count} entrée(s) enregistrée(s)";
            TxtRecordName.Text = string.Empty;
        }

        private void TxtSearchRecords_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Simple filtering logic
        }

        private void RecordsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Load selected record
        }

        private void OpenVariableDesigner_Click(object sender, RoutedEventArgs e)
        {
            // Potentially open VariableDesignView or a designer window
        }

        private void CloseDataEntryMap_Click(object sender, RoutedEventArgs e)
        {
            MapPanel.Visibility = Visibility.Collapsed;
        }
    }
}
