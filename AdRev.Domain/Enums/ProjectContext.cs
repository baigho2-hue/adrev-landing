using System.ComponentModel;

namespace AdRev.Domain.Enums
{
    public enum ProjectContext
    {
        [Description("Projet de Thèse Médecine")]
        MedicalThesis,
        
        [Description("Projet de Mémoire de Spécialisation")]
        SpecializationThesis,
        
        [Description("Projet d'évaluation ONG")]
        ONGEvaluation,
        
        [Description("Projet de recherche ONG")]
        ONGResearch,
        
        [Description("Projet National")]
        NationalProject,

        [Description("Autre")]
        Other
    }
}
