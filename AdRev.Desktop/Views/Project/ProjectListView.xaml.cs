using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AdRev.Desktop;

namespace AdRev.Desktop.Views.Project
{
    public partial class ProjectListView : UserControl
    {
        public ObservableCollection<ProjectViewModel> RecentProjects { get; set; } = new ObservableCollection<ProjectViewModel>();
        public ObservableCollection<ProjectViewModel> AllProjects { get; set; } = new ObservableCollection<ProjectViewModel>();

        public ProjectListView()
        {
            InitializeComponent();
            InitializeData();
            
            // Binding
            RecentProjectsControl.ItemsSource = RecentProjects;
            AllProjectsGrid.ItemsSource = AllProjects;
        }

        private void InitializeData()
        {
            // Seed Recent Projects (7 items)
            RecentProjects.Clear();
            RecentProjects.Add(new ProjectViewModel { Name = "Étude Diabète Type 2 - Phase 1", Type = "Cohorte", Status = "En cours", LastModified = "Aujourd'hui, 09:30" });
            RecentProjects.Add(new ProjectViewModel { Name = "Essai Clinique Cardio", Type = "Interventionnel", Status = "Brouillon", LastModified = "Hier, 14:15" });
            RecentProjects.Add(new ProjectViewModel { Name = "Revue Systématique Ebola", Type = "Méta-analyse", Status = "Terminé", LastModified = "10 Jan 2024" });
            RecentProjects.Add(new ProjectViewModel { Name = "Enquête Sante Publique Mali", Type = "Transversale", Status = "En cours", LastModified = "08 Jan 2024" });
            RecentProjects.Add(new ProjectViewModel { Name = "Analyse Qualitative Stress", Type = "Phénoménologique", Status = "Terminé", LastModified = "05 Jan 2024" });
            RecentProjects.Add(new ProjectViewModel { Name = "Audit Clinique Urgences", Type = "Audit", Status = "En cours", LastModified = "02 Jan 2024" });
            RecentProjects.Add(new ProjectViewModel { Name = "Étude Pilote Vaccin", Type = "Pilote", Status = "Brouillon", LastModified = "28 Dec 2023" });

            // Seed All Projects (More items)
            AllProjects.Clear();
            foreach(var p in RecentProjects) AllProjects.Add(p);
            
            AllProjects.Add(new ProjectViewModel { Name = "Étude Nutritionnelle 2023", Type = "Transversale", Status = "Archivé", LastModified = "15 Nov 2023" });
            AllProjects.Add(new ProjectViewModel { Name = "Recherche Cancer du Sein", Type = "Case-Control", Status = "Terminé", LastModified = "30 Oct 2023" });
            AllProjects.Add(new ProjectViewModel { Name = "Enquête Satisfaction Patient", Type = "Qualitative", Status = "Archivé", LastModified = "20 Sep 2023" });
            AllProjects.Add(new ProjectViewModel { Name = "Projet VIH/SIDA Nord", Type = "Cohorte", Status = "Suspendu", LastModified = "10 Aout 2023" });
            AllProjects.Add(new ProjectViewModel { Name = "Formation Hygiène Hospitalière", Type = "Interventionnel", Status = "Terminé", LastModified = "05 Juil 2023" });
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ProjectViewModel vm)
            {
                 MessageBox.Show($"Ouverture du projet : {vm.Name}", "Ouverture");
                 // Logic to actually open would go here
            }
        }
    }

    // Reuse or redefine if not accessible
    // public class ProjectViewModel { ... } is already in MainWindow namespace ?
    // Assuming partial class structure or using the one from MainWindow if public.
    // Ideally should be in a separate file. For now, let's redefine narrowly or rely on using namespace.
}
