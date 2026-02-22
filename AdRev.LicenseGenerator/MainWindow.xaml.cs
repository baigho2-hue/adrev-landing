using System;
<<<<<<< HEAD
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using AdRev.Core.Services;
using AdRev.Domain.Models;
using Microsoft.Win32;

namespace AdRev.LicenseGenerator
{
    public partial class MainWindow : Window
    {
        private readonly LicensingService _licensingService;
        private List<PurchaseRecord> _purchases = new List<PurchaseRecord>();
        private readonly string _dbPath;
=======
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace AdRev.LicenseGenerator
{
    public enum LicenseType { Lifetime, Annual, Institutional }

    public class LicenseMetadata
    {
        public string Hwid { get; set; } = string.Empty;
        public LicenseType Type { get; set; } = LicenseType.Lifetime;
        public DateTime ExpiryDate { get; set; } = DateTime.MaxValue;
        public int MaxSeats { get; set; } = 1;
        public string Signature { get; set; } = string.Empty;
    }

    public partial class MainWindow : Window
    {
        private const string SecretSalt = "AdRev_Security_2026_Robust_Research_System_V2";
>>>>>>> origin/main

        public MainWindow()
        {
            InitializeComponent();
<<<<<<< HEAD
            _licensingService = new LicensingService();
            
            // Setup DB path
            string adminDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AdRev", "Admin");
            if (!Directory.Exists(adminDir)) Directory.CreateDirectory(adminDir);
            _dbPath = Path.Combine(adminDir, "purchases.json");

            LoadPurchases();
        }

        #region DB LOGIC
        private void LoadPurchases()
        {
            try
            {
                if (File.Exists(_dbPath))
                {
                    string json = File.ReadAllText(_dbPath);
                    _purchases = JsonSerializer.Deserialize<List<PurchaseRecord>>(json) ?? new List<PurchaseRecord>();
                }
                else
                {
                    // Add some dummy data for the demonstration
                    _purchases = new List<PurchaseRecord>
                    {
                        new PurchaseRecord { FirstName = "Jean", LastName = "Dupont", Email = "jean.dupont@email.com", Hwid = "PC-SALLE-A101", RequestedType = LicenseType.Elite, PurchaseDate = DateTime.Now.AddDays(-2) },
                        new PurchaseRecord { FirstName = "Marie", LastName = "Curie", Email = "m.curie@research.org", Hwid = "LAB-WKS-02", RequestedType = LicenseType.Pro, PurchaseDate = DateTime.Now.AddDays(-1) }
                    };
                    SavePurchases();
                }
                PurchasesGrid.ItemsSource = _purchases;
                PurchasesGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                StatusLog.Text = "Erreur chargement base : " + ex.Message;
            }
        }

        private void SavePurchases()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_purchases, options);
                File.WriteAllText(_dbPath, json);
            }
            catch (Exception ex)
            {
                StatusLog.Text = "Erreur sauvegarde base : " + ex.Message;
            }
        }
        #endregion

        #region MANUAL TAB
=======
        }

>>>>>>> origin/main
        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AnnualOptions == null || InstitutionalOptions == null) return;

            var item = TypeCombo.SelectedItem as ComboBoxItem;
            string tag = item?.Tag?.ToString() ?? "";

<<<<<<< HEAD
            AnnualOptions.Visibility = (tag == "Pro" || tag == "Student" || tag == "Elite" || tag == "Enterprise") ? Visibility.Visible : Visibility.Collapsed;
            InstitutionalOptions.Visibility = (tag == "Enterprise") ? Visibility.Visible : Visibility.Collapsed;
=======
            AnnualOptions.Visibility = (tag == "Annual") ? Visibility.Visible : Visibility.Collapsed;
            InstitutionalOptions.Visibility = (tag == "Institutional") ? Visibility.Visible : Visibility.Collapsed;
>>>>>>> origin/main
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
            GenerateLicenseFromInputs();
        }

        private void GenerateLicenseFromInputs()
        {
            string hwid = HwidInput.Text.Trim().ToUpper();
            var item = TypeCombo.SelectedItem as ComboBoxItem;
            string tag = item?.Tag?.ToString() ?? "Unlimited";

            LicenseType type;
            DateTime expiryDate = DateTime.MaxValue;
            int maxSeats = 1;
            bool isBatch = false;

            switch (tag)
            {
                case "Student": type = LicenseType.Student; expiryDate = DateTime.UtcNow.AddYears(1); break;
                case "Pro": type = LicenseType.Pro; expiryDate = DateTime.UtcNow.AddYears(1); break;
                case "Elite": type = LicenseType.Elite; expiryDate = DateTime.UtcNow.AddYears(1); break;
                case "Enterprise":
                    type = LicenseType.Enterprise;
                    expiryDate = DateTime.UtcNow.AddYears(1);
                    maxSeats = (int)SeatsSlider.Value;
                    if (string.IsNullOrEmpty(hwid)) hwid = "ENTERPRISE-GENERIC";
                    isBatch = true;
                    break;
                case "Trial": type = LicenseType.Trial; expiryDate = DateTime.UtcNow.AddDays(7); break;
                default: type = LicenseType.Unlimited; break;
            }

            string email = EmailInput.Text.Trim();
            string baseLabel = LabelInput.Text.Trim();

            ProcessGeneration(hwid, type, maxSeats, expiryDate, email, baseLabel, isBatch);
        }

        private void ProcessGeneration(string hwid, LicenseType type, int maxSeats, DateTime expiryDate, string email, string label, bool isBatch)
        {
            try
            {
                if (isBatch)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine($"--- LOT DE {maxSeats} CLÉS ---");
                    for (int i = 1; i <= maxSeats; i++)
                    {
                        string specificLabel = string.IsNullOrEmpty(label) ? $"Poste {i}" : $"{label} - {i}";
                        string key = _licensingService.GenerateLicense(hwid, type, 1, expiryDate, email, specificLabel);
                        sb.AppendLine(key);
                        sb.AppendLine();
                    }
                    LicenseOutput.Text = sb.ToString();
                }
                else
                {
                    string encrypted = _licensingService.GenerateLicense(hwid, type, maxSeats, expiryDate, email, label);
                    LicenseOutput.Text = encrypted;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
=======
            string hwid = HwidInput.Text.Trim().ToUpper();
            var item = TypeCombo.SelectedItem as ComboBoxItem;
            string tag = item?.Tag?.ToString() ?? "Lifetime";

            var metadata = new LicenseMetadata();
            metadata.Hwid = hwid;

            if (tag == "Lifetime")
            {
                metadata.Type = LicenseType.Lifetime;
                metadata.ExpiryDate = DateTime.MaxValue;
                metadata.MaxSeats = 1;
            }
            else if (tag == "Annual")
            {
                metadata.Type = LicenseType.Annual;
                metadata.ExpiryDate = DateTime.UtcNow.AddYears(1);
                metadata.MaxSeats = 1;
            }
            else // Institutional
            {
                metadata.Type = LicenseType.Institutional;
                metadata.ExpiryDate = DateTime.MaxValue;
                metadata.MaxSeats = (int)SeatsSlider.Value;
                if (string.IsNullOrEmpty(hwid)) metadata.Hwid = "INSTITUTIONAL-GENERIC";
            }

            // Create Signature
            string dataToSign = $"{metadata.Hwid}|{metadata.Type}|{metadata.ExpiryDate:yyyy-MM-dd}|{metadata.MaxSeats}";
            metadata.Signature = HashString(dataToSign + SecretSalt);

            // Serialize and Encrypt
            string json = JsonSerializer.Serialize(metadata);
            string encrypted = EncryptString(json);

            LicenseOutput.Text = encrypted;
>>>>>>> origin/main
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LicenseOutput.Text))
            {
                Clipboard.SetText(LicenseOutput.Text);
<<<<<<< HEAD
                MessageBox.Show("Code copié !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region PURCHASE TAB
        private void RefreshPurchases_Click(object sender, RoutedEventArgs e)
        {
            LoadPurchases();
            StatusLog.Text = $"Données rafraîchies à {DateTime.Now:HH:mm:ss}";
        }

        private void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Fichiers CSV (*.csv)|*.csv" };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var lines = File.ReadAllLines(dialog.FileName);
                    foreach (var line in lines.Skip(1)) // Skip header
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 4)
                        {
                            _purchases.Add(new PurchaseRecord
                            {
                                FirstName = parts[0].Trim(),
                                LastName = parts[1].Trim(),
                                Email = parts[2].Trim(),
                                Hwid = parts[3].Trim(),
                                RequestedType = LicenseType.Pro,
                                PurchaseDate = DateTime.Now
                            });
                        }
                    }
                    SavePurchases();
                    PurchasesGrid.Items.Refresh();
                    StatusLog.Text = "Importation réussie.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur import CSV : " + ex.Message);
                }
            }
        }

        private void GenerateForPurchase_Click(object sender, RoutedEventArgs e)
        {
            var purchase = (sender as Button)?.DataContext as PurchaseRecord;
            if (purchase == null) return;

            // Prepare inputs for generation
            HwidInput.Text = purchase.Hwid;
            EmailInput.Text = purchase.Email;
            LabelInput.Text = $"Client: {purchase.FirstName} {purchase.LastName}";
            
            // Map type
            int typeIndex = 1; // Default to Pro
            if (purchase.RequestedType == LicenseType.Elite) typeIndex = 2;
            else if (purchase.RequestedType == LicenseType.Student) typeIndex = 3;
            TypeCombo.SelectedIndex = typeIndex;

            // Switch to generation tab or just generate here?
            // Let's generate and show the result
            GenerateLicenseFromInputs();
            
            // Mark as processed
            purchase.IsProcessed = true;
            purchase.GeneratedLicenseKey = LicenseOutput.Text;
            SavePurchases();
            PurchasesGrid.Items.Refresh();

            StatusLog.Text = $"Licence générée pour {purchase.Email}";
        }
        #endregion
    }
}


=======
                MessageBox.Show("Code de licence copié !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string HashString(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }

        private string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(SecretSalt.Substring(0, 32));
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }
    }
}
>>>>>>> origin/main
