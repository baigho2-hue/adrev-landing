using AdRev.Domain.Enums;
using AdRev.Domain.Protocols;
using AdRev.Domain.Variables;
using System.Collections.Generic;
using System.Linq;

namespace AdRev.Core.Services
{
    public class StatisticalSuggesterService
    {
        public List<string> SuggestTests(ResearchProtocol protocol)
        {
            var suggestions = new List<string>();
            var variables = protocol.Variables ?? new List<StudyVariable>();

            if (!variables.Any())
            {
                suggestions.Add("💡 Définissez vos variables dans le Concepteur de Variables pour obtenir des suggestions de tests.");
                return suggestions;
            }

            // Identify Dependent Variable (often the first or one marked, here we look for a 'Result' variable or just analyze pairs)
            var quantVars = variables.Where(v => v.Type == VariableType.QuantitativeContinuous || v.Type == VariableType.QuantitativeDiscrete).ToList();
            var qualVars = variables.Where(v => v.Type == VariableType.QualitativeBinary || v.Type == VariableType.QualitativeNominal || v.Type == VariableType.QualitativeOrdinal).ToList();

            // 1. Comparison of 2 groups (e.g. Sex vs Outcome)
            if (qualVars.Any(v => v.Type == VariableType.QualitativeBinary))
            {
                if (quantVars.Any())
                {
                    suggestions.Add("📊 Comparaison de moyennes : Comme vous avez une variable binaire et des variables quantitatives, le **Test t de Student** (ou Mann-Whitney si non-normal) est recommandé.");
                }
                
                if (qualVars.Count >= 2)
                {
                    suggestions.Add("📊 Association : Pour croiser deux variables qualitatives, utilisez le **Test du Chi-carré de Pearson**.");
                }
            }

            // 2. Comparison of >2 groups
            if (qualVars.Any(v => v.Type == VariableType.QualitativeNominal && !v.ChoiceOptions.Contains(";") || (v.ChoiceOptions?.Split(';').Length > 2)))
            {
                if (quantVars.Any())
                {
                    suggestions.Add("📊 Comparaison multiple : Pour comparer une variable quantitative entre plus de 2 groupes, utilisez l'**ANOVA à un facteur** (ou Kruskal-Wallis).");
                }
            }

            // 3. Correlation / Regression
            if (quantVars.Count >= 2)
            {
                suggestions.Add("📊 Relation linéaire : Pour deux variables quantitatives, utilisez la **Corrélation de Pearson** et la **Régression Linéaire**.");
            }

            if (qualVars.Any(v => v.Type == VariableType.QualitativeBinary) && quantVars.Any())
            {
                 suggestions.Add("📊 Prédiction : Pour prédire une issue binaire (ex: Malade/Sain), utilisez la **Régression Logistique**.");
            }

            // 4. Study specific suggestions
            if (protocol.EpidemiologyType == EpidemiologicalStudyType.CaseControl)
            {
                suggestions.Add("📊 Étude Cas-Témoins : L'analyse principale doit inclure le calcul de l'**Odds Ratio (OR)**.");
            }
            else if (protocol.EpidemiologyType == EpidemiologicalStudyType.CohortProspective)
            {
                suggestions.Add("📊 Étude de Cohorte : L'analyse doit inclure le **Risque Relatif (RR)** ou des courbes de survie de **Kaplan-Meier**.");
            }

            return suggestions;
        }

        public string GetMethodologicalAdvice(ResearchProtocol protocol)
        {
            if (protocol.StudyType == StudyType.Qualitative)
            {
                return "💡 Pour votre étude qualitative, l'analyse thématique de contenu (inductive ou déductive) est recommandée via le codage des verbatims.";
            }
            
            if (protocol.IsClusterSampling)
            {
                return "⚠️ Attention : Comme vous utilisez un échantillonnage en grappe, vos tests statistiques devront être ajustés pour l'effet de plan (Clustered Standard Errors).";
            }

            return "💡 Assurez-vous que la distribution de vos variables quantitatives est normale avant d'utiliser des tests paramétriques.";
        }
    }
}
