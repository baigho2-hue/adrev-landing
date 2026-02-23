# resolve-conflicts.ps1
# Resolves all git merge conflict markers in .cs files
# Strategy: Keep HEAD (ours) version, discard origin/main version

param(
    [string]$SearchDir = "..\AdRev.Desktop",
    [string[]]$Extensions = @("*.cs")
)

$totalFixed = 0

foreach ($ext in $Extensions) {
    $files = Get-ChildItem -Path $SearchDir -Filter $ext -Recurse -File

    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        
        if ($content -match '<<<<<<< HEAD') {
            Write-Host "Fixing: $($file.FullName)" -ForegroundColor Yellow
            
            # Process line by line to resolve conflicts
            $lines = $content -split "`r`n|`n"
            $result = [System.Collections.Generic.List[string]]::new()
            $inOrigin = $false
            $conflictCount = 0

            foreach ($line in $lines) {
                if ($line -match '^<<<<<<< ') {
                    $inOrigin = $false
                    $conflictCount++
                }
                elseif ($line -match '^=======$') {
                    $inOrigin = $true
                }
                elseif ($line -match '^>>>>>>> ') {
                    $inOrigin = $false
                }
                else {
                    # Keep HEAD lines, discard origin/main lines
                    if (-not $inOrigin) {
                        $result.Add($line)
                    }
                }
            }
            
            $newContent = $result -join "`r`n"
            Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
            Write-Host "  -> Resolved $conflictCount conflict(s)" -ForegroundColor Green
            $totalFixed++
        }
    }
}

Write-Host ""
Write-Host "Done! Fixed $totalFixed file(s)." -ForegroundColor Cyan
