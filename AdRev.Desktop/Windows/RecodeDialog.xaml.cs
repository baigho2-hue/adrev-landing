using System.Collections.Generic;
using System.Windows;
using AdRev.Domain.Variables;
using System.Collections.ObjectModel;

namespace AdRev.Desktop.Windows
{
    public partial class RecodeDialog : Window
    {
        public ObservableCollection<RecodeViewModel> Rules { get; set; } = new ObservableCollection<RecodeViewModel>();
        public List<RecodeInstruction> ResultInstructions { get; private set; } = new List<RecodeInstruction>();

        public RecodeDialog(List<RecodeInstruction> existingRules)
        {
            InitializeComponent();
            Rules = new ObservableCollection<RecodeViewModel>();
            if (existingRules != null)
            {
                foreach (var r in existingRules)
                {
                    Rules.Add(new RecodeViewModel 
                    { 
                        IsRange = r.IsRange,
                        RangeMin = r.RangeMin?.ToString() ?? string.Empty,
                        RangeMax = r.RangeMax?.ToString() ?? string.Empty,
                        SourceValue = r.SourceValue ?? string.Empty,
                        TargetValue = r.TargetValue ?? string.Empty
                    });
                }
            }
            RulesGrid.ItemsSource = Rules;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ResultInstructions = new List<RecodeInstruction>();
            foreach (var vm in Rules)
            {
                var instr = new RecodeInstruction
                {
                    IsRange = vm.IsRange,
                    SourceValue = vm.SourceValue,
                    TargetValue = vm.TargetValue
                };
                
                if (double.TryParse(vm.RangeMin, out double min)) instr.RangeMin = min;
                if (double.TryParse(vm.RangeMax, out double max)) instr.RangeMax = max;

                ResultInstructions.Add(instr);
            }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class RecodeViewModel
    {
        public bool IsRange { get; set; }
        public string RangeMin { get; set; } = string.Empty;
        public string RangeMax { get; set; } = string.Empty;
        public string SourceValue { get; set; } = string.Empty;
        public string TargetValue { get; set; } = string.Empty;
    }
}
