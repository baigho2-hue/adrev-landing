using AdRev.Domain.Protocols;
using AdRev.Domain.Variables;
using AdRev.Domain.Enums;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace AdRev.Core.Services
{
    public class QuestionnaireGenerator
    {
        public string GenerateMarkdownQuestionnaire(ResearchProtocol protocol)
        {
            var sb = new StringBuilder();

            // 1. En-tête
            sb.AppendLine("# FICHE D'ENQUÊTE / CAHIER D'OBSERVATION");
            sb.AppendLine("________________________________________________________________________________");
            sb.AppendLine($"**TITRE :** {protocol.Title}");
            sb.AppendLine($"**CODE ÉTUDE :** {protocol.Id.Substring(0, 8).ToUpper()}");
            sb.AppendLine($"**DATE :** ____ / ____ / ________");
            sb.AppendLine($"**ENQUÊTEUR :** __________________________________");
            sb.AppendLine($"**N° ID PARTICIPANT :** |__|__|__|__|__|");
            sb.AppendLine("________________________________________________________________________________\n");

            if (protocol.Variables == null || !protocol.Variables.Any())
            {
                sb.AppendLine("*Aucune variable définie pour le moment.*");
                return sb.ToString();
            }

            // 2. Regrouper par sections
            var groups = protocol.Variables
                .GroupBy(v => string.IsNullOrWhiteSpace(v.GroupName) ? "VARIABLES GÉNÉRALES" : v.GroupName)
                .OrderBy(g => g.Key == "VARIABLES GÉNÉRALES" ? 0 : 1); // Général en premier

            foreach (var group in groups)
            {
                sb.AppendLine($"## {group.Key.ToUpper()}");
                sb.AppendLine("--------------------------------------------------");

                foreach (var variable in group)
                {
                    sb.AppendLine(FormatVariable(variable));
                }
                sb.AppendLine(""); // Espace entre les groupes
            }

            // 3. Pied de page
            sb.AppendLine("________________________________________________________________________________");
            sb.AppendLine("FIN DE L'ENQUÊTE");
            sb.AppendLine("Merci de vérifier que toutes les questions obligatoires (*) sont remplies.");

            return sb.ToString();
        }

        private string FormatVariable(StudyVariable v)
        {
            var sb = new StringBuilder();
            
            // Marqueur obligatoire
            string req = v.IsRequired ? "(*)" : "";
            
            // Condition d'affichage (Visible uniquement si...)
            string condition = !string.IsNullOrWhiteSpace(v.VisibilityCondition) 
                ? $" [SI {v.VisibilityCondition}]" 
                : "";

            // Question
            sb.AppendLine($"**{v.Prompt}** {req}{condition}");
            
            // Zone de réponse selon le type
            switch (v.Type)
            {
                case VariableType.Text:
                    sb.AppendLine($"   [__________________________________________________] (Texte)");
                    break;
                
                case VariableType.Memo:
                    sb.AppendLine($"   [__________________________________________________]");
                    sb.AppendLine($"   [__________________________________________________]");
                    sb.AppendLine($"   [__________________________________________________]");
                    break;
                
                case VariableType.QuantitativeDiscrete:
                case VariableType.QuantitativeContinuous:
                    sb.AppendLine($"   |__|__|__|__|__| {(v.Type == VariableType.QuantitativeContinuous ? ", |__|__|" : "")}");
                    break;
                
                case VariableType.QuantitativeTemporal:
                    sb.AppendLine($"   __ / __ / ____ (JJ/MM/AAAA)");
                    break;
                
                case VariableType.Time:
                    sb.AppendLine($"   __ : __ (HH:MM)");
                    break;
                
                case VariableType.QualitativeBinary:
                    sb.AppendLine($"   [ ] OUI    [ ] NON");
                    if (!string.IsNullOrWhiteSpace(v.SkipLogic))
                    {
                         sb.AppendLine($"   -> {v.SkipLogic}");
                    }
                    break;
                
                case VariableType.QualitativeNominal:
                case VariableType.QualitativeOrdinal:
                case VariableType.MultipleChoice:
                    if (!string.IsNullOrWhiteSpace(v.ChoiceOptions))
                    {
                        var options = v.ChoiceOptions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var opt in options)
                        {
                            string box = v.Type == VariableType.MultipleChoice ? "[ ]" : "( )";
                            sb.AppendLine($"   {box} {opt.Trim()}");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"   ( ) Option 1   ( ) Option 2   ( ) ...");
                    }
                    
                    // Affichage de la logique de saut si présente
                    if (!string.IsNullOrWhiteSpace(v.SkipLogic))
                    {
                         sb.AppendLine($"   -> {v.SkipLogic}");
                    }
                    break;
                
                case VariableType.Calculated:
                    sb.AppendLine($"   [ CALCUL AUTOMATIQUE ]");
                    if (!string.IsNullOrWhiteSpace(v.CalculationFormula))
                        sb.AppendLine($"   Formule : {v.CalculationFormula}");
                    break;
                
                default:
                    sb.AppendLine($"   [____________________]");
                    break;
            }

            // Nom de variable technique (discret)
            sb.AppendLine($"   _{v.Name}_");
            sb.AppendLine(""); // Espace après la question

            return sb.ToString();
        }
    }
}
