using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using AdRev.Domain.Models;
using AdRev.Core.Services;
using AdRev.Desktop.Windows;

namespace AdRev.Desktop.Views.Project
{
    public partial class LibraryView : UserControl
    {
        private ResearchProject? _project;
        private ObservableCollection<LibraryItemViewModel> _itemViewModels = new ObservableCollection<LibraryItemViewModel>();
        private LibraryItemViewModel? _selectedItem;
        private bool _isLodingDetails = false;
        
        private readonly LibrarySearchService _searchService = new LibrarySearchService();
        private readonly CloudSyncService _cloudService = new CloudSyncService();
        private readonly BibliographicMetadataService _metadataService = new BibliographicMetadataService();

        public LibraryView()
        {
            InitializeComponent();
            LibraryItemsControl.ItemsSource = _itemViewModels;
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                await PdfViewer.EnsureCoreWebView2Async(null);
            }
            catch { }
        }

        public void LoadProject(ResearchProject project)
        {
            _project = project;
            
            Task.Run(() => {
                foreach (var item in _project.LibraryItems)
                {
                    _searchService.IndexItem(_project.StoragePath, item);
                }
            });

            RefreshList();
        }

        private void RefreshList(string filter = "")
        {
            _itemViewModels.Clear();
            
            if (_project == null) return;
            IEnumerable<LibraryItem> items;
            if (string.IsNullOrWhiteSpace(filter))
            {
                items = _project.LibraryItems;
            }
            else
            {
                var matchedIds = _searchService.SearchInLibrary(filter, _project.LibraryItems);
                items = _project.LibraryItems.Where(i => matchedIds.Contains(i.Id));
            }
            
            foreach (var item in items.OrderByDescending(i => i.AddedDate))
            {
                _itemViewModels.Add(new LibraryItemViewModel(item));
            }

            if (_itemViewModels.Count == 0 && string.IsNullOrEmpty(filter))
                NoSelectionText.Visibility = Visibility.Visible;
        }

        private void OnItemSelect(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is LibraryItemViewModel vm)
            {
                _selectedItem = vm;
                ShowDetails(vm.Item);
            }
        }

        private void ShowDetails(LibraryItem item)
        {
            _isLodingDetails = true;
            DetailsPanel.Visibility = Visibility.Visible;
            NoSelectionText.Visibility = Visibility.Collapsed;
            
            DetailTitle.Text = item.Title;
            DetailDescription.Text = item.Description;
            IsCitedCheck.IsChecked = item.IsCited;
            ImportanceRating.Value = item.Importance;

            DetailAuthors.Text = string.IsNullOrEmpty(item.Authors) ? "--" : item.Authors;
            DetailJournal.Text = string.IsNullOrEmpty(item.Journal) ? "--" : item.Journal;
            DetailYear.Text = string.IsNullOrEmpty(item.Year) ? "--" : item.Year;
            var ids = new List<string>();
            if (!string.IsNullOrEmpty(item.Doi)) ids.Add($"DOI: {item.Doi}");
            if (!string.IsNullOrEmpty(item.Pmid)) ids.Add($"PMID: {item.Pmid}");
            if (!string.IsNullOrEmpty(item.Pmcid)) ids.Add($"PMCID: {item.Pmcid}");
            if (!string.IsNullOrEmpty(item.Isbn)) ids.Add($"ISBN: {item.Isbn}");
            if (!string.IsNullOrEmpty(item.ArxivId)) ids.Add($"arXiv: {item.ArxivId}");
            
            if (!string.IsNullOrEmpty(item.HalId)) ids.Add($"HAL: {item.HalId}");
            
            DetailIdentifier.Text = ids.Count > 0 ? string.Join("\n", ids) : "--";

            foreach (ComboBoxItem cbItem in StatusCombo.Items)
            {
                if (cbItem.Tag.ToString() == item.Status.ToString())
                {
                    StatusCombo.SelectedItem = cbItem;
                    break;
                }
            }
            
            UpdatePreview(item);
            _isLodingDetails = false;
        }

        private void UpdatePreview(LibraryItem item)
        {
            if (item.Type == LibraryItemType.Pdf)
            {
                string fullPath = Path.Combine(_project.StoragePath, item.RelativePath);
                if (File.Exists(fullPath))
                {
                    NoPreviewPanel.Visibility = Visibility.Collapsed;
                    PdfViewer.Visibility = Visibility.Visible;
                    PdfViewer.CoreWebView2?.Navigate(new Uri(fullPath).AbsoluteUri);
                }
                else { ShowNoPreview(); }
            }
            else if (item.Type == LibraryItemType.Web && !string.IsNullOrEmpty(item.Url))
            {
                 NoPreviewPanel.Visibility = Visibility.Collapsed;
                 PdfViewer.Visibility = Visibility.Visible;
                 try { PdfViewer.CoreWebView2?.Navigate(item.Url); } catch { ShowNoPreview(); }
            }
            else { ShowNoPreview(); }
        }

        private void ShowNoPreview()
        {
            PdfViewer.Visibility = Visibility.Collapsed;
            NoPreviewPanel.Visibility = Visibility.Visible;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { RefreshList(SearchBox.Text); }

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Documents (*.pdf;*.docx)|*.pdf;*.docx|Tous les fichiers (*.*)|*.*";
            
            if (openFileDialog.ShowDialog() == true)
            {
                string sourceFile = openFileDialog.FileName;
                string fileName = Path.GetFileName(sourceFile);
                string libraryDir = Path.Combine(_project.StoragePath, "Library");
                if (!Directory.Exists(libraryDir)) Directory.CreateDirectory(libraryDir);
                string destPath = Path.Combine(libraryDir, fileName);
                try {
                    File.Copy(sourceFile, destPath, true);
                    var newItem = new LibraryItem {
                        Title = Path.GetFileNameWithoutExtension(fileName).Replace("_", " "),
                        FileName = fileName,
                        RelativePath = Path.Combine("Library", fileName),
                        Type = fileName.ToLower().EndsWith(".pdf") ? LibraryItemType.Pdf : LibraryItemType.Other
                    };
                    _project.LibraryItems.Add(newItem);
                    _searchService.IndexItem(_project.StoragePath, newItem);
                    RefreshList();
                } catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            }
        }

        private async void AddByIdentifier_Click(object sender, RoutedEventArgs e)
        {
            var prompt = new PromptWindow("Ajouter par DOI ou PMID", "Entrez l'identifiant (ex: 10.1111/j.1234.x ou 12345678) :");
            if (prompt.ShowDialog() == true)
            {
                string input = prompt.Result.Trim();
                if (string.IsNullOrEmpty(input)) return;

                LibraryItem? newItem = null;
                string lowerInput = input.ToLower();

                if (lowerInput.Contains("isbn") || (input.Length >= 10 && input.All(c => char.IsDigit(c) || c == '-' || c == 'x' || c == 'X') && !input.Contains(".")))
                    newItem = await _metadataService.FetchByIsbn(input);
                else if (lowerInput.StartsWith("pmc"))
                    newItem = await _metadataService.FetchByPmcid(input);
                else if (lowerInput.Contains("arxiv") || (input.Contains(".") && input.Split('.').Length >= 2 && input.Split('.')[0].All(char.IsDigit) && input.Split('.')[1].All(char.IsDigit)))
                    newItem = await _metadataService.FetchByArxiv(input);
                else if (lowerInput.StartsWith("hal-") || (lowerInput.Contains("-") && lowerInput.Split('-').Length == 2 && lowerInput.StartsWith("hal")))
                    newItem = await _metadataService.FetchByHalId(input);
                else if (input.Contains(".") || input.Contains("/")) 
                    newItem = await _metadataService.FetchByDoi(input);
                else 
                    newItem = await _metadataService.FetchByPmid(input);

                if (newItem != null)
                {
                    _project.LibraryItems.Add(newItem);
                    RefreshList();
                    ShowDetails(newItem);
                }
                else
                {
                    MessageBox.Show("Impossible de trouver les métadonnées pour cet identifiant.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void AddLink_Click(object sender, RoutedEventArgs e)
        {
            var prompt = new PromptWindow("Ajouter un lien", "Entrez l'URL :");
            if (prompt.ShowDialog() == true)
            {
                var newItem = new LibraryItem { Title = "Lien Web", Url = prompt.Result, Type = LibraryItemType.Web };
                _project.LibraryItems.Add(newItem);
                RefreshList();
            }
        }

        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            try {
                if (_selectedItem.Item.Type == LibraryItemType.Web)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_selectedItem.Item.Url) { UseShellExecute = true });
                else
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Path.Combine(_project.StoragePath, _selectedItem.Item.RelativePath)) { UseShellExecute = true });
            } catch { }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null || _project == null) return;
            if (MessageBox.Show("Supprimer ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _project.LibraryItems.Remove(_selectedItem.Item);
                DetailsPanel.Visibility = Visibility.Collapsed;
                RefreshList();
            }
        }

        private void StatusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLodingDetails || _selectedItem == null) return;
            if (StatusCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
                _selectedItem.Item.Status = (LibraryItemStatus)Enum.Parse(typeof(LibraryItemStatus), item.Tag.ToString() ?? "ToBeRead");
            _selectedItem.NotifyStatusChanged();
        }

        private void ImportanceRating_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLodingDetails || _selectedItem == null) return;
            _selectedItem.Item.Importance = (int)ImportanceRating.Value;
        }

        private void DetailDescription_TextChanged(object sender, TextChangedEventArgs e) { if (!_isLodingDetails && _selectedItem != null) _selectedItem.Item.Description = DetailDescription.Text; }

        private void IsCited_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLodingDetails || _selectedItem == null) return;
            _selectedItem.Item.IsCited = IsCitedCheck.IsChecked ?? false;
            
            // Sync with Project References
            SyncReferenceWithProject(_selectedItem.Item);
        }

        private void SyncReferenceWithProject(LibraryItem item)
        {
            if (_project == null) return;

            if (item.IsCited)
            {
                // Ensure it's in the project references
                if (!_project.References.Any(r => r.Doi == item.Doi && !string.IsNullOrEmpty(item.Doi) || r.Id == item.Id))
                {
                    _project.References.Add(new Citation
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Authors = item.Authors,
                        Journal = item.Journal,
                        Year = item.Year,
                        Doi = item.Doi,
                        Source = item.Url
                    });
                }
            }
            else
            {
                // Optional: remove from references if unchecked?
                // Usually better to leave it but let's keep it clean
                var existing = _project.References.FirstOrDefault(r => r.Id == item.Id || (r.Doi == item.Doi && !string.IsNullOrEmpty(item.Doi)));
                if (existing != null) _project.References.Remove(existing);
            }
        }
    }

    public class LibraryItemViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public LibraryItem Item { get; }
        public LibraryItemViewModel(LibraryItem item) => Item = item;
        public string Title => Item.Title;
        public string FileName => Item.FileName;
        public string StatusLabel => Item.Status switch { LibraryItemStatus.ToBeRead => "À LIRE", LibraryItemStatus.Reading => "EN COURS", LibraryItemStatus.Read => "LU", LibraryItemStatus.Irrelevant => "EXCLU", _ => "INCONNU" };
        public System.Windows.Media.Brush StatusColor => Item.Status switch { LibraryItemStatus.ToBeRead => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219)), LibraryItemStatus.Reading => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 126, 34)), LibraryItemStatus.Read => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113)), LibraryItemStatus.Irrelevant => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(149, 165, 166)), _ => System.Windows.Media.Brushes.Gray };
        public string IconKind => Item.Type switch { LibraryItemType.Pdf => "FilePdfBox", LibraryItemType.Web => "Web", _ => "FileDocument" };
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        public void NotifyStatusChanged() { PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(StatusLabel))); PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(StatusColor))); }
    }
}


