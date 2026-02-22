$content = Get-Content "MainWindow.xaml" -Raw

$replacements = @(
    @('Text="Accueil"', 'Text="{DynamicResource BtnHome}"'),
    @('Text="Nouveau Projet"', 'Text="{DynamicResource BtnNewProject}"'),
    @('Text="Équipe"', 'Text="{DynamicResource BtnTeam}"'),
    @('Text="Partager"', 'Text="{DynamicResource BtnShare}"'),
    @('Text="Documentation"', 'Text="{DynamicResource BtnDoc}"'),
    @('Text="Support"', 'Text="{DynamicResource BtnSupport}"'),
    @('Text="À propos"', 'Text="{DynamicResource BtnAbout}"'),
    @('Text="Mon Profil"', 'Text="{DynamicResource BtnProfile}"'),
    @('Content="Fichier"', 'Content="{DynamicResource TabFile}"'),
    @('Content="Aide"', 'Content="{DynamicResource TabHelp}"'),
    @('Text="Tableau de Bord Scientifique"', 'Text="{DynamicResource DashboardTitle}"'),
    @('Text="Vue d''ensemble de vos recherches et analyses"', 'Text="{DynamicResource DashboardSubtitle}"'),
    @('Text="Projets En Cours"', 'Text="{DynamicResource CardOngoingTitle}"'),
    @('Text="Actifs"', 'Text="{DynamicResource CardOngoingStatus}"'),
    @('Text="Protocoles Validés"', 'Text="{DynamicResource CardValidatedTitle}"'),
    @('Text="Terminés"', 'Text="{DynamicResource CardValidatedStatus}"'),
    @('Text="Analyse Rapide"', 'Text="{DynamicResource CardAnalysisTitle}"'),
    @('Text="Importer CSV/Excel"', 'Text="{DynamicResource CardAnalysisSubtitle}"'),
    @('Text="Importer Dossier"', 'Text="{DynamicResource CardImportTitle}"'),
    @('Text="Charger depuis PC"', 'Text="{DynamicResource CardImportSubtitle}"'),
    @('Text="Licence AdRev 🔑"', 'Text="{DynamicResource LicenseTitle}"'),
    @('Text="Chargement..."', 'Text="{DynamicResource LicenseLoading}"'),
    @('Content="🚀 UPGRADE"', 'Content="{DynamicResource BtnUpgrade}"'),
    @('Text="AdRev Connect 🌐"', 'Text="{DynamicResource ConnectTitle}"'),
    @('Text="Projets partagés"', 'Text="{DynamicResource ConnectSubtitle}"'),
    @('Text="Serveur / IP :"', 'Text="{DynamicResource LabelServerIP}"'),
    @('Content="Connexion Réseau"', 'Content="{DynamicResource BtnConnectNetwork}"'),
    @('Text="Connecté"', 'Text="{DynamicResource LabelConnected}"'),
    @('Content="Déconnecter"', 'Content="{DynamicResource BtnDisconnect}"'),
    @('Text="VOS PROJETS RÉCENTS"', 'Text="{DynamicResource SectionRecentProjects}"'),
    @('Text="PROJETS D''ÉQUIPE"', 'Text="{DynamicResource SectionTeamProjects}"'),
    @('Text="PROJETS PERSONNELS"', 'Text="{DynamicResource SectionIndividualProjects}"'),
    @('Text="ARCHIVES / TERMINÉS"', 'Text="{DynamicResource SectionArchiveProjects}"'),
    @('Text="ÉQUIPE"', 'Text="{DynamicResource TagTeam}"'),
    @('Text="PERSONNEL"', 'Text="{DynamicResource TagPersonal}"'),
    @('Text="Initialisation du Nouveau Projet"', 'Text="{DynamicResource NewProjectTitle}"'),
    @('Text="Titre du projet"', 'Text="{DynamicResource LabelProjectTitle}"'),
    @('Tag="Entrez le titre de votre projet de recherche ici..."', 'Tag="{DynamicResource PlaceholderProjectTitle}"'),
    @('Text="Institution / Organisme"', 'Text="{DynamicResource LabelInstitution}"'),
    @('Tag="Ex: Université de Paris, Hôpital Central..."', 'Tag="{DynamicResource PlaceholderInstitution}"'),
    @('Text="Domaine Scientifique"', 'Text="{DynamicResource LabelDomain}"'),
    @('Text="Type d''étude"', 'Text="{DynamicResource LabelStudyType}"'),
    @('Text="Auteurs (séparés par des virgules)"', 'Text="{DynamicResource LabelAuthors}"'),
    @('Tag="Listez les auteurs (Nom Prénom), séparés par des virgules..."', 'Tag="{DynamicResource PlaceholderAuthors}"'),
    @('Text="Emplacement du Projet (Local)"', 'Text="{DynamicResource LabelLocation}"'),
    @('Content="Valider et Créer le Projet"', 'Content="{DynamicResource BtnValidateCreate}"'),
    @('Text="Prêt"', 'Text="{DynamicResource StatusReady}"'),
    @('Text="AdRev Science Suite v1.0.0 (First Edition) | © 2024-2026 AdRev Team"', 'Text="{DynamicResource StatusCopyright}"')
)

foreach ($pair in $replacements) {
    if ($content.Contains($pair[0])) {
        Write-Host "Replacing $($pair[0])"
        $content = $content.Replace($pair[0], $pair[1])
    }
}

Set-Content "MainWindow.xaml" $content -Encoding UTF8
