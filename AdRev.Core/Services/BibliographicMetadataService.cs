using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using AdRev.Domain.Models;
using System.Xml.Linq;
using System.Linq;

namespace AdRev.Core.Services
{
    public class BibliographicMetadataService
    {
        private readonly HttpClient _httpClient;

        public BibliographicMetadataService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AdRevResearchTool/1.0 (mailto:support@adrev.com)");
        }

        public async Task<LibraryItem?> FetchByDoi(string doi)
        {
            try
            {
                // Crossref API
                string url = $"https://api.crossref.org/works/{doi}";
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);
                var message = doc.RootElement.GetProperty("message");

                var item = new LibraryItem
                {
                    Doi = doi,
                    Title = message.GetProperty("title")[0].GetString() ?? "Unknown Title",
                    Journal = message.TryGetProperty("container-title", out var ct) && ct.ValueKind == JsonValueKind.Array && ct.GetArrayLength() > 0 ? ct[0].GetString() ?? "" : "",
                    Year = message.TryGetProperty("published-print", out var pp) ? pp.GetProperty("date-parts")[0][0].ToString() : (message.TryGetProperty("created", out var cr) ? cr.GetProperty("date-parts")[0][0].ToString() : ""),
                    Type = LibraryItemType.Web,
                    Url = message.TryGetProperty("URL", out var u) ? u.GetString() ?? "" : ""
                };

                // Authors
                if (message.TryGetProperty("author", out var authors))
                {
                    var names = authors.EnumerateArray().Select(a => 
                        (a.TryGetProperty("family", out var f) ? f.GetString() : "") + " " + 
                        (a.TryGetProperty("given", out var g) ? g.GetString() : ""));
                    item.Authors = string.Join(", ", names);
                }

                return item;
            }
            catch
            {
                return null;
            }
        }

        public async Task<LibraryItem?> FetchByPmid(string pmid)
        {
            return await FetchFromNcbi(pmid, "pubmed");
        }

        public async Task<LibraryItem?> FetchByPmcid(string pmcid)
        {
            // Remove 'PMC' prefix if present
            string id = pmcid.Replace("PMC", "", StringComparison.OrdinalIgnoreCase).Trim();
            return await FetchFromNcbi(id, "pmc");
        }

        private async Task<LibraryItem?> FetchFromNcbi(string id, string db)
        {
            try
            {
                string url = $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esummary.fcgi?db={db}&id={id}&retmode=json";
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);
                var result = doc.RootElement.GetProperty("result").GetProperty(id);

                var item = new LibraryItem
                {
                    Title = result.GetProperty("title").GetString() ?? "Unknown Title",
                    Journal = result.TryGetProperty("fulljournalname", out var j) ? j.GetString() ?? "" : (result.TryGetProperty("source", out var s) ? s.GetString() ?? "" : ""),
                    Year = result.GetProperty("pubdate").GetString()?.Split(' ')[0] ?? "",
                    Type = LibraryItemType.Web,
                    Url = db == "pubmed" ? $"https://pubmed.ncbi.nlm.nih.gov/{id}/" : $"https://www.ncbi.nlm.nih.gov/pmc/articles/PMC{id}/"
                };

                if (db == "pubmed") item.Pmid = id; else item.Pmcid = "PMC" + id;

                if (result.TryGetProperty("authors", out var authors))
                {
                    var names = authors.EnumerateArray().Select(a => a.GetProperty("name").GetString());
                    item.Authors = string.Join(", ", names);
                }

                return item;
            }
            catch { return null; }
        }

        public async Task<LibraryItem?> FetchByIsbn(string isbn)
        {
            try
            {
                string cleanIsbn = isbn.Replace("-", "").Replace(" ", "");
                string url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{cleanIsbn}";
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);
                
                if (doc.RootElement.GetProperty("totalItems").GetInt32() == 0) return null;
                
                var volume = doc.RootElement.GetProperty("items")[0].GetProperty("volumeInfo");
                var item = new LibraryItem
                {
                    Isbn = cleanIsbn,
                    Title = volume.GetProperty("title").GetString() ?? "Unknown Title",
                    Journal = volume.TryGetProperty("publisher", out var p) ? p.GetString() ?? "" : "Book",
                    Year = volume.TryGetProperty("publishedDate", out var d) ? d.GetString()?.Split('-')[0] ?? "" : "",
                    Type = LibraryItemType.Other,
                    Url = volume.TryGetProperty("infoLink", out var l) ? l.GetString() ?? "" : ""
                };

                if (volume.TryGetProperty("authors", out var authors))
                {
                    item.Authors = string.Join(", ", authors.EnumerateArray().Select(a => a.GetString()));
                }

                return item;
            }
            catch { return null; }
        }

        public async Task<LibraryItem?> FetchByArxiv(string arxivId)
        {
            try
            {
                string url = $"http://export.arxiv.org/api/query?id_list={arxivId}";
                var response = await _httpClient.GetStringAsync(url);
                var xdoc = XDocument.Parse(response);
                var ns = XNamespace.Get("http://www.w3.org/2005/Atom");
                var entry = xdoc.Descendants(ns + "entry").FirstOrDefault();

                if (entry == null) return null;

                var item = new LibraryItem
                {
                    ArxivId = arxivId,
                    Title = entry.Element(ns + "title")?.Value.Replace("\n", " ").Trim() ?? "Unknown",
                    Journal = "arXiv preprint",
                    Year = entry.Element(ns + "published")?.Value.Split('-')[0] ?? "",
                    Type = LibraryItemType.Web,
                    Url = $"https://arxiv.org/abs/{arxivId}"
                };

                var authors = entry.Descendants(ns + "name").Select(a => a.Value);
                item.Authors = string.Join(", ", authors);

                return item;
            }
            catch { return null; }
        }

        public async Task<LibraryItem?> FetchByHalId(string halId)
        {
            try
            {
                // HAL Open API
                string url = $"https://api.archives-ouvertes.fr/search/?q=hal_id:\"{halId}\"&fl=title_s,authFullName_s,journalTitle_s,producedDateY_i,uri_s&wt=json";
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);
                
                var responseObj = doc.RootElement.GetProperty("response");
                if (responseObj.GetProperty("numFound").GetInt32() == 0) return null;

                var docObj = responseObj.GetProperty("docs")[0];
                var item = new LibraryItem
                {
                    HalId = halId,
                    Title = docObj.TryGetProperty("title_s", out var t) && t.ValueKind == JsonValueKind.Array ? t[0].GetString() ?? "Unknown" : "Unknown",
                    Journal = docObj.TryGetProperty("journalTitle_s", out var j) ? j.GetString() ?? "HAL Open Archive" : "HAL Open Archive",
                    Year = docObj.TryGetProperty("producedDateY_i", out var y) ? y.GetInt32().ToString() : "",
                    Type = LibraryItemType.Web,
                    Url = docObj.TryGetProperty("uri_s", out var u) ? u.GetString() ?? "" : ""
                };

                if (docObj.TryGetProperty("authFullName_s", out var authors) && authors.ValueKind == JsonValueKind.Array)
                {
                    item.Authors = string.Join(", ", authors.EnumerateArray().Select(a => a.GetString()));
                }

                return item;
            }
            catch { return null; }
        }
    }
}
