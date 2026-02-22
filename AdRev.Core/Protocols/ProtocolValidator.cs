using AdRev.Domain.Protocols;
<<<<<<< HEAD
using AdRev.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System;
=======
using System.Collections.Generic;
using System.Linq;
>>>>>>> origin/main

namespace AdRev.Core.Protocols
{
    public class ValidationResult
    {
        public int Score { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();

        // Helper methods for convenience, assuming they are intended to be added or already exist
        public void AddError(string error) => Errors.Add(error);
        public void AddSuggestion(string suggestion) => Suggestions.Add(suggestion);
    }

    public class ProtocolValidator
    {
        // This method was part of the user's provided snippet but seems to be a placeholder
        // for a refactoring that is not part of the current instruction.
        private readonly StatisticalSuggesterService _suggester = new StatisticalSuggesterService();

        private void CheckBloomTaxonomy(ResearchProtocol protocol, ValidationResult result)
        {
            var bloomVerbs = GetBloomVerbs();
            var soLines = (protocol.SpecificObjectives ?? "").Split(new[] { '\r', '\n', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in soLines)
            {
                var cleaned = line.Trim().TrimStart('-', '*', '•', ' ');
                if (string.IsNullOrWhiteSpace(cleaned)) continue;
                
                var firstWord = cleaned.Split(' ')[0].ToLower();
                if (!bloomVerbs.Keys.Any(v => firstWord.StartsWith(v)))
                {
                    result.AddSuggestion($"💡 Verbe Bloom : '{firstWord}' n'est pas un verbe d'action standard. Essayez d'utiliser 'Déterminer', 'Mesurer' ou 'Analyser'.");
                }
            }
        }

        private void CheckStudyDesignCompliance(ResearchProtocol protocol, ValidationResult result)
        {
            // Assuming EpidemiologicalStudyType enum is defined in AdRev.Domain.Protocols or a related namespace
            // For this change, I'll assume it's accessible.
            if (protocol.EpidemiologyType == AdRev.Domain.Enums.EpidemiologicalStudyType.None) return;

            string methodology = (protocol.StudySetting + " " + protocol.StudyPopulation + " " + protocol.DataCollection + " " + protocol.SamplingMethod).ToLower();
            string objectives = (protocol.GeneralObjective + " " + protocol.SpecificObjectives).ToLower();

            // 1. Essais Cliniques / Expérimentales
            if (protocol.EpidemiologyType == AdRev.Domain.Enums.EpidemiologicalStudyType.RandomizedControlledTrial ||
                protocol.EpidemiologyType == AdRev.Domain.Enums.EpidemiologicalStudyType.QuasiExperimental)
            {
                if (!methodology.Contains("randomis") && protocol.EpidemiologyType == AdRev.Domain.Enums.EpidemiologicalStudyType.RandomizedControlledTrial)
                {
                    result.AddError("❌ Pour un Essai Randomisé, vous devez décrire la méthode de RANDOMISATION (blocs, simple, table...).");
                    result.Score -= 10;
                }
                if (!methodology.Contains("aveugle") && !methodology.Contains("insu") && !methodology.Contains("ouvert"))
                {
                    result.AddError("❌ Précisez si l'étude est en simple/double AVEUGLE (insu) ou en OUVERT.");
                    result.Score -= 5;
                }
                if (!methodology.Contains("groupe") && !methodology.Contains("bras"))
                {
                     result.AddSuggestion("⚠️ Mentionnez clairement les GROUPES (Intervention vs Témoin/Contrôle).");
                }
            }

            // 2. Cas-Témoins
            else if (protocol.EpidemiologyType == AdRev.Domain.Enums.EpidemiologicalStudyType.CaseControl)
            {
                if (!methodology.Contains("cas") || !methodology.Contains("témoin") && !methodology.Contains("temoin"))
                {
                    result.AddError("❌ Une étude Cas-Témoins exige une définition stricte des 'CAS' et des 'TÉMOINS'.");
                    result.Score -= 10;
                }
                if (!methodology.Contains("apparie") && !methodology.Contains("match"))
                {
                    result.AddSuggestion("💡 Avez-vous prévu un APPARIEMENT (matching) entre cas et témoins ? Si non, justifiez.");
                }
                if (!protocol.DataAnalysis.ToLower().Contains("odds") && !protocol.DataAnalysis.ToLower().Contains("rapport de cote"))
                {
                     result.AddError("❌ L'analyse statistique d'une étude Cas-Témoins doit mentionner le calcul de l'ODDS RATIO (OR).");
                     result.Score -= 5;
                }
            }

            // 3. Cohorte
            else if (protocol.EpidemiologyType == AdRev.Domain.Enums.EpidemiologicalStudyType.CohortProspective ||
                     protocol.EpidemiologyType == AdRev.Domain.Enums.EpidemiologicalStudyType.CohortRetrospective)
            {
                if (!methodology.Contains("suivi") && !methodology.Contains("suivre") && !methodology.Contains("durée"))
                {
                    result.AddError("❌ Une étude de Cohorte nécessite la mnetion d'une période de SUIVI.");
                    result.Score -= 5;
                }
                if (!methodology.Contains("expos"))
                {
                    result.AddError("❌ Définissez clairement les groupes EXPOSÉS et NON-EXPOSÉS.");
                    result.Score -= 5;
                }
                if (!protocol.DataAnalysis.ToLower().Contains("relatif") && !protocol.DataAnalysis.ToLower().Contains("incidence"))
                {
                     result.AddSuggestion("💡 L'analyse devrait viser le calcul du RISQUE RELATIF ou du Ratio d'Incidence.");
                }
            }
            
            // 4. Transversale
            else if (protocol.EpidemiologyType == AdRev.Domain.Enums.EpidemiologicalStudyType.CrossSectionalDescriptive)
            {
                 if (!methodology.Contains("période") && !methodology.Contains("date"))
                 {
                     result.AddSuggestion("⚠️ Une étude transversale est une 'photographie'. Précisez bien la PÉRIODE de collecte.");
                 }
            }
        }

        public ValidationResult Validate(ResearchProtocol protocol)
        {
            var result = new ValidationResult { Score = 100 };
            
            // Appels des validations spécifiques
            CheckBloomTaxonomy(protocol, result);
            CheckStudyDesignCompliance(protocol, result);

            var errors = result.Errors;
            var suggestions = result.Suggestions;

            // 1. Titre (Impact: 10 pts)
            if (string.IsNullOrWhiteSpace(protocol.Title))
            {
                errors.Add("❌ Titre manquant.");
                result.Score -= 10;
            }
            else if (protocol.Title.Length < 15)
            {
                suggestions.Add("⚠️ Le titre est très court. Assurez-vous qu'il contient les variables principales et la population d'étude.");
                result.Score -= 5;
            }

            // 1 bis. Introduction Structurée (Impact: 20 pts)
            int introLength = (protocol.Context?.Length ?? 0) + (protocol.ProblemJustification?.Length ?? 0);
            
            if (string.IsNullOrWhiteSpace(protocol.Context))
            {
                 errors.Add("❌ Contexte manquant.");
                 result.Score -= 10;
            }
            if (string.IsNullOrWhiteSpace(protocol.ProblemJustification))
            {
                 errors.Add("❌ Problématique et Justification manquantes.");
                 result.Score -= 10;
            }

            // Vérification longueur (Max 2 pages A4 ~ 2000 mots approx)
            int wordCount = (protocol.Context + " " + protocol.ProblemJustification)
                            .Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Length;

            if (wordCount > 2000)
            {
                suggestions.Add($"⚠️ L'introduction est trop longue ({wordCount} mots). Essayez de rester sous les 2000 mots.");
                result.Score -= 5;
            }

            // 2. Questions de Recherche (Impact: 15 pts)
            if (string.IsNullOrWhiteSpace(protocol.ResearchQuestion))
            {
                errors.Add("❌ Questions de recherche manquantes.");
                result.Score -= 15;
            }
            else
            {
                int questionCount = protocol.ResearchQuestion.Count(c => c == '?');
                if (questionCount > 3)
                {
                    errors.Add($"❌ Trop de questions de recherche ({questionCount}). Le maximum autorisé est de 3.");
                    result.Score -= 10;
                }
                else if (questionCount == 0)
                {
                     suggestions.Add("⚠️ Aucune question identifiée (pas de point d'interrogation ?).");
                }
            }

            // 2 bis. Hypothèses (Impact: 10 pts)
            if (!string.IsNullOrWhiteSpace(protocol.Hypotheses))
            {
                // Estimation grossière par saut de ligne
                var lines = protocol.Hypotheses.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 2)
                {
                     suggestions.Add($"⚠️ Vérifiez le nombre d'hypothèses. Le maximum recommandé est de 2.");
                     result.Score -= 5;
                }
            }
            
            // 3. Objectifs (Impact: 20 pts) - Analyse Taxonomique
            var bloomVerbs = GetBloomVerbs();
            
            // A. Analyse Objectif Général
            if (string.IsNullOrWhiteSpace(protocol.GeneralObjective))
            {
                errors.Add("❌ Objectif général manquant.");
                result.Score -= 10;
            }
            else
            {
                var goLines = protocol.GeneralObjective.Split(new[] { '\r', '\n', '.' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Where(l => l.Trim().Length > 5).ToArray();
                
                if (goLines.Length > 2)
                {
                    errors.Add($"❌ Trop d'objectifs généraux ({goLines.Length}). Maximum autorisé : 2.");
                    result.Score -= 5;
                }

                // Vérification du verbe de l'OG
                foreach (var line in goLines)
                {
                    var firstWord = line.Trim().Split(' ')[0].ToLower();
                    if (protocol.StudyType == AdRev.Domain.Enums.StudyType.Quantitative && (firstWord.StartsWith("comprend") || firstWord.StartsWith("explor")))
                    {
                        suggestions.Add($"⚠️ Incohérence : Pour une étude Quantitative, évitez le verbe '{firstWord}'. Préférez 'Mesurer', 'Quantifier', 'Déterminer'.");
                        result.Score -= 2;
                    }
                }
            }

            // B. Analyse Objectifs Spécifiques
            if (string.IsNullOrWhiteSpace(protocol.SpecificObjectives))
            {
                errors.Add("❌ Objectifs spécifiques manquants.");
                result.Score -= 5;
            }
            else
            {
                var soLines = protocol.SpecificObjectives.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .Where(l => l.Trim().Length > 3).ToArray();

                int goCount = Math.Max(1, protocol.GeneralObjective.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length);
                int maxSo = goCount * 4;

                if (soLines.Length > maxSo)
                {
                    errors.Add($"❌ Trop d'objectifs spécifiques ({soLines.Length}). Max recommandé : 4 par objectif général (Total Max ici : {maxSo}).");
                    result.Score -= 5;
                }

                foreach (var line in soLines)
                {
                    var firstWord = line.Trim().TrimStart('-', '*', '•', ' ').Split(' ')[0].ToLower();
                    
                    if (bloomVerbs.Keys.Any(k => firstWord.StartsWith(k)))
                    {
                         // Verbe reconnu, c'est bien
                    }
                    else
                    {
                        // suggestions.Add($"ℹ️ Suggestion : '{firstWord}' ne figure pas dans la liste standard de Bloom.");
                    }
                }
            }

            // Définition des Concepts (Bonus ou pénalité légère)
            if (string.IsNullOrWhiteSpace(protocol.ConceptDefinitions))
            {
                suggestions.Add("⚠️ Pensez à définir les concepts clés pour clarifier votre approche.");
                result.Score -= 2;
            }
            else if (protocol.SpecificObjectives.Split(';').Length < 2 && !protocol.SpecificObjectives.Contains("\n"))
            {
                suggestions.Add("⚠️ Il est recommandé d'avoir au moins 2 ou 3 objectifs spécifiques pour opérationnaliser l'objectif général.");
                result.Score -= 2;
            }

            // 4. Méthodologie Détaillée (Impact: 40 pts) - Le cœur scientifique
            bool methoIncomplete = false;
            
            if (string.IsNullOrWhiteSpace(protocol.StudySetting)) { errors.Add("❌ Cadre de l'étude (Lieu/Période) manquant."); methoIncomplete = true; }
            if (string.IsNullOrWhiteSpace(protocol.StudyPopulation)) { errors.Add("❌ Population d'étude manquante."); methoIncomplete = true; }
            
            if (string.IsNullOrWhiteSpace(protocol.InclusionCriteria) && string.IsNullOrWhiteSpace(protocol.ExclusionCriteria)) 
            { 
                suggestions.Add("⚠️ Aucun critère d'éligibilité (inclusion/exclusion) défini."); 
                result.Score -= 5; 
            }

            if (string.IsNullOrWhiteSpace(protocol.SamplingMethod))
            {
                errors.Add("❌ Méthode d'échantillonnage manquante.");
                methoIncomplete = true;
            }
            else if (!protocol.SamplingMethod.Any(char.IsDigit) && !protocol.SamplingMethod.ToLower().Contains("exhaustif"))
            {
                suggestions.Add("💡 Pensez à préciser la taille de l'échantillon (chiffre) ou si c'est exhaustif.");
            }

            if (string.IsNullOrWhiteSpace(protocol.DataAnalysis))
            {
                errors.Add("❌ Plan d'analyse des données manquant.");
                methoIncomplete = true;
            }

            if (methoIncomplete)
            {
                result.Score -= 30;
            }
            else
            {
                // Bonus de cohérence si tout est rempli
                result.Score += 5;
            }

            // 5. Structure et Rigueur (20 pts)
            if (string.IsNullOrWhiteSpace(protocol.Ethics)) { suggestions.Add("⚠️ Considérations éthiques non mentionnées."); result.Score -= 5; }
            if (string.IsNullOrWhiteSpace(protocol.References)) { errors.Add("❌ Références bibliographiques manquantes (à débuter dès l'intro)."); result.Score -= 10; }

            // 6. Suggestions Statistiques Intelligentes (Nouveauté Sync Site)
            var statSuggestions = _suggester.SuggestTests(protocol);
            foreach(var s in statSuggestions) result.AddSuggestion(s);
            
            var advice = _suggester.GetMethodologicalAdvice(protocol);
            if (!string.IsNullOrEmpty(advice)) result.AddSuggestion(advice);

            // Score plancher à 0
            if (result.Score < 0) result.Score = 0;

            return result;
        }

        private Dictionary<string, int> GetBloomVerbs()
        {
            return new Dictionary<string, int>
            {
                // Niveau 1 : Connaissance
                { "définir", 1 }, { "lister", 1 }, { "nommer", 1 }, { "identifier", 1 },
                // Niveau 2 : Compréhension
                { "décrire", 2 }, { "expliquer", 2 }, { "discuter", 2 },
                // Niveau 3 : Application
                { "appliquer", 3 }, { "démontrer", 3 }, { "illustrer", 3 }, { "utiliser", 3 },
                // Niveau 4 : Analyse
                { "analyser", 4 }, { "comparer", 4 }, { "distinguer", 4 }, { "examiner", 4 },
                // Niveau 5 : Évaluation / Synthèse
                { "évaluer", 5 }, { "estimer", 5 }, { "juger", 5 }, { "valider", 5 }, { "concevoir", 5 }, { "formuler", 5 }
            };
        }
    }
}
