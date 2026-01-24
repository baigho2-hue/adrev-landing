using AdRev.Domain.Enums;
using System.Collections.Generic;

namespace AdRev.Domain.Variables
{
    /// <summary>
    /// Représente une variable d'étude (une question dans le masque de saisie)
    /// Inspiré de la structure de métadonnées d'Epi Info 7
    /// </summary>
    public class StudyVariable
    {
        /// <summary>
        /// Nom de la variable dans la base de données (ex: "DATENAISS")
        /// Doit être court, sans espace, unique.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Le type de données (Texte, Nombre, Date...)
        /// </summary>
        public VariableType Type { get; set; }

        /// <summary>
        /// Indique si la réponse est obligatoire
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Pour les types numériques : valeur minimale acceptée
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// Pour les types numériques : valeur maximale acceptée
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// Pour les types Text : longueur maximale
        /// </summary>
        public int MaxLength { get; set; } = 255;

        /// <summary>
        /// Pour les listes de choix : les options disponibles (séparées par des virgules ou stockées ailleurs)
        /// Ex: "Masculin,Féminin"
        /// </summary>
        public string ChoiceOptions { get; set; } = string.Empty;

        /// <summary>
        /// Groupe ou Section auquel appartient la variable (pour organiser la page)
        /// Ex: "Données Sociodémographiques", "Données Cliniques"
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Pour les études qualitatives déductives : Thème majeur de codification
        /// </summary>
        public string Theme { get; set; } = string.Empty;
        public string SubTheme { get; set; } = string.Empty;

        /// <summary>
        /// Indique si cette variable est un attribut de cas (pour le qualitatif).
        /// </summary>
        public bool IsQualitativeCaseAttribute { get; set; }

        /// <summary>
        /// Nom de la feuille de classification (ex: "Sociodémographique") pour le qualitatif.
        /// </summary>
        public string ClassificationSheet { get; set; } = string.Empty;

        /// <summary>
        /// Ordre d'affichage dans le masque
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Note ou instruction pour l'enquêteur
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Logique de saut (Skip Pattern)
        /// Ex: "Si NON, aller à Q10" ou condition formelle
        /// </summary>
        public string SkipLogic { get; set; } = string.Empty;

        /// <summary>
        /// Formule pour les champs calculés
        /// Ex: "[POIDS] / ([TAILLE] * [TAILLE])"
        /// </summary>
        public string CalculationFormula { get; set; } = string.Empty;

        /// <summary>
        /// Condition pour que cette question soit posée
        /// Ex: "[SEXE] = 'Femme'"
        /// </summary>
        public string VisibilityCondition { get; set; } = string.Empty;

        /// <summary>
        /// Coordonnées par défaut pour les variables de géolocalisation
        /// </summary>
        public double? DefaultLatitude { get; set; }
        public double? DefaultLongitude { get; set; }

        public StudyVariable()
        {
            Type = VariableType.Text;
            IsRequired = false;
        }

        public override string ToString()
        {
            return $"{Name} ({Type}) - {Prompt}";
        }
    }
}
