using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AdRev.Desktop
{
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            LoadTopic("Intro");
        }

        private void Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem item && item.Tag is string topic)
            {
                // Unselect others
                if (listBox != MenuGeneral) MenuGeneral.SelectedIndex = -1;
                if (listBox != MenuFeatures) MenuFeatures.SelectedIndex = -1;
                if (listBox != MenuSupport) MenuSupport.SelectedIndex = -1;

                // Restore selection if it was cleared by the above
                listBox.SelectedItem = item; 
                
                LoadTopic(topic);
                if (TopicBreadcrumb != null) TopicBreadcrumb.Text = $"Documentation > {item.Content}";
            }
        }

        public void LoadTopic(string topic)
        {
            if (TopicTitle == null || ContentRichText == null) return;

            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Segoe UI");
            doc.FontSize = 14;
            doc.PagePadding = new Thickness(0);

            switch (topic)
            {
                case "Intro":
                    TopicTitle.Text = "Bienvenue dans AdRev";
                    AddParagraph(doc, "AdRev est votre plateforme intégrée pour la recherche médicale, conçue pour simplifier la conception de protocoles, la gestion de données et l'analyse statistique.", true);
                    AddImage(doc, "Assets/help_dashboard.png");
                    AddHeader(doc, "🚀 Démarrage Rapide");
                    AddParagraph(doc, "1. **Création** : Commencez par créer un nouveau projet depuis le tableau de bord.");
                    AddParagraph(doc, "2. **Étapes** : Suivez le flux de travail guidé : Protocole → Variables → Collecte → Analyse → Rapport.");
                    AddParagraph(doc, "3. **Sauvegarde** : Utilisez l'icône de sauvegarde rapide dans la barre de titre pour ne rien perdre.");
                    break;

                case "Dashboard":
                    TopicTitle.Text = "Tableau de Bord Central";
                    AddParagraph(doc, "Le point d'entrée de tous vos travaux de recherche.", true);
                    AddHeader(doc, "Éléments Clés");
                    AddBullet(doc, "Projets Récents : Affiche vos derniers travaux avec leur statut d'avancement.");
                    AddBullet(doc, "Nouveau Projet : Lance l'assistant de création guidée.");
                    AddBullet(doc, "Importation Directe : Pour analyser un fichier Excel/CSV sans créer de projet complet.");
                    AddHeader(doc, "Statistiques de Session");
                    AddParagraph(doc, "La barre latérale affiche des statistiques sur votre productivité et l'état global de vos bases de données.");
                    break;

                case "Protocol":
                    TopicTitle.Text = "Conception du Protocole (IMRAD/SPIRIT)";
                    AddParagraph(doc, "L'éditeur de protocole assure que votre étude respecte les standards scientifiques internationaux.", true);
                    AddImage(doc, "Assets/help_protocol.png");
                    AddHeader(doc, "Navigation par Étapes");
                    AddParagraph(doc, "Le menu latéral gauche vous permet de naviguer à travers les 14 étapes cruciales :");
                    AddBullet(doc, "Introduction & Contexte : Justification scientifique de l'étude.");
                    AddBullet(doc, "Objectifs : Définition précise (Général et Spécifiques).");
                    AddBullet(doc, "Méthodologie : Type d'étude, population, et critères d'éligibilité.");
                    AddBullet(doc, "Échantillonnage : Utilisez les calculatrices (Cochran, etc.) pour déterminer la taille N.");
                    AddHeader(doc, "Citations Bibliographiques");
                    AddParagraph(doc, "Cliquez sur 'Citer' pour ajouter des références. AdRev gère automatiquement les styles Vancouver et APA.");
                    break;

                case "Analysis":
                    TopicTitle.Text = "Analyse Statistique & Qualitative";
                    AddParagraph(doc, "Un outil de traitement de données puissant sans la complexité des logiciels traditionnels.", true);
                    AddImage(doc, "Assets/help_analysis.png");
                    AddHeader(doc, "Analyses Quantitatives");
                    AddBullet(doc, "Descriptives : Tableaux de fréquences, moyennes, écart-types.");
                    AddBullet(doc, "Inférentielles : Automatisation des tests selon la nature des variables (Chi2, Student, ANOVA).");
                    AddBullet(doc, "Modélisation : Régression linéaire et logistique simplifiée.");
                    AddHeader(doc, "Analyses Qualitatives");
                    AddParagraph(doc, "Utilisez l'Atelier de Codage pour identifier les thèmes dans vos entretiens ou focus groups.");
                    break;

                case "Discussion":
                    TopicTitle.Text = "Aide à la Discussion & Publication";
                    AddParagraph(doc, "L'étape finale pour transformer vos résultats en publications scientifiques.", true);
                    AddHeader(doc, "Assistant Discussion 💬");
                    AddParagraph(doc, "Un module interactif qui vous aide à structurer vos arguments selon le schéma IMRAD.");
                    AddHeader(doc, "Exportation Word");
                    AddParagraph(doc, "Générez en un clic votre protocole, votre manuel de variables, ou votre manuscrit final au format .docx.");
                    break;

                case "FAQ":
                    TopicTitle.Text = "FAQ & Support";
                    AddHeader(doc, "Besoin d'aide ?");
                    AddParagraph(doc, "Pour toute question technique ou commerciale, contactez-nous à : baigho2@gmail.com");
                    AddParagraph(doc, "Version : 1.0 - Janvier 2026");
                    break;

                case "About":
                    TopicTitle.Text = "À propos d'AdRev";
                    AddParagraph(doc, "AdRev Desktop App", true);
                    AddParagraph(doc, "Version 1.0.0 (Édition Initiale)");
                    AddParagraph(doc, "© 2024-2026 AdRev Science Team. Tous droits réservés.");
                    
                    AddHeader(doc, "🌍 Offre Spéciale Mali");
                    AddParagraph(doc, "AdRev s'engage pour la recherche au Mali avec des tarifs adaptés :");
                    AddBullet(doc, "Pack Étudiant : 10 000 FCFA (1 an) / 20 000 FCFA (3 ans)");
                    AddBullet(doc, "Pack Professionnel : 45 000 FCFA (Renouvellement : 25 000 FCFA)");
                    AddBullet(doc, "Pack Elite : 75 000 FCFA (Renouvellement : 50 000 FCFA)");
                    AddBullet(doc, "Pack Institutionnel : 500 000 FCFA / an (7 PC)");
                    AddBullet(doc, "Version Illimitée : 1 000 000 FCFA (Paiement unique)");
                    AddParagraph(doc, "Paiements : Orange Money au 00223 79 27 64 70", false);
                    AddParagraph(doc, "Envoyez votre confirmation de paiement à : baigho2@gmail.com", true);

                    AddHeader(doc, "Crédits");
                    AddParagraph(doc, "Développé par l'équipe AdRev avec WPF & .NET 8.");
                    AddParagraph(doc, "Icônes par Segoe MDL2 Assets.");
                    break;
            }

            ContentRichText.Document = doc;
        }

        private void AddHeader(FlowDocument doc, string text)
        {
            var run = new Run(text) { Foreground = Brushes.Black };
            doc.Blocks.Add(new Paragraph(run) 
            { 
                FontSize = 18, 
                FontWeight = FontWeights.Bold, 
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 20, 0, 10)
            });
        }

        private void AddParagraph(FlowDocument doc, string text, bool isBold = false)
        {
            var run = new Run(text);
            run.Foreground = Brushes.Black;
            var p = new Paragraph(run);
            p.Foreground = Brushes.Black;
            if (isBold) p.FontWeight = FontWeights.SemiBold;
            p.Margin = new Thickness(0, 0, 0, 10);
            doc.Blocks.Add(p);
        }

        private void AddBullet(FlowDocument doc, string text)
        {
            var list = new List();
            list.MarkerStyle = TextMarkerStyle.Disc;
            list.Margin = new Thickness(10, 0, 0, 0);
            var run = new Run(text);
            run.Foreground = Brushes.Black;
            var p = new Paragraph(run);
            p.Foreground = Brushes.Black;
            list.ListItems.Add(new ListItem(p));
            doc.Blocks.Add(list);
        }

        private void AddImage(FlowDocument doc, string assetPath)
        {
            try
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assetPath);
                // Fallback if running from IDE/different path
                if (!File.Exists(fullPath)) fullPath = Path.GetFullPath(assetPath);

                if (File.Exists(fullPath))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(fullPath));
                    var image = new Image { Source = bitmap, MaxWidth = 600, Stretch = Stretch.Uniform, Margin = new Thickness(0, 10, 0, 15) };
                    
                    var container = new InlineUIContainer(image);
                    var p = new Paragraph(container);
                    doc.Blocks.Add(p);
                }
            }
            catch { /* Silent fail for images */ }
        }

        private void OpenPdfManual_Click(object sender, RoutedEventArgs e)
        {
            string pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MANUEL_UTILISATEUR.pdf");
            string mdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MANUEL_UTILISATEUR.md");

            string? fileToOpen = null;

            if (File.Exists(pdfPath))
            {
                fileToOpen = pdfPath;
            }
            else if (File.Exists("MANUEL_UTILISATEUR.pdf")) // Check current dir just in case
            {
                fileToOpen = Path.GetFullPath("MANUEL_UTILISATEUR.pdf");
            }
            else if (File.Exists(mdPath))
            {
                var result = MessageBox.Show("Le manuel PDF n'est pas encore généré. Voulez-vous ouvrir la version texte (Markdown) ?", "PDF Introuvable", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    fileToOpen = mdPath;
                }
                else return;
            }
            else
            {
                MessageBox.Show("Le manuel d'utilisation est introuvable.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (fileToOpen != null)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(fileToOpen) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ouverture du manuel : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
