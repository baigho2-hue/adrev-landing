$ErrorActionPreference = "Stop"

Write-Host "Starting AdRev Installer Build..." -ForegroundColor Cyan

# 1. Check WiX
if (-not (Get-Command "wix" -ErrorAction SilentlyContinue)) {
    Write-Host "WiX Toolset not found." -ForegroundColor Yellow
    exit 1
}



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
    "-p:PublishSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-o", $publishDir
)

& dotnet $publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed." -ForegroundColor Red
    exit 1
}

# 6. Build MSI
Write-Host "Building MSI..." -ForegroundColor Cyan
& wix build Product.wxs -o "AdRevSetup.msi"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Success: AdRevSetup.msi created." -ForegroundColor Green
    
    # 4. Organize Outputs
    Write-Host "Organizing releases..." -ForegroundColor Cyan
    $releaseDir = "..\Releases"
    if (-not (Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir | Out-Null }
    
    Copy-Item "AdRevSetup.msi" -Destination $releaseDir -Force
    Copy-Item "$publishDir\AdRev.Desktop.exe" -Destination $releaseDir -Force
    
    # Copy CLI Tools
    $toolsDir = "$releaseDir\Tools"
    if (-not (Test-Path $toolsDir)) { New-Item -ItemType Directory -Path $toolsDir | Out-Null }
    Copy-Item "..\AdRev.CLI\bin\Release\net8.0-windows\AdRev.CLI.exe" -Destination $toolsDir -Force
    Copy-Item "..\AdRev.CLI\bin\Release\net8.0-windows\AdRev.CLI.runtimeconfig.json" -Destination $toolsDir -Force
    Copy-Item "..\AdRev.CLI\bin\Release\net8.0-windows\*.dll" -Destination $toolsDir -Force # Dependencies (Core/Domain)

    Write-Host "🎉 Build Complete!" -ForegroundColor Green
    Write-Host "Files available in: $(Resolve-Path $releaseDir)" -ForegroundColor White
    Write-Host "  - Setup: AdRevSetup.msi" -ForegroundColor Gray
    Write-Host "  - Portable: AdRev.Desktop.exe" -ForegroundColor Gray
    Write-Host "  - Tools: Tools\AdRev.CLI.exe" -ForegroundColor Gray
} else {
    Write-Host "MSI Build failed." -ForegroundColor Red
}
