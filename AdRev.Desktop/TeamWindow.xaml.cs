using System;
using System.Collections.ObjectModel;
using System.Windows;
using AdRev.Domain.Models;
using System.Linq;

namespace AdRev.Desktop
{
    public partial class TeamWindow : Window
    {
        private ResearchProject _project;
        private ObservableCollection<Author> _teamMembers;

        public TeamWindow(ResearchProject project)
        {
            InitializeComponent();
            _project = project;

            // Initialize Team if list is null (should normally be init in model)
            if (_project.Team == null) _project.Team = new System.Collections.Generic.List<Author>();

            // Observable wrapper for UI
            _teamMembers = new ObservableCollection<Author>(_project.Team);
            TeamListBox.ItemsSource = _teamMembers;

            NewRoleBox.ItemsSource = System.Enum.GetValues(typeof(FunctionalRole));
            NewRoleBox.SelectedIndex = 0;
        }

        private void Invite_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewNameBox.Text)) {
                MessageBox.Show("Veuillez entrer un nom."); 
                return;
            }

            var member = new Author
            {
                LastName = NewNameBox.Text,
                Role = (FunctionalRole)NewRoleBox.SelectedItem,
                Email = NewEmailBox.Text,
                AccessLevel = (UserAccessLevel)(NewAccessBox.SelectedIndex >= 0 ? NewAccessBox.SelectedIndex : 1) // Default to Editor
            };

            // Add to UI List
            _teamMembers.Add(member);
            
            // Add to Project Model
            _project.Team.Add(member);


            MessageBox.Show($"Invitation envoyée à {member.Email} pour rejoindre le projet '{_project.Title}'.", "Invitation Envoyée", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear Form
            NewNameBox.Text = "";
            NewEmailBox.Text = "";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
