$ErrorActionPreference = "Stop"

Write-Host "Starting AdRev Installer Build..." -ForegroundColor Cyan

# 1. Check WiX
if (-not (Get-Command "wix" -ErrorAction SilentlyContinue)) {
    Write-Host "WiX Toolset not found." -ForegroundColor Yellow
    exit 1
}

# 1.1 Read Version from Project
$projectFile = "..\AdRev.Desktop\AdRev.Desktop.csproj"
if (Test-Path $projectFile) {
    [xml]$xml = Get-Content $projectFile
    $version = $xml.Project.PropertyGroup.Version
    if (-not $version) { $version = "1.0.0" }
}
else {
    $version = "1.0.0"
}
Write-Host "Target Version: $version" -ForegroundColor Magenta



# 2. Obfuscation setup (Security)
if (-not (Get-Command "obfuscar.console" -ErrorAction SilentlyContinue)) {
    Write-Host "Installing Obfuscar..." -ForegroundColor Yellow
    dotnet tool install --global Obfuscar.GlobalTool
}

# 3. Clean and Build Release
Write-Host "Building AdRev.Desktop (Release)..." -ForegroundColor Cyan
dotnet build "..\AdRev.Desktop\AdRev.Desktop.csproj" -c Release
Write-Host "Building AdRev.CLI (Release)..." -ForegroundColor Cyan
dotnet build "..\AdRev.CLI\AdRev.CLI.csproj" -c Release
Write-Host "Building AdRev.LicenseGenerator (Release)..." -ForegroundColor Cyan
dotnet build "..\AdRev.LicenseGenerator\AdRev.LicenseGenerator.csproj" -c Release

# 4. Obfuscate
Write-Host "Obfuscating Assemblies..." -ForegroundColor Cyan
# Ensure we are in the correct directory for relative paths in obfuscar.xml
Push-Location "..\AdRev.Desktop"
obfuscar.console obfuscar.xml
if ($LASTEXITCODE -ne 0) { Write-Host "Obfuscation failed" -ForegroundColor Red; exit 1 }
Pop-Location

# 5. Publish (Using Obfuscated Assemblies)
# Trick: We will publish from the Obfuscated folder? No, that's hard.
# We will just accept that 'dotnet publish' might overwrite.
# For SingleFile, we need to replace the DLLs in the obj folder or use a hacked target.
# Let's simplify: We will provide the Obfuscated DLLs alongside the EXE in Releases for manual inspection? No.
# Only way to effectively bundle obfuscated code in SingleFile via script:
# Copy Obfuscated DLLs *over* the build output in bin\Release\net8.0-windows\
# Then run publish with --no-build to use existing binaries?
Write-Host "Patching Release build with Obfuscated assemblies..." -ForegroundColor Cyan
Copy-Item "..\AdRev.Desktop\Obfuscated\*.dll" "..\AdRev.Desktop\bin\Release\net8.0-windows\" -Force

Write-Host "Publishing AdRev.Desktop (Using Patched Assemblies)..." -ForegroundColor Cyan
$projectPath = "..\AdRev.Desktop\AdRev.Desktop.csproj"
$publishDir = "..\AdRev.Desktop\bin\Release\net8.0-windows\win-x64\publish"

$publishArgs = @(
    "publish",
    $projectPath,
    "-c", "Release",
    "-r", "win-x64",
    "--no-build",        # Critical: extensive use of pre-built (and patched) logic
    "--self-contained", "true",
    "-p:PublishSingleFile=false",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-o", $publishDir
)

& dotnet $publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed." -ForegroundColor Red
    exit 1
}


# 5.1 Harvest Files
Write-Host "Harvesting Files..." -ForegroundColor Cyan
.\harvest.ps1 -SourceDir $publishDir -OutputFile "GeneratedFiles.wxs" -ComponentGroupId "PublishedComponents" -DirectoryId "INSTALLFOLDER"

# 6. Build MSI
Write-Host "Building MSI (Version $version)..." -ForegroundColor Cyan
& wix build Product.wxs GeneratedFiles.wxs -d Version=$version -arch x64 `
    "Localization\fr-FR.wxl" `
    -o "AdRev$version.msi"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Success: AdRev1.0.msi created." -ForegroundColor Green
    
    # 4. Organize Outputs
    Write-Host "Organizing releases..." -ForegroundColor Cyan
    $releaseDir = "..\Releases"
    if (-not (Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir | Out-Null }
    
    Copy-Item "AdRev$version.msi" -Destination $releaseDir -Force
    Copy-Item "$publishDir\AdRev.Desktop.exe" -Destination $releaseDir -Force

    # 7. Build Bundle
    Write-Host "Building Bundle..." -ForegroundColor Cyan
    
    # Locate WiX Extension (Safe Fallback)
    $balExt = "WixToolset.Bal.wixext" # Default
    $candidate = Get-ChildItem -Path "$env:USERPROFILE\.nuget\packages\wixtoolset.bal.wixext" -Recurse -Filter "WixToolset.BootstrapperApplications.wixext.dll" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName -First 1
    if ($candidate) {
        Write-Host "Found local extension: $candidate" -ForegroundColor Gray
        $balExt = $candidate
    }

    & wix build Bundle.wxs -d Version=$version -d MsiSource="AdRev$version.msi" -ext $balExt `
        "Localization\fr-FR.wxl" `
        -o "AdRevSetup.exe"
    
    if ($LASTEXITCODE -eq 0) {
        Copy-Item "AdRevSetup.exe" -Destination $releaseDir -Force
        Write-Host "Success: AdRevSetup.exe created." -ForegroundColor Green
    }
    else {
        Write-Host "Bundle Build failed." -ForegroundColor Red
    }
    
    # Copy CLI Tools
    $toolsDir = "$releaseDir\Tools"
    if (-not (Test-Path $toolsDir)) { New-Item -ItemType Directory -Path $toolsDir | Out-Null }
    Copy-Item "..\AdRev.CLI\bin\Release\net8.0-windows\AdRev.CLI.exe" -Destination $toolsDir -Force
    Copy-Item "..\AdRev.CLI\bin\Release\net8.0-windows\AdRev.CLI.runtimeconfig.json" -Destination $toolsDir -Force
    Copy-Item "..\AdRev.CLI\bin\Release\net8.0-windows\*.dll" -Destination $toolsDir -Force # Dependencies (Core/Domain)

    # Copy License Generator
    $genDir = "$releaseDir\LicenseGenerator"
    if (-not (Test-Path $genDir)) { New-Item -ItemType Directory -Path $genDir | Out-Null }
    # Copy all files from build output
    Copy-Item "..\AdRev.LicenseGenerator\bin\Release\net8.0-windows\*" -Destination $genDir -Recurse -Force

    Write-Host "🎉 Build Complete!" -ForegroundColor Green
    Write-Host "Files available in: $(Resolve-Path $releaseDir)" -ForegroundColor White
    Write-Host "  - Installer: AdRevSetup.exe (Bundle)" -ForegroundColor Yellow
    Write-Host "  - Setup: AdRev$version.msi" -ForegroundColor Gray
    Write-Host "  - Portable: AdRev.Desktop.exe" -ForegroundColor Gray
    Write-Host "  - Tools: Tools\AdRev.CLI.exe" -ForegroundColor Gray
    Write-Host "  - Generator: LicenseGenerator\AdRev.LicenseGenerator.exe" -ForegroundColor Gray
}
else {
    Write-Host "MSI Build failed." -ForegroundColor Red
}
