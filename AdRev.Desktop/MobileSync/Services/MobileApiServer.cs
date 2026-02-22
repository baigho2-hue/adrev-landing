using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using AdRev.Domain.MobileSync.Models;

namespace AdRev.Desktop.MobileSync.Services;

/// <summary>
/// Serveur HTTP simple pour l'API mobile
/// </summary>
public class MobileApiServer
{
    private HttpListener? _listener;
    private readonly PairingService _pairingService;
    private readonly AdRev.Core.Protocols.ProtocolService _protocolService;
    private readonly AdRev.Core.Common.ResearchProjectService _projectService;
    private bool _isRunning;
    private Task? _listenerTask;

    public event EventHandler<string>? LogMessage;
    public bool IsRunning => _isRunning;
    public int Port { get; private set; } = 5000;

    public MobileApiServer(PairingService pairingService, 
                           AdRev.Core.Protocols.ProtocolService protocolService,
                           AdRev.Core.Common.ResearchProjectService projectService)
    {
        _pairingService = pairingService;
        _protocolService = protocolService;
        _projectService = projectService;
    }

    /// <summary>
    /// Démarre le serveur HTTP
    /// </summary>
    public async Task StartAsync(int port = 5000)
    {
        if (_isRunning) return;

        Port = port;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        
        try
        {
            _listener.Start();
            _isRunning = true;
            Log($"Serveur API démarré sur le port {port}");

            _listenerTask = Task.Run(async () => await ListenAsync());
        }
        catch (Exception ex)
        {
            Log($"Erreur au démarrage du serveur: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Arrête le serveur HTTP
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _listener?.Stop();
        _listener?.Close();
        Log("Serveur API arrêté");
    }

    /// <summary>
    /// Boucle d'écoute des requêtes
    /// </summary>
    private async Task ListenAsync()
    {
        while (_isRunning && _listener != null)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
            catch (HttpListenerException)
            {
                // Listener arrêté
                break;
            }
            catch (Exception ex)
            {
                Log($"Erreur lors de l'écoute: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gère une requête HTTP
    /// </summary>
    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            // CORS headers
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            var path = request.Url?.AbsolutePath ?? "/";
            Log($"{request.HttpMethod} {path}");

            // Router
            if (path == "/api/pairing/ping" && request.HttpMethod == "GET")
            {
                await HandlePingAsync(response);
            }
            else if (path == "/api/pairing/validate" && request.HttpMethod == "POST")
            {
                await HandleValidatePairingAsync(request, response);
            }
            else if (path == "/api/pairing/devices" && request.HttpMethod == "GET")
            {
                await HandleGetDevicesAsync(response);
            }
            else if (path == "/api/sync/questionnaires" && request.HttpMethod == "GET")
            {
                await HandleGetQuestionnairesAsync(response);
            }
            else if (path == "/api/sync/data" && request.HttpMethod == "POST")
            {
                await HandleSyncDataAsync(request, response);
            }
            else
            {
                response.StatusCode = 404;
                await SendJsonAsync(response, new { error = "Not found" });
            }
        }
        catch (Exception ex)
        {
            Log($"Erreur lors du traitement de la requête: {ex.Message}");
            context.Response.StatusCode = 500;
            await SendJsonAsync(context.Response, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Endpoint ping
    /// </summary>
    private async Task HandlePingAsync(HttpListenerResponse response)
    {
        response.StatusCode = 200;
        await SendJsonAsync(response, new
        {
            status = "ok",
            serverTime = DateTime.Now,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Endpoint de validation de jumelage
    /// </summary>
    private async Task HandleValidatePairingAsync(HttpListenerRequest request, HttpListenerResponse response)
    {
        try
        {
            var body = await ReadRequestBodyAsync(request);
            var pairingRequest = JsonConvert.DeserializeObject<ValidatePairingRequest>(body);

            if (pairingRequest == null || string.IsNullOrWhiteSpace(pairingRequest.PairingCode))
            {
                response.StatusCode = 400;
                await SendJsonAsync(response, new PairingResponse
                {
                    Success = false,
                    Message = "Données invalides"
                });
                return;
            }

            var (success, token, key, message) = _pairingService.ValidatePairingCode(
                pairingRequest.PairingCode,
                pairingRequest.DeviceName,
                pairingRequest.DeviceId
            );

            response.StatusCode = 200;
            await SendJsonAsync(response, new PairingResponse
            {
                Success = success,
                Token = token,
                EncryptionKey = key,
                Message = message,
                ExpiresAt = DateTime.Now.AddDays(30)
            });
        }
        catch (Exception ex)
        {
            Log($"Erreur validation jumelage: {ex.Message}");
            response.StatusCode = 500;
            await SendJsonAsync(response, new PairingResponse
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// Endpoint pour obtenir la liste des appareils
    /// </summary>
    private async Task HandleGetDevicesAsync(HttpListenerResponse response)
    {
        var devices = _pairingService.GetPairedDevices();
        response.StatusCode = 200;
        await SendJsonAsync(response, devices);
    }

    /// <summary>
    /// Endpoint pour récupérer les questionnaires
    /// </summary>
    private async Task HandleGetQuestionnairesAsync(HttpListenerResponse response)
    {
        var questionnaires = new List<MobileQuestionnaire>();
        
        try
        {
            var protocols = _protocolService.GetAllProtocols();
            foreach (var protocol in protocols)
            {
                // Only share protocols with defined variables
                if (protocol.Variables != null && protocol.Variables.Any())
                {
                    questionnaires.Add(new MobileQuestionnaire
                    {
                        Id = protocol.Id,
                        Title = protocol.Title,
                        Description = string.IsNullOrWhiteSpace(protocol.GeneralObjective) ? protocol.StudyType.ToString() : protocol.GeneralObjective,
                        CreatedAt = protocol.CreatedAt,
                        Variables = protocol.Variables
                    });
                }
            }
            
            Log($"Envoyé {questionnaires.Count} questionnaires au mobile");
        }
        catch (Exception ex)
        {
            Log($"Erreur lors de la récupération des protocoles: {ex.Message}");
        }

        response.StatusCode = 200;
        await SendJsonAsync(response, questionnaires);
    }

    /// <summary>
    /// Endpoint pour recevoir les données
    /// </summary>
    private async Task HandleSyncDataAsync(HttpListenerRequest request, HttpListenerResponse response)
    {
        try
        {
            // Récupérer le token pour trouver la clé
            var authHeader = request.Headers["Authorization"];
            var token = authHeader?.StartsWith("Bearer ") == true ? authHeader.Substring("Bearer ".Length) : "";
            var key = _pairingService.GetEncryptionKey(token);

            var body = await ReadRequestBodyAsync(request);
            List<CollectedDataRecord>? records = null;

            if (!string.IsNullOrEmpty(key))
            {
                // Mode Sécurisé
                try 
                {
                    var securePayload = JsonConvert.DeserializeObject<SecurePayload>(body);
                    if (securePayload != null && !string.IsNullOrEmpty(securePayload.Content))
                    {
                        var json = AdRev.Domain.Utils.SecurityUtils.Decrypt(securePayload.Content, key);
                        records = JsonConvert.DeserializeObject<List<CollectedDataRecord>>(json);
                    }
                }
                catch (Exception ex)
                {
                    Log($"Erreur déchiffrement: {ex.Message}");
                    response.StatusCode = 400;
                    await SendJsonAsync(response, new { success = false, error = "Decryption failed" });
                    return;
                }
            }
            else
            {
                // Mode non-sécurisé (Fallback ou Legacy)
                // Pour l'instant on accepte encore le JSON clair si pas de clé trouvée (ou si session perdue)
                // Idéalement, on devrait rejeter.
                // Log("Attention: Réception de données non chiffrées ou clé introuvable.");
                records = JsonConvert.DeserializeObject<List<CollectedDataRecord>>(body);
            }

            if (records != null && records.Count > 0)
            {
                // 1. Sauvegarder dans un dossier local (Backup)
                var syncDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AdRev", "SyncedData");
                Directory.CreateDirectory(syncDir);

                foreach (var record in records)
                {
                    var filename = $"Record_{record.DeviceId}_{record.Id}.json";
                    File.WriteAllText(Path.Combine(syncDir, filename), JsonConvert.SerializeObject(record, Formatting.Indented));
                }
                
                // 2. Intégrer dans les Projets
                var projects = _projectService.GetAllProjects();
                int integratedCount = 0;
                
                // Grouper par QuestionnaireId (ProtocolId)
                var groupedRecords = records.GroupBy(r => r.QuestionnaireId);
                
                foreach (var group in groupedRecords)
                {
                    var protocolId = group.Key;
                    // Trouver le projet lié à ce protocole
                    var project = projects.FirstOrDefault(p => p.SourceProtocolId == protocolId);
                    
                    if (project != null)
                    {
                        bool projectUpdated = false;
                        foreach (var record in group)
                        {
                            try 
                            {
                                var dataRow = JsonConvert.DeserializeObject<Dictionary<string, object>>(record.AnswersJson);
                                if (dataRow != null)
                                {
                                    // Ajouter métadonnées
                                    dataRow["_MobileDeviceId"] = record.DeviceId;
                                    dataRow["_MobileCollectionDate"] = record.CollectedAt;
                                    
                                    project.DataRows.Add(dataRow);
                                    projectUpdated = true;
                                    integratedCount++;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log($"Erreur parsing record {record.Id}: {ex.Message}");
                            }
                        }
                        
                        if (projectUpdated)
                        {
                            _projectService.SaveProject(project);
                            Log($"Projet '{project.Title}' mis à jour avec {group.Count()} nouvelles entrées.");
                        }
                    }
                    else
                    {
                        Log($"Aucun projet trouvé pour le protocole {protocolId}. Données sauvegardées uniquement en backup JSON.");
                    }
                }
                
                Log($"Reçu {records.Count} enregistrements. {integratedCount} intégrés dans les projets.");
            }

            response.StatusCode = 200;
            await SendJsonAsync(response, new { success = true });
        }
        catch (Exception ex)
        {
            Log($"Erreur sync data: {ex.Message}");
            response.StatusCode = 500;
            await SendJsonAsync(response, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Lit le corps de la requête
    /// </summary>
    private async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Envoie une réponse JSON
    /// </summary>
    private async Task SendJsonAsync(HttpListenerResponse response, object data)
    {
        response.ContentType = "application/json";
        var json = JsonConvert.SerializeObject(data);
        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }

    /// <summary>
    /// Log un message
    /// </summary>
    private void Log(string message)
    {
        LogMessage?.Invoke(this, $"[API] {message}");
    }
}
