using AdRev.Domain.Enums;
using AdRev.Domain.Models;
using System.Linq;

namespace AdRev.Desktop.Services
{
    public class WritingAssistantService
    {
        public class WritingTip
        {
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string TenseHelp { get; set; } = string.Empty;
        }

        public static WritingTip GetTipForSection(string sectionName, AcademicLevel level, StudyType studyType, JournalSubmissionCriteria? journal = null)
        {
            var tip = new WritingTip();

            switch (sectionName.ToLower())
            {
                case "introduction":
                    tip.Title = "Logique de l'Introduction";
                    tip.Content = level == AcademicLevel.NGOReport 
                        ? "Concentrez-vous sur le contexte humanitaire/social et l'urgence de l'intervention."
                        : "Partez du contexte global vers votre problématique spécifique (Entonnoir).";
                    tip.TenseHelp = "Présent (faits établis) et Passé Composé (études antérieures).";
                    break;

                case "methodologie":
                    tip.Title = "Rigueur et Transparence";
                    string checklist = studyType == StudyType.Quantitative ? "STROBE (Obs.) ou CONSORT (Essais)" : "COREQ (Qualitatif)";
                    tip.Content = $"Assurez-vous de couvrir les critères {checklist}. Décrivez ce qui A ÉTÉ fait.";
                    tip.TenseHelp = "IMPÉRATIF : Passez du Futur au Passé Simple/Imparfait.";
                    
                    if (journal != null && journal.RequiredSections.Contains("Methods"))
                         tip.Content += $" Note : {journal.JournalName} exige une description précise des analyses statistiques.";
                    break;

                case "resultats":
                    tip.Title = "Résultats - Faits Bruts";
                    tip.Content = level == AcademicLevel.NGOReport
                        ? "Mettez en avant les indicateurs clés de performance et l'impact direct."
                        : "Présentez les faits sans interprétation. Utilisez Tableaux/Figures.";
                    tip.TenseHelp = "Passé Simple ou Passé Composé exclusif.";
                    break;

                case "discussion":
                    tip.Title = "Discussion et Contextualisation";
                    tip.Content = "Comparez vos résultats à la littérature. Expliquez les limites.";
                    tip.TenseHelp = "Mélange : Passé (vos résultats) et Présent (littérature).";
                    break;

                case "conclusion":
                    tip.Title = "Conclusion et Recommandations";
                    tip.Content = level == AcademicLevel.NGOReport
                        ? "Proposez des actions concrètes et opérationnelles pour le terrain."
                        : "Synthèse de l'apport théorique et ouverture.";
                    tip.TenseHelp = "Présent (conclusion) et Conditionnel (recommandations).";
                    break;

                default:
                    tip.Title = "Conseil Général";
                    tip.Content = "Soyez clair, précis et concis.";
                    break;
            }

            // Journal Specific Constraints
            if (journal != null)
            {
                if (sectionName.ToLower() == "abstract" || sectionName.ToLower() == "resume")
                {
                    tip.Content += $"\n\nCONTRAINTE REVUE : Max {journal.MaxWordCountAbstract} mots.";
                }
            }

            return tip;
        }

        public static string SuggestPastTense(string futureVerb)
        {
            // Simple mapping for common research verbs
            var mapping = new Dictionary<string, string>
            {
                { "inclura", "a inclus / inclut" },
                { "collectera", "a collecté / collecta" },
                { "analysera", "a analysé / analysa" },
                { "évaluera", "a évalué / évalua" },
                { "mesurera", "a mesuré / mesura" },
                { "recrutera", "a recruté / recruta" },
                { "distribuera", "a distribué / distribua" }
            };

            return mapping.ContainsKey(futureVerb.ToLower()) ? mapping[futureVerb.ToLower()] : "Vérifiez la concordance des temps (Passé Simple/Composé)";
        }
    }
}
