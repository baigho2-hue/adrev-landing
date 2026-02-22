using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using AdRev.Domain.MobileSync.Models;

namespace AdRev.Desktop.MobileSync.Services;

/// <summary>
/// Service de gestion du jumelage entre PC et mobile
/// </summary>
public class PairingService
{
    private readonly Dictionary<string, PairingSession> _activePairings = new();
    private Dictionary<string, PairedDevice> _pairedDevices = new();
    private Dictionary<string, string> _tokenKeys = new(); // Token -> AES Key
    private readonly object _lock = new();
    private readonly string _storagePath;

    public event EventHandler<PairingSession>? PairingCodeGenerated;
    public event EventHandler<PairedDevice>? DevicePaired;
    public event EventHandler<string>? DeviceRevoked;

    public PairingService()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AdRev", "Config");
        Directory.CreateDirectory(folder);
        _storagePath = Path.Combine(folder, "pairing_data.json");
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            if (File.Exists(_storagePath))
            {
                byte[] fileBytes = File.ReadAllBytes(_storagePath);
                string json;

                try
                {
                    // Tenter de déchiffrer (DPAPI)
                    byte[] decryptedBytes = ProtectedData.Unprotect(fileBytes, null, DataProtectionScope.CurrentUser);
                    json = Encoding.UTF8.GetString(decryptedBytes);
                }
                catch
                {
                    // Fallback: Si échec (ancien format non chiffré), on lit le texte brut
                    json = Encoding.UTF8.GetString(fileBytes);
                    // On force une sauvegarde immédiate pour chiffrer
                    Task.Run(SaveData);
                }

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<PairingData>(json);
                if (data != null)
                {
                    _pairedDevices = data.Devices ?? new();
                    _tokenKeys = data.Keys ?? new();
                }
            }
        }
        catch { /* Ignore critical errors preventing startup */ }
    }

    private void SaveData()
    {
        try
        {
            var data = new PairingData
            {
                Devices = _pairedDevices,
                Keys = _tokenKeys
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            
            // Chiffrer avant d'écrire (DPAPI User Scope)
            byte[] plainBytes = Encoding.UTF8.GetBytes(json);
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            
            File.WriteAllBytes(_storagePath, encryptedBytes);
        }
        catch { /* Ignore errors */ }
    }

    private class PairingData
    {
        public Dictionary<string, PairedDevice> Devices { get; set; } = new();
        public Dictionary<string, string> Keys { get; set; } = new();
    }

    /// <summary>
    /// Génère un nouveau code de jumelage
    /// </summary>
    public PairingSession GeneratePairingCode()
    {
        lock (_lock)
        {
            // Générer un code à 6 chiffres
            var code = Random.Shared.Next(100000, 999999).ToString();
            
            var session = new PairingSession
            {
                Code = code,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(5),
                IsUsed = false
            };

            _activePairings[code] = session;
            
            // Nettoyer les codes expirés
            CleanExpiredCodes();
            
            PairingCodeGenerated?.Invoke(this, session);
            
            return session;
        }
    }

    /// <summary>
    /// Valide un code de jumelage et crée un appareil jumelé
    /// </summary>
    public (bool Success, string Token, string EncryptionKey, string Message) ValidatePairingCode(
        string code, 
        string deviceName, 
        string deviceId)
    {
        lock (_lock)
        {
            if (!_activePairings.TryGetValue(code, out var session))
            {
                return (false, string.Empty, string.Empty, "Code de jumelage invalide");
            }

            if (session.IsUsed)
            {
                return (false, string.Empty, string.Empty, "Ce code a déjà été utilisé");
            }

            if (DateTime.Now > session.ExpiresAt)
            {
                _activePairings.Remove(code);
                return (false, string.Empty, string.Empty, "Code de jumelage expiré");
            }

            // Marquer comme utilisé
            session.IsUsed = true;

            // Créer l'appareil jumelé
            var device = new PairedDevice
            {
                DeviceId = deviceId,
                DeviceName = deviceName,
                PairedAt = DateTime.Now,
                LastSyncAt = DateTime.Now,
                IsActive = true
            };

            _pairedDevices[deviceId] = device;

            // Générer un token
            var token = GenerateToken(deviceId);

            // Générer une clé de chiffrement pour cette session
            var key = AdRev.Domain.Utils.SecurityUtils.GenerateKey();
            _tokenKeys[token] = key;
            
            SaveData();

            DevicePaired?.Invoke(this, device);

            return (true, token, key, "Jumelage réussi");
        }
    }

    /// <summary>
    /// Vérifie si un appareil est jumelé
    /// </summary>
    public bool IsDevicePaired(string deviceId)
    {
        lock (_lock)
        {
            return _pairedDevices.TryGetValue(deviceId, out var device) && device.IsActive;
        }
    }

    /// <summary>
    /// Obtient la liste des appareils jumelés
    /// </summary>
    public List<PairedDevice> GetPairedDevices()
    {
        lock (_lock)
        {
            return _pairedDevices.Values.Where(d => d.IsActive).ToList();
        }
    }

    /// <summary>
    /// Révoque un appareil jumelé
    /// </summary>
    public bool RevokeDevice(string deviceId)
    {
        lock (_lock)
        {
            if (_pairedDevices.TryGetValue(deviceId, out var device))
            {
                device.IsActive = false;
                SaveData();
                DeviceRevoked?.Invoke(this, deviceId);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Met à jour la date de dernière synchronisation
    /// </summary>
    public void UpdateLastSync(string deviceId)
    {
        lock (_lock)
        {
            if (_pairedDevices.TryGetValue(deviceId, out var device))
            {
                device.LastSyncAt = DateTime.Now;
                SaveData();
            }
        }
    }

    /// <summary>
    /// Nettoie les codes expirés
    /// </summary>
    private void CleanExpiredCodes()
    {
        var now = DateTime.Now;
        var expiredCodes = _activePairings
            .Where(p => now > p.Value.ExpiresAt)
            .Select(p => p.Key)
            .ToList();

        foreach (var code in expiredCodes)
        {
            _activePairings.Remove(code);
        }
    }

    /// <summary>
    /// Génère un token simple pour l'authentification
    /// </summary>
    private string GenerateToken(string deviceId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = $"{deviceId}:{timestamp}";
        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(data)));
        return $"{deviceId}:{timestamp}:{hash}";
    }

    /// <summary>
    /// Valide un token
    /// </summary>
    public bool ValidateToken(string token)
    {
        try
        {
            var parts = token.Split(':');
            if (parts.Length != 3) return false;

            var deviceId = parts[0];
            var timestamp = long.Parse(parts[1]);
            var hash = parts[2];

            // Vérifier que l'appareil est jumelé
            if (!IsDevicePaired(deviceId)) return false;

            // Vérifier le hash
            var data = $"{deviceId}:{timestamp}";
            var expectedHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(data)));

            return hash == expectedHash;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extrait le deviceId d'un token
    /// </summary>
    public string? GetDeviceIdFromToken(string token)
    {
        try
        {
            var parts = token.Split(':');
            return parts.Length >= 1 ? parts[0] : null;
        }
        catch
        {
            return null;
        }
    }

    public string? GetEncryptionKey(string token)
    {
        lock (_lock)
        {
            return _tokenKeys.TryGetValue(token, out var key) ? key : null;
        }
    }
}
