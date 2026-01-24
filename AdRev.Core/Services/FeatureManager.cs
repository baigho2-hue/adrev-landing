using AdRev.Domain.Models;
using System;

namespace AdRev.Core.Services
{
    public enum AppFeature
    {
        BasicProtocol,          // Student+
        AdvancedSampling,       // Pro+ (Multi-stage, Cluster)
        DescriptiveStats,       // Student+
        InferentialStats,       // Pro+ (T-Test, ANOVA, Chi2)
        RegressionAnalysis,     // Elite+ (Linear, Logistic)
        QualitativeAnalysis,     // Elite+ (Coding, Themes)
        QualityValidation,      // Pro+ (Checklists CONSORT/STROBE)
        BloomValidation,        // Student+
        StatisticalSuggester,   // Pro+
        CloudSync,              // Institution/Elite only
        AutomaticCRF            // Pro+
    }

    public class FeatureManager
    {
        private readonly LicensingService _licensingService;

        public FeatureManager(LicensingService licensingService)
        {
            _licensingService = licensingService;
        }

        public bool IsFeatureAvailable(AppFeature feature)
        {
            var license = _licensingService.GetCurrentLicense();
            if (license == null) return false;

            switch (license.Type)
            {
                case LicenseType.Trial:
                    // Full access during trial
                    return true;

                case LicenseType.Student:
                    return feature == AppFeature.BasicProtocol ||
                           feature == AppFeature.DescriptiveStats ||
                           feature == AppFeature.BloomValidation;

                case LicenseType.Annual: // Assuming Professional
                case LicenseType.Lifetime:
                    return feature != AppFeature.RegressionAnalysis &&
                           feature != AppFeature.QualitativeAnalysis &&
                           feature != AppFeature.CloudSync;

                case LicenseType.Enterprise:
                    return true; // All features

                default:
                    return false;
            }
        }

        public string GetTierName()
        {
            var license = _licensingService.GetCurrentLicense();
            if (license == null) return "Non activé";

            return license.Type switch
            {
                LicenseType.Student => "Édition Étudiant",
                LicenseType.Annual => "Édition Professionnelle",
                LicenseType.Lifetime => "Édition Professionnelle",
                LicenseType.Enterprise => "Édition Institutionnelle / Elite",
                LicenseType.Trial => "Période d'Essai (Full Access)",
                _ => "Inconnu"
            };
        }

        public string GetUpgradeMessage(AppFeature feature)
        {
            return $"🔒 La fonctionnalité '{TranslateFeature(feature)}' nécessite une version supérieure d'AdRev.";
        }

        private string TranslateFeature(AppFeature feature)
        {
            return feature switch
            {
                AppFeature.AdvancedSampling => "Échantillonnage Complexe",
                AppFeature.InferentialStats => "Tests Inférentiels (ANOVA, T-Test)",
                AppFeature.RegressionAnalysis => "Analyses de Régression",
                AppFeature.QualitativeAnalysis => "Atelier Qualitatif",
                AppFeature.QualityValidation => "Vérification CONSORT/STROBE",
                AppFeature.CloudSync => "Synchronisation Cloud",
                AppFeature.AutomaticCRF => "Générateur de CRF",
                _ => feature.ToString()
            };
        }
    }
}
