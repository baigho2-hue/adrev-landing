using System.ComponentModel;

namespace AdRev.Domain.Enums
{
    public enum EpidemiologicalStudyType
    {
        [Description("Non spécifié")]
        None,

        // Observationnelles Descriptives
        [Description("Transversale (Prévalence)")]
        CrossSectionalDescriptive,
        
        [Description("Longitudinale (Incidence)")]
        LongitudinalIncidence,
        
        [Description("Écologique (Corrélationnelle)")]
        Ecological,
        
        [Description("Série de Cas")]
        CaseSeries,

        // Observationnelles Analytiques
        [Description("Cas-Témoins")]
        CaseControl,
        
        [Description("Cohorte Prospective")]
        CohortProspective,
        
        [Description("Cohorte Rétrospective")]
        CohortRetrospective,
        
        [Description("Transversale Analytique")]
        CrossSectionalAnalytic,

        // Expérimentales (Interventionnelles)
        [Description("Essai Randomisé Contrôlé (ERC)")]
        RandomizedControlledTrial,
        
        [Description("Essai Quasi-Expérimental")]
        QuasiExperimental,
        
        [Description("Essai Avant-Après")]
        BeforeAfterStudy,

        // Synthèse
        [Description("Revue Systématique / Méta-analyse")]
        SystematicReviewMetaAnalysis
    }
}
