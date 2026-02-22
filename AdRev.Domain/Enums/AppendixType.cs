using System.ComponentModel;

namespace AdRev.Domain.Enums;

public enum AppendixType
{
    [Description("Questionnaire")]
    Questionnaire,
    [Description("Guide d'entretien")]
    InterviewGuide,
    [Description("Consentement Éclairé")]
    InformedConsent,
    [Description("Formulaire d'Ascentement")]
    InformedAssent,
    [Description("Grille d'observation")]
    ObservationGrid,
    [Description("Fiche de collecte de données")]
    DataCollectionForm,
    [Description("Approbation Éthique")]
    EthicalApproval,
    [Description("Notice d'information")]
    InformationNotice,
    [Description("Engagement de confidentialité")]
    ConfidentialityAgreement,
    [Description("Stratégie de recherche (Mots-clés)")]
    SearchStrategy,
    [Description("Données Budgétaires")]
    BudgetData,
    [Description("Curriculum Vitae")]
    CurriculumVitae,
    [Description("Autre")]
    Other
}
