namespace AdRev.Domain.Variables
{
    public class RecodeInstruction
    {
        // For simple replacement (A -> B)
        public string SourceValue { get; set; } = string.Empty;
        
        // For range mapping (Min <= X < Max) -> Target
        // Use double? to allow nulls (open ranges)
        public double? RangeMin { get; set; }
        public double? RangeMax { get; set; }
        
        public string TargetValue { get; set; } = string.Empty;
        
        public bool IsRange { get; set; }
    }
}
