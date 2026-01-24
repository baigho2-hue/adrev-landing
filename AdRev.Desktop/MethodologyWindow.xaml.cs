using System.Windows;
using AdRev.Core.Methodology;
using AdRev.Domain.Protocols;

namespace AdRev.Desktop
{
    public partial class MethodologyWindow : Window
    {
        private readonly MethodologyService _methodologyService;

        public MethodologyWindow(ResearchProtocol protocol)
        {
            InitializeComponent();
            _methodologyService = new MethodologyService();

            CheckProtocol(protocol);
        }

        private void CheckProtocol(ResearchProtocol protocol)
        {
            var check = _methodologyService.CheckProtocol(protocol);

            TitleStatusTextBlock.Text = check.TitleFilled ? "OK" : "Manquant";
            ResearchQuestionStatusTextBlock.Text = check.ResearchQuestionFilled ? "OK" : "Manquant";
            GeneralObjectiveStatusTextBlock.Text = check.GeneralObjectiveFilled ? "OK" : "Manquant";
            SpecificObjectivesStatusTextBlock.Text = check.SpecificObjectivesFilled ? "OK" : "Manquant";

            OverallStatusTextBlock.Text = check.IsValid() ? "Protocole valide ✅" : "Protocole incomplet ❌";
        }
    }
}
