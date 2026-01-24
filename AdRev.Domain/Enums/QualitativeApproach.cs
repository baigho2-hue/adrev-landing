using System.ComponentModel;

namespace AdRev.Domain.Enums
{
    public enum QualitativeApproach
    {
        [Description("Inductive (Grounded Theory, Phénoménologie)")]
        Inductive,

        [Description("Déductive (Framework Analysis, Analyse de contenu dirigée)")]
        Deductive,

        [Description("Mixte / Hybride")]
        Mixed
    }
}
