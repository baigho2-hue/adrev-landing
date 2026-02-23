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
                },

                // 7. The Lancet Global Health
                new JournalSubmissionCriteria
                {
                    JournalName = "The Lancet Global Health",
                    Publisher = "Elsevier",
                    Specialties = new List<string> { "Global Health", "Public Health", "Epidemiology" },
                    MaxWordCountAbstract = 300,
                    MaxWordCountBody = 3500,
                    MaxFiguresAndTables = 5,
                    MaxReferences = 30,
                    RequiresStructuredAbstract = true,
                    RequiredSections = new List<string> { "Introduction", "Methods", "Results", "Discussion" }, 
                    CitationStyle = "Vancouver",
                    SubmissionUrl = "https://www.editorialmanager.com/langlo",
                    GuidelinesUrl = "https://www.thelancet.com/journals/langlo/info/author-guidelines"
                },

                // 8. PLOS ONE
                new JournalSubmissionCriteria
                {
                    JournalName = "PLOS ONE",
                    Publisher = "PLOS",
                    Specialties = new List<string> { "Multidisciplinary", "Science", "Medicine" },
                    MaxWordCountAbstract = 300, // No strict limit but recommended
                    MaxWordCountBody = 0, // No limit
                    MaxFiguresAndTables = 0, // No limit
                    MaxReferences = 0, // No limit
                    RequiresStructuredAbstract = false,
                    RequiredSections = new List<string> { "Introduction", "Materials and Methods", "Results", "Discussion", "Conclusion" }, 
                    CitationStyle = "Vancouver",
                    SubmissionUrl = "https://www.editorialmanager.com/pone",
                    GuidelinesUrl = "https://journals.plos.org/plosone/s/submission-guidelines"
                },

                // 9. BMC Public Health
                new JournalSubmissionCriteria
                {
                    JournalName = "BMC Public Health",
                    Publisher = "BioMed Central",
                    Specialties = new List<string> { "Public Health", "Epidemiology" },
                    MaxWordCountAbstract = 350,
                    MaxWordCountBody = 0, // No limit
                    MaxFiguresAndTables = 0, // No limit
                    MaxReferences = 0, // No limit
                    RequiresStructuredAbstract = true,
                    RequiredSections = new List<string> { "Background", "Methods", "Results", "Discussion", "Conclusions" }, 
                    CitationStyle = "Vancouver (Numbered)",
                    SubmissionUrl = "https://www.editorialmanager.com/pubh",
                    GuidelinesUrl = "https://bmcpublichealth.biomedcentral.com/submission-guidelines"
                },

                // 10. Mali Médical
                new JournalSubmissionCriteria
                {
                    JournalName = "Mali Médical",
                    Publisher = "Société Malienne de Médecine",
                    Specialties = new List<string> { "General Medicine", "Tropical Medicine" },
                    MaxWordCountAbstract = 250,
                    MaxWordCountBody = 3000, 
                    MaxFiguresAndTables = 4, 
                    MaxReferences = 25, 
                    RequiresStructuredAbstract = true,
                    RequiredSections = new List<string> { "Introduction", "Méthodes", "Résultats", "Discussion", "Conclusion" }, 
                    CitationStyle = "Vancouver",
                    SubmissionUrl = "http://www.malimedical.org",
                    GuidelinesUrl = "http://www.malimedical.org/instructions-aux-auteurs"
                }
            };
        }
    }
}
