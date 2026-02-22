$path = "MainWindow.xaml"
$content = Get-Content $path -Raw

$replacements = @(
    @('Text="Centre d''Aide &amp; Ressources"', 'Text="{DynamicResource HelpTitle}"'),
    @('Text="Tout ce dont vous avez besoin pour maîtriser AdRev."', 'Text="{DynamicResource HelpSubtitle}"'),
    @('Text="Guides utilisateurs complets."', 'Text="{DynamicResource CardDocDesc}"'),
    @('Text="Tutoriels Vidéo"', 'Text="{DynamicResource CardTutorialTitle}"'),
    @('Text="Apprenez en regardant."', 'Text="{DynamicResource CardTutorialDesc}"'),
    @('Text="Support Technique"', 'Text="{DynamicResource CardSupportTitle}"'),
    @('Text="Une question ? Contactez-nous."', 'Text="{DynamicResource CardSupportDesc}"'),
    @('Text="Abonnement"', 'Text="{DynamicResource CardSubscriptionTitle}"'),
    @('Text="Gérez votre licence Pro / USB."', 'Text="{DynamicResource CardSubscriptionDesc}"'),
    @('Header="Actions"', 'Header="{DynamicResource ColActions}"'),
    @('ToolTip="Marquer comme Accepté"', 'ToolTip="{DynamicResource TooltipMarkAccepted}"'),
    @('ToolTip="Marquer comme Publié"', 'ToolTip="{DynamicResource TooltipMarkPublished}"'),
    @('ToolTip="Ouvrir le Projet"', 'ToolTip="{DynamicResource TooltipOpenProject}"')
)

foreach ($pair in $replacements) {
    if ($content.Contains($pair[0])) {
        Write-Host "Replacing $($pair[0])"
        $content = $content.Replace($pair[0], $pair[1])
    }
    else {
        Write-Warning "Target string not found: $($pair[0])"
    }
}

Set-Content $path $content -Encoding UTF8
