using System.Windows;

namespace AdRev.Desktop.Windows
{
    public partial class PromptWindow : Window
    {
        public string Result => InputTextBox.Text;

        public PromptWindow(string title, string instruction)
        {
            InitializeComponent();
            this.Title = title;
            this.InstructionText.Text = instruction;
            InputTextBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}


