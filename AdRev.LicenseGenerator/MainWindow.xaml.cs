using System;
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AnnualOptions == null || InstitutionalOptions == null) return;

            var item = TypeCombo.SelectedItem as ComboBoxItem;
            string tag = item?.Tag?.ToString() ?? "";

            AnnualOptions.Visibility = (tag == "Annual") ? Visibility.Visible : Visibility.Collapsed;
            InstitutionalOptions.Visibility = (tag == "Institutional") ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
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
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LicenseOutput.Text))
            {
                Clipboard.SetText(LicenseOutput.Text);
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
