using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AdRev.Domain.Models;

namespace AdRev.Desktop.Views.Project
{
    public partial class ProjectDashboardView : UserControl
    {
        public ProjectDashboardView()
        {
            InitializeComponent();
        }

        public void LoadProject(ResearchProject project)
        {
            ProjectTitleText.Text = project.Title.ToUpper();
            SourceCount.Text = project.LibraryItems.Count.ToString();
            
            if (project.LibraryItems.Count > 0)
            {
                int readCount = project.LibraryItems.Count(i => i.Status == LibraryItemStatus.Read);
                double progress = (double)readCount / project.LibraryItems.Count * 100;
                ReadingProgressText.Text = $"{(int)progress}%";
                LibraryProgress.Value = progress;
                LibraryStatusText.Text = progress >= 100 ? "Terminé" : (progress > 0 ? "En cours" : "À débuter");
            }
            else
            {
                ReadingProgressText.Text = "0%";
                LibraryProgress.Value = 0;
                LibraryStatusText.Text = "Vide";
            }

            VariableCount.Text = (project.Variables.Count + project.QualitativeCodes.Count).ToString();
        }
    }
}
