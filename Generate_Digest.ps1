<#
.SYNOPSIS
    Génère un résumé textuel de la base de code pour NotebookLM.
    
.DESCRIPTION
    Ce script parcourt les dossiers AdRev.Core, AdRev.Domain et AdRev.Desktop
    et combine le contenu des fichiers .cs et .xaml dans un seul fichier texte.
#>

$outputFile = "AdRev_Codebase_Digest.txt"
$includeExtensions = @("*.cs", "*.xaml", "*.md")
$excludePaths = @("bin", "obj", "Properties", "Releases", "Setup")

Write-Host "--- Génération du Digest AdRev pour NotebookLM ---" -ForegroundColor Cyan

if (Test-Path $outputFile) { Remove-Item $outputFile }

Add-Content $outputFile "ADREV 2.0 CODEBASE DIGEST`n"
Add-Content $outputFile "Généré le : $(Get-Date)`n"
Add-Content $outputFile "====================================`n`n"

$files = Get-ChildItem -Recurse -Include $includeExtensions | Where-Object { 
    $path = $_.FullName
    $match = $false
    foreach ($exclude in $excludePaths) {
        if ($path -like "*\$exclude\*") { $match = $true; break }
    }
    -not $match
}

foreach ($file in $files) {
    $relativePath = $file.FullName.Replace((Get-Location).Path, "")
    Write-Host "Traitement de : $relativePath" -ForegroundColor Gray
    
    Add-Content $outputFile "FILE: $relativePath"
    Add-Content $outputFile "------------------------------------"
    Add-Content $outputFile (Get-Content $file.FullName -Raw)
    Add-Content $outputFile "`n`n"
}

Write-Host "`nTerminé ! Le fichier $outputFile est prêt." -ForegroundColor Green
