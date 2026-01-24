using System;
using System.Windows;
using AdRev.Core.Services;

namespace AdRev.Desktop
{
    public partial class ActivationWindow : Window
    {
        private readonly LicensingService _licensingService = new LicensingService();

        public ActivationWindow()
        {
            InitializeComponent();
            HwidText.Text = _licensingService.GetHardwareId();
            
            // Check current status
            if (_licensingService.IsActivated(out string status))
            {
                // Technically shouldn't be here if App.xaml.cs works, but good for feedback
                LicenseKeyBox.Text = "LOGICIEL ACTIVÉ";
                LicenseKeyBox.IsEnabled = false;
            }
        }

        private void CopyHwid_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(HwidText.Text);
            MessageBox.Show("Identifiant matériel copié dans le presse-papier.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            string key = LicenseKeyBox.Text.Trim();
            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("Veuillez entrer un code d'activation.", "Champs requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_licensingService.Activate(key))
            {
                MessageBox.Show("AdRev a été activé avec succès ! Merci de votre confiance.", "Activation Réussie", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Code d'activation invalide ou expiré pour cet ordinateur.", "Erreur d'activation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
