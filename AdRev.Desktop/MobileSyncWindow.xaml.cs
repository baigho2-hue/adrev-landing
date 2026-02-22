using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using QRCoder;
using AdRev.Domain.MobileSync.Models;
using AdRev.Desktop.MobileSync.Services;

namespace AdRev.Desktop;

public partial class MobileSyncWindow : Window
{
    private readonly PairingService _pairingService;
    private readonly MobileApiServer _apiServer;
    private readonly AdRev.Core.Protocols.ProtocolService _protocolService;
    private readonly AdRev.Core.Common.ResearchProjectService _projectService;
    private DispatcherTimer? _expiryTimer;
    private PairingSession? _currentSession;

    public MobileSyncWindow()
    {
        InitializeComponent();
        
        _pairingService = new PairingService();
        _protocolService = new AdRev.Core.Protocols.ProtocolService();
        _projectService = new AdRev.Core.Common.ResearchProjectService();
        
        _apiServer = new MobileApiServer(_pairingService, _protocolService, _projectService);

        // Subscribe to events
        _pairingService.PairingCodeGenerated += OnPairingCodeGenerated;
        _pairingService.DevicePaired += OnDevicePaired;
        _pairingService.DeviceRevoked += OnDeviceRevoked;
        _apiServer.LogMessage += OnLogMessage;

        // Initialize timer for expiry countdown
        _expiryTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _expiryTimer.Tick += ExpiryTimer_Tick;

        Loaded += MobileSyncWindow_Loaded;
    }

    private void MobileSyncWindow_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshDevicesList();
        AddLog("Fenêtre de synchronisation mobile chargée");
    }

    private async void StartServerButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_apiServer.IsRunning)
            {
                _apiServer.Stop();
                StartServerButton.Content = "Démarrer le serveur";
                ServerStatusIndicator.Fill = Brushes.Red;
                ServerStatusText.Text = "Serveur arrêté";
                GenerateCodeButton.IsEnabled = false;
            }
            else
            {
                await _apiServer.StartAsync(5000);
                StartServerButton.Content = "Arrêter le serveur";
                ServerStatusIndicator.Fill = Brushes.Green;
                ServerStatusText.Text = "Serveur actif";
                GenerateCodeButton.IsEnabled = true;
                
                // Get local IP
                var localIP = GetLocalIPAddress();
                ServerUrlText.Text = $"http://{localIP}:5000";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            AddLog($"Erreur serveur: {ex.Message}");
        }
    }

    private void GenerateCodeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _currentSession = _pairingService.GeneratePairingCode();
            PairingCodeText.Text = _currentSession.Code;
            
            // Generate QR Code
            GenerateQRCode(_currentSession.Code);
            
            // Start expiry timer
            _expiryTimer?.Start();
            
            AddLog($"Code de jumelage généré: {_currentSession.Code}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            AddLog($"Erreur génération code: {ex.Message}");
        }
    }

    private void GenerateQRCode(string code)
    {
        try
        {
            var localIP = GetLocalIPAddress();
            var qrData = $"{{\"code\":\"{code}\",\"server\":\"http://{localIP}:5000\"}}";
            
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var qrBitmap = qrCode.GetGraphic(20);
            
            QRCodeImage.Source = ConvertBitmapToImageSource(qrBitmap);
        }
        catch (Exception ex)
        {
            AddLog($"Erreur génération QR code: {ex.Message}");
        }
    }

    private BitmapImage ConvertBitmapToImageSource(System.Drawing.Bitmap bitmap)
    {
        using var memory = new System.IO.MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        memory.Position = 0;
        
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        
        return bitmapImage;
    }

    private void ExpiryTimer_Tick(object? sender, EventArgs e)
    {
        if (_currentSession == null) return;

        var remaining = _currentSession.ExpiresAt - DateTime.Now;
        if (remaining.TotalSeconds <= 0)
        {
            PairingExpiryText.Text = "Code expiré";
            PairingCodeText.Text = "------";
            QRCodeImage.Source = null;
            _expiryTimer?.Stop();
            _currentSession = null;
        }
        else
        {
            var minutes = (int)remaining.TotalMinutes;
            var seconds = remaining.Seconds;
            PairingExpiryText.Text = $"Expire dans {minutes:D2}:{seconds:D2}";
        }
    }

    private void OnPairingCodeGenerated(object? sender, PairingSession session)
    {
        Dispatcher.Invoke(() =>
        {
            AddLog($"Nouveau code généré: {session.Code} (expire à {session.ExpiresAt:HH:mm:ss})");
        });
    }

    private void OnDevicePaired(object? sender, PairedDevice device)
    {
        Dispatcher.Invoke(() =>
        {
            AddLog($"Appareil jumelé: {device.DeviceName} ({device.DeviceId})");
            RefreshDevicesList();
            
            // Clear pairing code
            PairingCodeText.Text = "------";
            PairingExpiryText.Text = "Jumelage réussi !";
            QRCodeImage.Source = null;
            _expiryTimer?.Stop();
            
            MessageBox.Show($"Appareil '{device.DeviceName}' jumelé avec succès!", 
                "Jumelage réussi", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    private void OnDeviceRevoked(object? sender, string deviceId)
    {
        Dispatcher.Invoke(() =>
        {
            AddLog($"Appareil révoqué: {deviceId}");
            RefreshDevicesList();
        });
    }

    private void OnLogMessage(object? sender, string message)
    {
        Dispatcher.Invoke(() => AddLog(message));
    }

    private void RefreshDevicesList()
    {
        var devices = _pairingService.GetPairedDevices();
        DevicesListView.ItemsSource = devices;
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        LogsTextBox.AppendText($"[{timestamp}] {message}\n");
        LogsTextBox.ScrollToEnd();
    }

    private string GetLocalIPAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch
        {
            // Fallback
        }
        return "localhost";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _apiServer.Stop();
        _expiryTimer?.Stop();
        base.OnClosed(e);
    }
}
