using System.ComponentModel;

namespace AdRev.Domain.Enums
{
    public enum AcademicLevel
    {
        [Description("Thèse de Doctorat")]
        Thesis,
        
        [Description("Mémoire de Master")]
        MasterMemoir,
        
        [Description("Mémoire de Licence")]
        BachelorMemoir,
        
        [Description("Article Scientifique")]
        ScientificArticle,
        
        [Description("Rapport de Stage / Etude")]
        TechnicalReport,

        [Description("Rapport ONG / Humanitaire")]
        NGOReport,

        [Description("Autre")]
        Other
    }
}
