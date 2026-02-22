using System.Windows;
using AdRev.Core.Services;

namespace AdRev.Desktop.Windows
{
    public partial class ProfileWindow : Window
    {
        private readonly ResearcherProfileService _service = new ResearcherProfileService();
        public ResearcherProfile Profile { get; private set; }

        public ProfileWindow()
        {
            InitializeComponent();
            Profile = _service.GetProfile();
            
            NameBox.Text = Profile.FullName;
            TitleBox.Text = Profile.Title;
            InstitutionBox.Text = Profile.Institution;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Profile.FullName = NameBox.Text;
            Profile.Title = TitleBox.Text;
            Profile.Institution = InstitutionBox.Text;

            _service.SaveProfile(Profile);
            DialogResult = true;
            Close();
        }
    }
}
