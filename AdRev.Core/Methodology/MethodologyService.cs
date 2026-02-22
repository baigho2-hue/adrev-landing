using AdRev.Domain.Methodology;
using AdRev.Domain.Protocols;

namespace AdRev.Core.Methodology
{
    public class MethodologyService
    {
        public MethodologyCheck CheckProtocol(ResearchProtocol protocol)
        {
            return new MethodologyCheck(protocol);
        }
    }
}
