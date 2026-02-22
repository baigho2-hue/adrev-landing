using System;
using System.Windows;
<<<<<<< HEAD
using System.Diagnostics;
=======
>>>>>>> origin/main
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

<<<<<<< HEAD
        private void RequestLicense_Click(object sender, RoutedEventArgs e)
        {
            string hwid = HwidText.Text;
            string subject = "Commande Licence AdRev";
            string body = $"Bonjour,\n\nJe souhaite commander une licence AdRev.\n\nMon Hardware ID (HWID) : {hwid}\n\nType de licence souhaité : (Étudiant / Pro / Elite)\nMode de paiement : Orange Money (00223 79 27 64 70)\n\nCi-joint ma confirmation de paiement.";
            
            string mailto = $"mailto:baigho2@gmail.com?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";

            try
            {
                Process.Start(new ProcessStartInfo(mailto) { UseShellExecute = true });
            }
            catch (Exception)
            {
                MessageBox.Show($"Impossible d'ouvrir le client mail par défaut.\n\nVeuillez envoyer un email à baigho2@gmail.com avec votre HWID : {hwid}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

=======
>>>>>>> origin/main
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
