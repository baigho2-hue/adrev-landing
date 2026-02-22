using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using AdRev.Domain.Models;

namespace AdRev.Core.Services
{
    public class LibrarySearchService
    {
        // Simple in-memory cache for the session
        private Dictionary<string, string> _textCache = new Dictionary<string, string>();

        public void IndexItem(string projectPath, LibraryItem item)
        {
            if (item.Type != LibraryItemType.Pdf) return;
            if (_textCache.ContainsKey(item.Id)) return;

            string fullPath = Path.Combine(projectPath, item.RelativePath);
            if (!File.Exists(fullPath)) return;

            try
            {
                using (var pdf = PdfDocument.Open(fullPath))
                {
                    var text = string.Join(" ", pdf.GetPages().Select(p => p.Text));
                    _textCache[item.Id] = text;
                }
            }
            catch
            {
                // Silently skip or log
            }
        }

        public List<string> SearchInLibrary(string query, List<LibraryItem> items)
        {
            if (string.IsNullOrWhiteSpace(query)) return items.Select(i => i.Id).ToList();

            var results = new List<string>();
            foreach (var item in items)
            {
                // Search in Metadata
                if (item.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                    item.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(item.Id);
                    continue;
                }

                // Search in PDF Content
                if (_textCache.TryGetValue(item.Id, out var content))
                {
                    if (content.Contains(query, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(item.Id);
                    }
                }
            }
            return results;
        }
    }
}


