using System;

namespace AdRev.Domain.Models
{
    public enum LibraryItemType
    {
        Pdf,
        Word,
        Web,
        Video,
        Audio,
        Other
    }

    public enum LibraryItemStatus
    {
        ToBeRead,    // À lire
        Reading,     // En cours
        Read,        // Lu
        Irrelevant   // Non pertinent
    }

    public class LibraryItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty; 
        public LibraryItemType Type { get; set; } = LibraryItemType.Other;
        public string Url { get; set; } = string.Empty; 
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        
        // New management properties
        public LibraryItemStatus Status { get; set; } = LibraryItemStatus.ToBeRead;
        public int Importance { get; set; } = 3; // 1-5 stars
        public bool IsCited { get; set; } = false; // Is used in the bibliography?

        // Bibliographic Metadata
        public string Doi { get; set; } = string.Empty;
        public string Pmid { get; set; } = string.Empty;
        public string Pmcid { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string ArxivId { get; set; } = string.Empty;
        public string HalId { get; set; } = string.Empty;
        public string Authors { get; set; } = string.Empty;
        public string Journal { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
    }
}
