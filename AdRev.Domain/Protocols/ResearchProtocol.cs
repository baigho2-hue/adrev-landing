using AdRev.Domain.Enums;
using AdRev.Domain.Models;
using System;
using System.Collections.Generic;

namespace AdRev.Domain.Protocols;

public class ResearchProtocol
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    
    // Auteurs structurés
    public Author PrincipalAuthor { get; set; } = new Author();
    public List<Author> CoAuthors { get; set; } = new List<Author>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Introduction Split
    public string Context { get; set; } = string.Empty;
    public string ProblemJustification { get; set; } = string.Empty;

    public string ResearchQuestion { get; set; } = string.Empty; // Max 3
    public string Hypotheses { get; set; } = string.Empty;       // Max 2
    
    public string GeneralObjective { get; set; } = string.Empty;
    public string SpecificObjectives { get; set; } = string.Empty;
    public string ConceptDefinitions { get; set; } = string.Empty; // Définition des concepts

    public StudyType StudyType { get; set; }
    public ScientificDomain Domain { get; set; } = ScientificDomain.Biomedical;
    public QualitativeApproach QualitativeApproach { get; set; }
    public EpidemiologicalStudyType EpidemiologyType { get; set; }

    // Methodology Split
    public string StudySetting { get; set; } = string.Empty;       // Cadre de l'étude
    public string ConceptualModel { get; set; } = string.Empty;    // Modèle Conceptuel
    
    // Caractéristiques de l'étude
    public bool IsMulticentric { get; set; } = false;              // Étude multicentrique
    public string StudyCenters { get; set; } = string.Empty;       // Liste des centres participants
    
    public string StudyPopulation { get; set; } = string.Empty;    // Population
    public string InclusionCriteria { get; set; } = string.Empty;
    public string ExclusionCriteria { get; set; } = string.Empty;
    
    // Échantillonnage avancé
    public SamplingType SamplingType { get; set; } = SamplingType.None;
    public bool IsStratified { get; set; } = false;                // Échantillonnage stratifié
    public string StratificationCriteria { get; set; } = string.Empty; // Critères de stratification
    public bool IsClusterSampling { get; set; } = false;           // Échantillonnage en grappe
    public int ClusterSize { get; set; } = 0;                      // Taille moyenne des grappes
    public double DesignEffect { get; set; } = 1.0;                // Effet de plan (Deff)
    public double ExpectedLossRate { get; set; } = 0.0;            // Taux de perdus de vue attendu (%)
    
    public string SamplingMethod { get; set; } = string.Empty;     // Texte descriptif final de l'échantillonnage
    public string DataCollection { get; set; } = string.Empty;     // Collecte (outils/variables)
    public string DataAnalysis { get; set; } = string.Empty;       // Plan d'analyse

    public string Ethics { get; set; } = string.Empty;
    public string Budget { get; set; } = string.Empty; // Budget prévisionnel
    public string ExpectedResults { get; set; } = string.Empty;
    
    // Discussion et Limites
    public string DiscussionPlan { get; set; } = string.Empty;
    public string StudyLimitations { get; set; } = string.Empty;

    public string Conclusion { get; set; } = string.Empty;
    public string References { get; set; } = string.Empty;

    // Dictionnaire des Variables (Style Epi Info - Masque de Saisie)
    public List<AdRev.Domain.Variables.StudyVariable> Variables { get; set; } = new List<AdRev.Domain.Variables.StudyVariable>();

    // Gestion Bibliographique
    public ReferenceStyle ReferenceStyle { get; set; } = ReferenceStyle.Vancouver;
    public List<Citation> Citations { get; set; } = new List<Citation>();
    
    // Ressources (Tableaux / Figures)
    public List<ProtocolResource> Resources { get; set; } = new List<ProtocolResource>();

    // Annexes (Questionnaire, Guide d'entretien, Consentement...)
    public List<ProtocolAppendix> Appendices { get; set; } = new List<ProtocolAppendix>();
}