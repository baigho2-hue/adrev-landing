using System.ComponentModel;

namespace AdRev.Domain.Enums
{
    /// <summary>
    /// Types de variables disponibles pour la création de masques de saisie (style Epi Info)
    /// </summary>
    public enum VariableType
    {
        [Description("Texte Court (ex: Nom, Ville)")]
        Text,

        [Description("Texte Long (ex: Commentaires)")]
        Memo,

        [Description("Quantitative Discrète (Nombre Entier)")]
        QuantitativeDiscrete, // Was NumberInteger

        [Description("Quantitative Continue (Nombre Décimal)")]
        QuantitativeContinuous, // Was NumberDecimal

        [Description("Quantitative Temporelle (Date)")]
        QuantitativeTemporal, // Was Date

        [Description("Quantitative Temporelle (Heure)")]
        Time,

        [Description("Qualitative Binaire (Oui/Non)")]
        QualitativeBinary, // Was YesNo

        [Description("Qualitative Nominale (Choix Unique)")]
        QualitativeNominal, // Was SingleChoice

        [Description("Qualitative Ordinale (Choix Ordonné)")]
        QualitativeOrdinal,

        [Description("Qualitative Catégorielle (Choix Multiples)")]
        MultipleChoice,

        [Description("Calculé / Formule")]
        Calculated,

        [Description("Label / Titre de section")]
        LabelOnly,
        
        [Description("Géolocalisation (Point unique, GPS)")]
        GeolocationPoint,
        
        [Description("Géolocalisation (Zone, Polygone)")]
        GeolocationZone
    }
}
