$content = Get-Content "Windows\ProjectWindow.xaml" -Raw

$replacements = @(
    @('Text="PROTOCOLE"', 'Text="{DynamicResource StepProtocol}"'),
    @('Text="VARIABLES"', 'Text="{DynamicResource StepVariables}"'),
    @('Text="COLLECTE"', 'Text="{DynamicResource StepData}"'),
    @('Text="ANALYSE"', 'Text="{DynamicResource StepAnalysis}"'),
    @('Text="PUBLICATION"', 'Text="{DynamicResource StepReport}"'),
    @('Content="Protocole"', 'Content="{DynamicResource RibbonProtocol}"'),
    @('Content="Variables"', 'Content="{DynamicResource RibbonVariables}"'),
    @('Content="Saisie"', 'Content="{DynamicResource RibbonEntry}"'),
    @('Content="Stats"', 'Content="{DynamicResource RibbonStats}"'),
    @('Content="Quali Dashboard"', 'Content="{DynamicResource RibbonQualiDash}"'),
    @('Content="Atelier Codage"', 'Content="{DynamicResource RibbonCoding}"'),
    @('Content="Discussion"', 'Content="{DynamicResource RibbonDiscussion}"'),
    @('Content="Rapport"', 'Content="{DynamicResource RibbonReport}"'),
    @('Content="Article"', 'Content="{DynamicResource RibbonArticle}"'),
    @('Content="Qualité"', 'Content="{DynamicResource RibbonQuality}"'),
    @('Text="Statut du Projet :"', 'Text="{DynamicResource LabelProjectStatus}"'),
    @('Text="Dernière sauvegarde : Jamais"', 'Text="{DynamicResource LabelLastSaved} {DynamicResource StatusNever}"'),
    @('ToolTip="Réduire"', 'ToolTip="{DynamicResource TooltipMinimize}"'),
    @('ToolTip="Agrandir/Restaurer"', 'ToolTip="{DynamicResource TooltipMaximize}"'),
    @('ToolTip="Fermer"', 'ToolTip="{DynamicResource TooltipClose}"'),
    @('ToolTip="Enregistrer"', 'ToolTip="{DynamicResource TooltipSave}"')
)

foreach ($pair in $replacements) {
    if ($content.Contains($pair[0])) {
        Write-Host "Replacing $($pair[0])"
        $content = $content.Replace($pair[0], $pair[1])
    }
}

Set-Content "Windows\ProjectWindow.xaml" $content -Encoding UTF8
