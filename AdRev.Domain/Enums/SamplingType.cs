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
        
        [Description("Raisonné (Purposive)")]
        Purposive,
        
        [Description("Boule de Neige")]
        Snowball,
        
        [Description("Par Quotas")]
        Quota,

        [Description("Théorique (Grounded Theory)")]
        Theoretical,
        
        // Autres
        [Description("Exhaustif (Recensement)")]
        Census
    }
}
