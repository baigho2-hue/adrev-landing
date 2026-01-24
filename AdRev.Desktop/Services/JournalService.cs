using System.Collections.Generic;
using AdRev.Domain.Models;

namespace AdRev.Desktop.Services
{
    public class JournalService
    {
        public List<JournalSubmissionCriteria> GetCommonJournals()
        {
            return new List<JournalSubmissionCriteria>
            {
                // 1. Annals of Family Medicine
                new JournalSubmissionCriteria
                {
                    JournalName = "Annals of Family Medicine",
                    Publisher = "Annals",
                    Specialties = new List<string> { "Primary Care", "Family Medicine", "Public Health" },
                    MaxWordCountAbstract = 250,
                    MaxWordCountBody = 2500, // Range 1200-2500 generally
                    MaxFiguresAndTables = 5,
                    MaxReferences = 50, // Often ~30-50 for research
                    RequiresStructuredAbstract = true,
                    RequiredSections = new List<string> { "Introduction", "Methods", "Results", "Discussion", "Conclusion" },
                    CitationStyle = "AMA / ICMJE",
                    SubmissionUrl = "https://annfammed.org",
                    GuidelinesUrl = "https://annfammed.org/authors/"
                },

                // 2. The Lancet
                new JournalSubmissionCriteria
                {
                    JournalName = "The Lancet",
                    Publisher = "Elsevier",
                    Specialties = new List<string> { "General Medicine", "Global Health", "Clinical Research" },
                    MaxWordCountAbstract = 300,
                    MaxWordCountBody = 3500, // 4500 for RCTs
                    MaxFiguresAndTables = 5, // Typically constrained
                    MaxReferences = 30, // Strict on Research Letters, flexible on Articles but ~30 recommended/enforced based on context
                    RequiresStructuredAbstract = true,
                    RequiredSections = new List<string> { "Introduction", "Methods", "Results", "Discussion" }, 
                    CitationStyle = "Vancouver",
                    SubmissionUrl = "https://www.thelancet.com/authors/submit",
                    GuidelinesUrl = "https://www.thelancet.com/lancet/information-for-authors"
                },

                // 3. The BMJ
                new JournalSubmissionCriteria
                {
                    JournalName = "The BMJ",
                    Publisher = "BMJ",
                    Specialties = new List<string> { "General Medicine", "Health Policy", "Clinical Research" },
                    MaxWordCountAbstract = 300,
                    MaxWordCountBody = 4000, 
                    MaxFiguresAndTables = 6,
                    MaxReferences = 50, // Flexible
                    RequiresStructuredAbstract = true,
                    RequiredSections = new List<string> { "Design", "Setting", "Participants", "Interventions", "Outcomes", "Results", "Conclusions" },
                    CitationStyle = "Vancouver",
                    SubmissionUrl = "https://mc.manuscriptcentral.com/bmj",
                    GuidelinesUrl = "https://www.bmj.com/about-bmj/resources-authors"
                },

                // 4. The Lancet Regional Health - Africa
                new JournalSubmissionCriteria
                {
                    JournalName = "The Lancet Regional Health - Africa",
                    Publisher = "Elsevier",
                    Specialties = new List<string> { "General Medicine", "Public Health", "Infectious Disease" },
                    MaxWordCountAbstract = 300,
                    MaxWordCountBody = 3500,
                    MaxFiguresAndTables = 5,
                    MaxReferences = 50,
                    RequiresStructuredAbstract = true,
                    RequiredSections = new List<string> { "Introduction", "Methods", "Results", "Discussion", "Research in Context" },
                    CitationStyle = "Vancouver",
                    SubmissionUrl = "https://www.editorialmanager.com/tlrhafrica",
                    GuidelinesUrl = "https://www.thelancet.com/journals/lanafr/info/author-guidelines"
                },

                // 5. Qualitative Health Research
                new JournalSubmissionCriteria
                {
                    JournalName = "Qualitative Health Research",
                    Publisher = "SAGE",
                    Specialties = new List<string> { "Qualitative Methodology", "Social Sciences", "Health Services" },
                    MaxWordCountAbstract = 150,
                    MaxWordCountBody = 9000, // ~30 pages
                    MaxFiguresAndTables = 10, // Flexible
                    MaxReferences = 100, // No strict limit, usually generous
                    RequiresStructuredAbstract = false,
                    RequiredSections = new List<string> { "Introduction", "Literature Review", "Methods", "Findings", "Discussion", "Conclusion" },
                    CitationStyle = "APA",
                    SubmissionUrl = "https://mc.manuscriptcentral.com/qhr",
                    GuidelinesUrl = "https://journals.sagepub.com/author-instructions/QHR"
                },

                // 6. European Scientific Journal (ESJ)
                new JournalSubmissionCriteria
                {
                    JournalName = "European Scientific Journal",
                    Publisher = "European Scientific Institute",
                    Specialties = new List<string> { "Multidisciplinary", "Social Sciences", "Medical Sciences", "Humanities" },
                    MaxWordCountAbstract = 150, // Approximately
                    MaxWordCountBody = 8000, // 7-20 pages approx
                    MaxFiguresAndTables = 10, // Flexible
                    MaxReferences = 60, 
                    RequiresStructuredAbstract = false,
                    RequiredSections = new List<string> { "Introduction", "Methods", "Results", "Conclusion" },
                    CitationStyle = "APA",
                    SubmissionUrl = "https://eujournal.org/index.php/esj/about/submissions",
                    GuidelinesUrl = "https://eujournal.org/index.php/esj/about/submissions"
                }
            };
        }
    }
}
