namespace AdRev.Domain.MobileSync.Models;

/// <summary>
/// Modèle pour la demande de jumelage
/// </summary>
public class PairingRequest
{
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>
/// Réponse de jumelage avec le token
/// </summary>
public class PairingResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string EncryptionKey { get; set; } = string.Empty; // Base64 AES Key
    public string Message { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Modèle pour valider un code de jumelage
/// </summary>
public class ValidatePairingRequest
{
    public string PairingCode { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>
/// Information sur un appareil jumelé
/// </summary>
public class PairedDevice
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTime PairedAt { get; set; }
    public DateTime LastSyncAt { get; set; }
    public bool IsActive { get; set; }
}


/// <summary>
/// Session de jumelage
/// </summary>
public class PairingSession
{
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}

/// <summary>
/// Un formulaire/masque de saisie à remplir sur mobile
/// </summary>
public class MobileQuestionnaire
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Nous utilisons une liste simplifiée ou la référence complète si possible
    // Pour la sérialisation, il vaut mieux s'assurer que StudyVariable est sérialisable
    public List<AdRev.Domain.Variables.StudyVariable> Variables { get; set; } = new();
}

/// <summary>
/// Un enregistrement de données collectées
/// </summary>
public class CollectedDataRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string QuestionnaireId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTime CollectedAt { get; set; } = DateTime.Now;
    
    // Stockage des réponses sous forme JSON ou Dictionnaire sérialisé
    // Clé = Nom de la variable, Valeur = Réponse convertie en string
    public string AnswersJson { get; set; } = "{}";
    
    public bool IsSynced { get; set; }
}

public class SecurePayload
{
    public string Content { get; set; } = string.Empty; // Encrypted Base64
}

/// <summary>
/// Package complet pour le partage P2P entre mobiles
/// </summary>
public class ProjectSharePackage
{
    public string PackageVersion { get; set; } = "1.0";
    public DateTime ExportedAt { get; set; } = DateTime.Now;
    public string ExportedByDevice { get; set; } = string.Empty;
    
    // Les protocoles/questionnaires nécessaires
    public List<MobileQuestionnaire> Questionnaires { get; set; } = new();
    
    // Les données collectées associées
    public List<CollectedDataRecord> Data { get; set; } = new();
}
