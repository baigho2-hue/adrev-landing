using AdRev.Domain.Enums;
using AdRev.Domain.Protocols;
using System;
using System.Linq;
using System.Text;

namespace AdRev.Core.Protocols
{
    /// <summary>
    /// Service pour générer des descriptions textuelles de la méthodologie d'échantillonnage
    /// </summary>
    public class SamplingDescriptionGenerator
    {
        /// <summary>
        /// Génère une description complète de la méthodologie d'échantillonnage
        /// incluant les ajustements pour l'effet de plan et les perdus de vue
        /// </summary>
        public string GenerateFullDescription(ResearchProtocol protocol, int baseSampleSize)
        {
            var sb = new StringBuilder();
            bool isQualitative = protocol.StudyType == StudyType.Qualitative; // Assumant que cet enum existe ou similar

            // Type d'échantillonnage
            sb.Append(GetSamplingTypeDescription(protocol.SamplingType, isQualitative));

            // Étude qualitative : Mention de la saturation
            if (isQualitative)
            {
                sb.Append(" La taille d'échantillon prévisionnelle est basée sur le principe de saturation des données ");
                if (protocol.SamplingType == SamplingType.Purposive) 
                    sb.Append("(diversification maximale jusqu'à redondance de l'information).");
                else
                    sb.Append("plutôt que sur une représentativité statistique.");
            }

            // Étude multicentrique
            if (protocol.IsMulticentric)
            {
                var centers = protocol.StudyCenters.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                int centerCount = centers.Length;
                
                // Adaptation terminologique pour le qualitatif
                string centerTerm = isQualitative ? "sites d'enquête" : "centres";
                
                sb.Append($" Cette étude multicentrique se déroulera dans {centerCount} {centerTerm}{(centerCount > 1 && !isQualitative ? "s" : "")}");
                
                if (centerCount > 0 && centerCount <= 5)
                {
                    sb.Append($" ({string.Join(", ", centers.Take(3))}{(centerCount > 3 ? ", etc." : "")})");
                }
                sb.Append(".");
            }

            // Stratification (Qualitatif : souvent appelé "Diversification")
            if (protocol.IsStratified && !string.IsNullOrWhiteSpace(protocol.StratificationCriteria))
            {
                if (isQualitative)
                    sb.Append($" La sélection des participants sera diversifiée selon : {protocol.StratificationCriteria} (échantillonnage raisonné stratifié).");
                else
                    sb.Append($" La population sera stratifiée selon : {protocol.StratificationCriteria}.");
            }

            // Échantillonnage en grappe
            if (protocol.IsClusterSampling)
            {
                if (isQualitative)
                {
                     sb.Append($" Une approche par sites/groupes (grappes) sera utilisée.");
                     if (protocol.ClusterSize > 0)
                        sb.Append($" Chaque groupe de discussion (Focus Group) comptera environ {protocol.ClusterSize} participants.");
                }
                else
                {
                    sb.Append($" L'échantillonnage en grappe sera utilisé");
                    if (protocol.ClusterSize > 0)
                        sb.Append($", avec une taille moyenne de {protocol.ClusterSize} sujets par grappe");
                    sb.Append(".");
                }
                
                // Ajustement pour design effect (Uniquement Quantitatif)
                if (!isQualitative && protocol.DesignEffect > 1.0)
                {
                    int adjustedSize = (int)Math.Ceiling(baseSampleSize * protocol.DesignEffect);
                    sb.Append($" Un effet de plan (Design Effect) de {protocol.DesignEffect:F2} a été appliqué, " +
                             $"augmentant la taille d'échantillon de {baseSampleSize} à {adjustedSize} sujets.");
                    baseSampleSize = adjustedSize;
                }
            }

            // Ajustement pour perdus de vue / abandons
            if (protocol.ExpectedLossRate > 0)
            {
                int finalSize = (int)Math.Ceiling(baseSampleSize / (1.0 - protocol.ExpectedLossRate / 100.0));
                
                if (isQualitative)
                     sb.Append($" En anticipant un taux d'abandon ou de refus de {protocol.ExpectedLossRate:F0}%, " +
                         $"nous visons de recruter environ {finalSize} participants pour atteindre la saturation.");
                else
                    sb.Append($" En anticipant un taux de perdus de vue de {protocol.ExpectedLossRate:F0}%, " +
                         $"la taille finale de l'échantillon sera de {finalSize} sujets.");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Génère une description courte du type d'échantillonnage
        /// </summary>
        private string GetSamplingTypeDescription(SamplingType samplingType, bool isQualitative = false)
        {
            switch (samplingType)
            {
                case SamplingType.SimpleRandom:
                    return "Un échantillonnage aléatoire simple sera utilisé, garantissant une probabilité égale de sélection pour chaque sujet.";
                
                case SamplingType.Systematic:
                    return "Un échantillonnage systématique sera utilisé, avec une sélection régulière des sujets selon un intervalle prédéfini.";
                
                case SamplingType.Stratified:
                    return isQualitative 
                        ? "Un échantillonnage raisonné stratifié sera utilisé pour assurer la diversité des points de vue." 
                        : "Un échantillonnage stratifié sera utilisé pour assurer la représentativité de tous les sous-groupes de la population.";
                
                case SamplingType.ClusterSampling:
                    return isQualitative
                        ? "Un échantillonnage par groupes (cluster) sera utilisé, adapté notamment pour les Focus Groups."
                        : "Un échantillonnage en grappe (cluster sampling) sera utilisé, avec une sélection de groupes naturels de la population.";
                
                case SamplingType.MultiStage:
                    return "Un échantillonnage à plusieurs degrés sera utilisé, avec une sélection progressive en plusieurs étapes.";
                
                case SamplingType.StratifiedCluster:
                    return "Un échantillonnage stratifié en grappes sera utilisé, combinant les avantages de la stratification et de l'échantillonnage en grappe.";
                
                case SamplingType.Convenience:
                    return isQualitative
                        ? "Un échantillonnage de convenance sera utilisé, sélectionnant les participants disponibles et volontaires pour les entretiens."
                        : "Un échantillonnage de convenance sera utilisé, avec une sélection des sujets facilement accessibles.";
                
                case SamplingType.Purposive:
                    return "Un échantillonnage raisonné (purposive) sera utilisé, avec une sélection intentionnelle de sujets " + 
                           (isQualitative ? "capables d'apporter une information riche sur le phénomène étudié." : "ayant des caractéristiques spécifiques.");
                
                case SamplingType.Snowball:
                    return "Un échantillonnage « boule de neige » sera utilisé, où les participants recrutent d'autres participants (utile pour les populations difficiles d'accès).";
                
                case SamplingType.Quota:
                    return "Un échantillonnage par quotas sera utilisé pour respecter des proportions prédéfinies dans l'échantillon.";
                
                case SamplingType.Census:
                    return "Un recensement exhaustif de la population sera réalisé (pas d'échantillonnage).";
                
                default:
                    return "La méthode d'échantillonnage sera précisée ultérieurement.";
            }
        }

        /// <summary>
        /// Calcule le nombre de grappes nécessaires
        /// </summary>
        public int CalculateNumberOfClusters(int totalSampleSize, int averageClusterSize)
        {
            if (averageClusterSize <= 0) return 0;
            return (int)Math.Ceiling((double)totalSampleSize / averageClusterSize);
        }

        /// <summary>
        /// Calcule la taille ajustée pour l'effet de design
        /// </summary>
        public int CalculateDesignEffectAdjustedSize(int baseSampleSize, double designEffect)
        {
            return (int)Math.Ceiling(baseSampleSize * designEffect);
        }

        /// <summary>
        /// Calcule la taille finale avec ajustement pour perdus de vue
        /// </summary>
        public int CalculateLossAdjustedSize(int sampleSize, double expectedLossRatePercent)
        {
            if (expectedLossRatePercent <= 0 || expectedLossRatePercent >= 100) return sampleSize;
            return (int)Math.Ceiling(sampleSize / (1.0 - expectedLossRatePercent / 100.0));
        }

        /// <summary>
        /// Calcule le Design Effect à partir de l'ICC et de la taille de grappe
        /// </summary>
        public double CalculateDesignEffect(double icc, int averageClusterSize)
        {
            if (icc <= 0 || averageClusterSize <= 1) return 1.0;
            return 1.0 + (averageClusterSize - 1) * icc;
        }

        /// <summary>
        /// Génère des recommandations pour l'échantillonnage
        /// </summary>
        public string GenerateRecommendations(ResearchProtocol protocol)
        {
            var recommendations = new StringBuilder();

            // Recommandation sur l'effet de design
            if (protocol.IsClusterSampling && protocol.DesignEffect < 1.2)
            {
                recommendations.AppendLine("⚠️ L'effet de plan semble faible pour un échantillonnage en grappe. Valeurs typiques : 1.5-2.0");
            }

            // Recommandation sur les perdus de vue
            if (protocol.EpidemiologyType == EpidemiologicalStudyType.CohortProspective && protocol.ExpectedLossRate < 10)
            {
                recommendations.AppendLine("💡 Pour une cohorte prospective, considérez un taux de perdus de vue d'au moins 10-15%");
            }

            // Recommandation multicentrique
            if (protocol.IsMulticentric && string.IsNullOrWhiteSpace(protocol.StudyCenters))
            {
                recommendations.AppendLine("❗ Veuillez lister les centres participants pour une étude multicentrique");
            }

            // Recommandation stratification
            if (protocol.IsStratified && string.IsNullOrWhiteSpace(protocol.StratificationCriteria))
            {
                recommendations.AppendLine("❗ Précisez les critères de stratification (ex: âge, sexe, région)");
            }

            return recommendations.ToString();
        }
    }
}
