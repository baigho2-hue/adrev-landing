using System.ComponentModel;

namespace AdRev.Domain.Enums
{
    public enum SamplingType
    {
        [Description("Non spécifié")]
        None,

        // Échantillonnage probabiliste
        [Description("Aléatoire Simple")]
        SimpleRandom,
        
        [Description("Systématique")]
        Systematic,
        
        [Description("Stratifié")]
        Stratified,
        
        [Description("En Grappe (Cluster)")]
        ClusterSampling,
        
        [Description("À Plusieurs Degrés")]
        MultiStage,
        
        [Description("Stratifié en Grappes")]
        StratifiedCluster,

        // Échantillonnage non probabiliste
        [Description("De Convenance")]
        Convenience,
        
        [Description("Théorique (Grounded Theory)")]
        Theoretical,

        [Description("Boule de Neige (Snowball)")]
        Snowball,

        [Description("Par Quotas (Qualitatif)")]
        Quota,
        
        // Échantillonnage Qualitatif Avancé
        [Description("Intentionnel (Purposeful)")]
        Purposeful,

        [Description("Intentionnel: Cas Typique")]
        PurposefulTypical,

        [Description("Intentionnel: Cas Extrême / Déviant")]
        PurposefulExtreme,

        [Description("Intentionnel: Cas Critique")]
        PurposefulCritical,

        [Description("Intentionnel: Homogène")]
        PurposefulHomogeneous,

        [Description("Intentionnel: Hétérogène (Variation Maximale)")]
        PurposefulHeterogeneous,

        [Description("Étude de Cas (Cas Multiples)")]
        CaseStudy,

        [Description("Étude de Cas: Cas Contrastés")]
        CaseStudyContrasted,

        [Description("Étude de Cas: Cas Similaires")]
        CaseStudySimilar,

        [Description("Étude de Cas: Cas Critiques")]
        CaseStudyCritical,

        [Description("Étude de Cas: Cas Exemplaires")]
        CaseStudyExemplary,

        [Description("Événementiel ou Contextuel")]
        EventContextual,

        [Description("Par Saturation (Critère d'arrêt)")]
        Saturation,
        
        // Autres
        [Description("Exhaustif (Recensement)")]
        Census
    }
}
