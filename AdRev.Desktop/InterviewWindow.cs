using System;
using System.Windows;
using System.Windows.Media;

namespace AdRev.Desktop
{
    // Simple placeholder window for Interview/Focus Group
    public class InterviewWindow : Window
    {
        public bool IsFocusGroup { get; }

        public InterviewWindow(bool isFocusGroup)
        {
            IsFocusGroup = isFocusGroup;
            Title = isFocusGroup ? "Focus Group Session" : "Entretien Individuel";
            Width = 800;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = Brushes.White;

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

            // Header
            var header = new System.Windows.Controls.TextBlock 
            { 
                Text = Title, 
                FontSize = 24, 
                FontWeight = FontWeights.Bold, 
                Margin = new Thickness(20),
                Foreground = Brushes.DarkSlateBlue
            };
            grid.Children.Add(header);

            // Mock Content
            var content = new System.Windows.Controls.TextBlock
            {
                Text = isFocusGroup 
                    ? "Module de gestion de Focus Group en cours de développement.\nFonctionnalités prévues : attribution des paroles, chronométrage, notes d'observation."
                    : "Module de transcription d'entretien individuel.\nFonctionnalités prévues : enregistrement audio, prise de notes synchronisée, codage à la volée.",
                Margin = new Thickness(20),
                FontSize = 14,
                Foreground = Brushes.Gray,
                TextWrapping = TextWrapping.Wrap
            };
            System.Windows.Controls.Grid.SetRow(content, 1);
            grid.Children.Add(content);

            Content = grid;
        }
    }
}
