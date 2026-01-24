using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using AdRev.Domain.Models;

namespace AdRev.Desktop
{
    public partial class CitationEntryWindow : Window
    {
        public List<Citation> ResultCitations { get; private set; } = new List<Citation>();

        private ObservableCollection<Citation> _pendingGroup = new ObservableCollection<Citation>();

        public CitationEntryWindow()
        {
            InitializeComponent();
            PendingCitationsBox.ItemsSource = _pendingGroup;
        }

        private async void SearchDoi_Click(object sender, RoutedEventArgs e)
        {
            string query = CitDoiBox.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                 MessageBox.Show("Veuillez entrer un DOI, un PMID ou un Titre.", "Recherche Manquante");
                 return;
            }

            // Cleanup DOI URL
            query = query.Replace("https://doi.org/", "").Replace("http://dx.doi.org/", "");
            
            // Detect Type
            bool isPmid = query.All(char.IsDigit) && query.Length < 10 && query.Length > 0;
            bool isDoi = query.Contains("10.") || query.Contains("/");

            try 
            {
                CitAuthorsBox.Text = "Recherche en cours...";
                
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "AdRev/1.0 (mailto:contact@adrev.com)");

                JsonDocument? doc = null;
                
                if (isPmid)
                {
                    var json = await client.GetStringAsync($"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esummary.fcgi?db=pubmed&id={query}&retmode=json");
                    var root = JsonDocument.Parse(json);
                    
                    if (root.RootElement.TryGetProperty("result", out var result) && result.TryGetProperty(query, out var item))
                    {
                         if (item.TryGetProperty("title", out var t)) CitTitleBox.Text = t.GetString();
                         if (item.TryGetProperty("source", out var s)) CitSourceBox.Text = s.GetString();
                         if (item.TryGetProperty("pubdate", out var d)) 
                         {
                             var ds = d.GetString() ?? "";
                             if (ds.Length >= 4) CitYearBox.Text = ds.Substring(0, 4);
                         }
                         if (item.TryGetProperty("authors", out var authorsAuth))
                         {
                              var authorList = new List<string>();
                              foreach(var auth in authorsAuth.EnumerateArray())
                              {
                                  if (auth.TryGetProperty("name", out var n)) authorList.Add(n.GetString() ?? string.Empty);
                              }
                               if (authorList.Count > 3) 
                                    CitAuthorsBox.Text = string.Join(", ", authorList.Take(3)) + " et al.";
                                else
                                    CitAuthorsBox.Text = string.Join(", ", authorList);
                         }
                    }
                    else
                    {
                         MessageBox.Show("Aucun résultat trouvé sur PubMed.", "Info");
                         CitAuthorsBox.Text = "";
                    }
                }
                else
                {
                    string url = isDoi ? $"https://api.crossref.org/works/{query}" : $"https://api.crossref.org/works?query.bibliographic={System.Uri.EscapeDataString(query)}&rows=1";
                    
                    var json = await client.GetStringAsync(url);
                    doc = JsonDocument.Parse(json);
                    
                    JsonElement message = default;
                    
                    if (isDoi)
                    {
                        if (doc.RootElement.TryGetProperty("message", out var m)) message = m;
                    }
                    else
                    {
                        if (doc.RootElement.TryGetProperty("message", out var m) && m.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                        {
                            message = items[0];
                        }
                    }

                    if (message.ValueKind != JsonValueKind.Undefined)
                    {
                        if (message.TryGetProperty("title", out var titles) && titles.GetArrayLength() > 0)
                            CitTitleBox.Text = titles[0].GetString();

                        if (message.TryGetProperty("author", out var authors))
                        {
                            var authorList = new List<string>();
                            foreach(var auth in authors.EnumerateArray())
                            {
                                string? family = auth.TryGetProperty("family", out var f) ? f.GetString() : "";
                                string? given = auth.TryGetProperty("given", out var g) ? g.GetString() : "";
                                if (!string.IsNullOrEmpty(family)) 
                                    authorList.Add($"{family} {given?.FirstOrDefault()}");
                            }
                            if (authorList.Count > 3) 
                                CitAuthorsBox.Text = string.Join(", ", authorList.Take(3)) + " et al.";
                            else
                                CitAuthorsBox.Text = string.Join(", ", authorList);
                        }
                        
                        var dateProp = message.TryGetProperty("published-print", out var pp) ? pp : 
                                       (message.TryGetProperty("created", out var c) ? c : default);
                                       
                        if (dateProp.ValueKind != JsonValueKind.Undefined && dateProp.TryGetProperty("date-parts", out var parts))
                        {
                            if (parts.GetArrayLength() > 0 && parts[0].GetArrayLength() > 0)
                                CitYearBox.Text = parts[0][0].ToString();
                        }

                        if (message.TryGetProperty("container-title", out var containers) && containers.GetArrayLength() > 0)
                            CitSourceBox.Text = containers[0].GetString();
                        else if (message.TryGetProperty("publisher", out var pub))
                             CitSourceBox.Text = pub.GetString();
                    }
                    else
                    {
                         MessageBox.Show("Aucun résultat trouvé.", "Info");
                         CitAuthorsBox.Text = "";
                    }
                }
            } 
            catch (Exception ex) 
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur API");
                CitAuthorsBox.Text = ""; 
            }
        }

        private void AddToPending_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CitAuthorsBox.Text) || string.IsNullOrWhiteSpace(CitYearBox.Text))
            {
                MessageBox.Show("Auteurs et Année requis.", "Manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cit = new Citation
            {
                Authors = CitAuthorsBox.Text,
                Year = CitYearBox.Text,
                Title = CitTitleBox.Text,
                Source = CitSourceBox.Text
            };
            if (string.IsNullOrEmpty(cit.Title)) cit.Title = "Sans titre";

            _pendingGroup.Add(cit);

            // Clear
            CitDoiBox.Text = "";
            CitAuthorsBox.Text = "";
            CitYearBox.Text = "";
            CitTitleBox.Text = "";
            CitSourceBox.Text = "";
            CitDoiBox.Focus();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            // Add current fields if likely forgotten
            if (!string.IsNullOrWhiteSpace(CitAuthorsBox.Text))
            {
                AddToPending_Click(sender, e);
            }

            if (_pendingGroup.Count == 0)
            {
                MessageBox.Show("Liste vide.", "Info");
                return;
            }

            ResultCitations = _pendingGroup.ToList();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}
