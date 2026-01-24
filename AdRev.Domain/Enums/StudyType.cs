namespace AdRev.Domain.Enums
{
    using System.ComponentModel;

    public enum StudyType
    {
        [Description("Quantitative")]
        Quantitative,
        
        [Description("Qualitative")]
        Qualitative,
        
        [Description("Mixte")]
        Mixed
    }
}
