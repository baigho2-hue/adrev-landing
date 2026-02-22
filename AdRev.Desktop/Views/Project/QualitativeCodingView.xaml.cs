using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using AdRev.Domain.Models;
using AdRev.Domain.Variables;
using AdRev.Domain.Enums;
using Microsoft.Win32;

namespace AdRev.Desktop.Views.Project
{
    public partial class QualitativeCodingView : UserControl
    {
        private ResearchProject? _project;

        public QualitativeCodingView()
        {
            InitializeComponent();
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
            UpdateQualitativeSourcesUI();
        }

        private void UpdateQualitativeSourcesUI()
        {
            SourceFilesList.Items.Clear();
            if (_project == null) return;

            foreach (var source in _project.QualitativeSources)
            {
                var item = new ListBoxItem
                {
                    Content = source.Name,
                    Tag = source
                };
                SourceFilesList.Items.Add(item);
            }
        }

        private void ImportFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_project == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Tous les fichiers|*.*|Documents Word|*.docx;*.doc|Fichiers Texte|*.txt|Images|*.jpg;*.png;*.bmp|Vidéos|*.mp4;*.avi;*.mov"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    _project.QualitativeSources.Add(new QualitativeSource
                    {
                        Name = System.IO.Path.GetFileName(filename),
                        FilePath = filename,
                        Type = System.IO.Path.GetExtension(filename).TrimStart('.').ToUpper(),
                        ImportDate = DateTime.Now
                    });
                }
                UpdateQualitativeSourcesUI();
            }
        }

        private void ImportFolder_Click(object sender, RoutedEventArgs e)
        {
            // Implementation similar to MainWindow
        }

        private void SourceFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SourceFilesList.SelectedItem is ListBoxItem item && item.Tag is QualitativeSource source)
            {
                CurrentFileNameTextBlock.Text = source.Name;
                DisplaySource(source);
            }
        }

        private void DisplaySource(QualitativeSource source)
        {
            CodingRichTextBox.Visibility = Visibility.Collapsed;
            CodingImageHost.Visibility = Visibility.Collapsed;
            CodingVideoHost.Visibility = Visibility.Collapsed;
            MediaControlsPanel.Visibility = Visibility.Collapsed;

            string ext = System.IO.Path.GetExtension(source.FilePath).ToLower();

            if (ext == ".jpg" || ext == ".png" || ext == ".bmp")
            {
                CodingImageHost.Visibility = Visibility.Visible;
                CodingImageHost.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(source.FilePath));
            }
            else if (ext == ".mp4" || ext == ".avi" || ext == ".mov")
            {
                CodingVideoHost.Visibility = Visibility.Visible;
                MediaControlsPanel.Visibility = Visibility.Visible;
                CodingMediaElement.Source = new Uri(source.FilePath);
                CodingMediaElement.Play();
            }
            else
            {
                CodingRichTextBox.Visibility = Visibility.Visible;
                if (System.IO.File.Exists(source.FilePath))
                {
                    string text = System.IO.File.ReadAllText(source.FilePath);
                    CodingRichTextBox.Document.Blocks.Clear();
                    CodingRichTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
                }
            }
        }

        private void MediaPlay_Click(object sender, RoutedEventArgs e) => CodingMediaElement.Play();
        private void MediaPause_Click(object sender, RoutedEventArgs e) => CodingMediaElement.Pause();
        private void MediaStop_Click(object sender, RoutedEventArgs e) => CodingMediaElement.Stop();

        private void ShowNewCodePanel_Click(object sender, RoutedEventArgs e)
        {
            NewCodeThemeComboBox.Items.Clear();
            NewCodeInputPanel.Visibility = Visibility.Visible;
            BtnNewCode.Visibility = Visibility.Collapsed;
        }

        private void CancelNewCode_Click(object sender, RoutedEventArgs e)
        {
            NewCodeInputPanel.Visibility = Visibility.Collapsed;
            BtnNewCode.Visibility = Visibility.Visible;
        }

        private void ConfirmNewCode_Click(object sender, RoutedEventArgs e)
        {
            // Logic to add code to project and UI
            NewCodeInputPanel.Visibility = Visibility.Collapsed;
            BtnNewCode.Visibility = Visibility.Visible;
        }

        private void NewInterview_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void NewFocusGroup_Click(object sender, RoutedEventArgs e) { /* ... */ }
    }
}
