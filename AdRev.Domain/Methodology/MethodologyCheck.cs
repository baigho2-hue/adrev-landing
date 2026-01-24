using AdRev.Domain.Protocols;

namespace AdRev.Domain.Methodology
{
    public class MethodologyCheck
    {
        public ResearchProtocol Protocol { get; private set; }
        public bool TitleFilled { get; private set; }
        public bool ResearchQuestionFilled { get; private set; }
        public bool GeneralObjectiveFilled { get; private set; }
        public bool SpecificObjectivesFilled { get; private set; }

        public MethodologyCheck(ResearchProtocol protocol)
        {
            Protocol = protocol;
            TitleFilled = !string.IsNullOrWhiteSpace(protocol.Title);
            ResearchQuestionFilled = !string.IsNullOrWhiteSpace(protocol.ResearchQuestion);
            GeneralObjectiveFilled = !string.IsNullOrWhiteSpace(protocol.GeneralObjective);
            SpecificObjectivesFilled = !string.IsNullOrWhiteSpace(protocol.SpecificObjectives);
        }

        public bool IsValid()
        {
            return TitleFilled && ResearchQuestionFilled &&
                   GeneralObjectiveFilled && SpecificObjectivesFilled;
        }
    }
}
