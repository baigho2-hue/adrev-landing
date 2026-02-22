namespace AdRev.Domain.Models
{
    public enum UserAccessLevel
    {
        [System.ComponentModel.Description("Lecteur")]
        Viewer,
        [System.ComponentModel.Description("Éditeur")]
        Editor,
        [System.ComponentModel.Description("Chef de Projet")]
        Admin
    }

    public enum FunctionalRole
    {
        [System.ComponentModel.Description("Investigateur Principal")]
        PrincipalInvestigator,
        [System.ComponentModel.Description("Méthodologiste")]
        Methodologist,
        [System.ComponentModel.Description("Statisticien")]
        Statistician,
        [System.ComponentModel.Description("Data Manager")]
        DataManager,
        [System.ComponentModel.Description("Co-Investigateur")]
        CoInvestigator,
        [System.ComponentModel.Description("Moniteur / ARC")]
        Monitor,
        [System.ComponentModel.Description("Étudiant / Stagiaire")]
        Student
    }

    public class Author
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // Prof, Dr, Mr, Mme...
        public string Institution { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        public UserAccessLevel AccessLevel { get; set; } = UserAccessLevel.Editor;
        public FunctionalRole Role { get; set; } = FunctionalRole.PrincipalInvestigator;

        public override string ToString()
        {
            return $"{Title} {FirstName} {LastName} ({Institution}) - {Role} ({AccessLevel})";
        }
    }
}
