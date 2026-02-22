using System.ComponentModel;

namespace AdRev.Domain.Enums
{
    public enum ScientificDomain
    {
        [Description("Biomédecine & Santé")]
        Biomedical,

        [Description("Sciences Sociales & Humaines")]
        SocialSciences,

        [Description("Histoire & Archéologie")]
        History,

        [Description("Géographie & Sciences de la Terre")]
        Geography,

        [Description("Espace & Physique")]
        SpacePhysics,

        [Description("Ingénierie & Technologie")]
        Engineering,

        [Description("Économie & Gestion")]
        Economics
    }
}
