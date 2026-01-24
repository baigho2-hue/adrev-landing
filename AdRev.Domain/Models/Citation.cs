using System;

namespace AdRev.Domain.Models;

public class Citation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Authors { get; set; } = string.Empty; // "Smith J, Doe A"
    public string Year { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // Journal, Publisher, URL
    public string Journal { get; set; } = string.Empty;
    public string Volume { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Pages { get; set; } = string.Empty;
    public string Doi { get; set; } = string.Empty;
    public string FormattedString { get; set; } = string.Empty; // Cache
}
