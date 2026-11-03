$ErrorActionPreference = "Stop"

# Publishes the app and builds the installer into dist\.
# install-forge.ifp uses paths relative to the repository root, InstallForge resolves them against the current directory.

$appProject = Join-Path $PSScriptRoot "Lalamachine.Desktop\Lalamachine.Desktop.csproj"
$installerProject = Join-Path $PSScriptRoot "install-forge.ifp"
$publishDir = "Lalamachine.Desktop\bin\Release\net10.0\win-x64\publish"
$publishPath = Join-Path $PSScriptRoot $publishDir

$builder = (Get-Command ifbuildx86.exe -ErrorAction SilentlyContinue).Source
if (-not $builder) { $builder = Join-Path ${env:ProgramFiles(x86)} "solicus\InstallForge\bin\ifbuildx86.exe" }
if (-not (Test-Path $builder)) { throw "Building the installer needs InstallForge 1.6 or later (https://installforge.net)." }

# The installer packages whole folders, don't let it pick up what an earlier publish left there.
if (Test-Path $publishPath) { Remove-Item $publishPath -Recurse -Force }

# Self-contained, so that the installer doesn't need .NET. LibVLC needs its files on disk, only this platform's.
dotnet publish $appProject -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:VlcWindowsX86Enabled=false -p:VlcWindowsArm64Enabled=false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# The debug symbols of Skia and HarfBuzz, 100 MB that are no use to anyone who installs the app.
Remove-Item (Join-Path $publishPath "libSkiaSharp.pdb"), (Join-Path $publishPath "libHarfBuzzSharp.pdb") -ErrorAction SilentlyContinue

# The app's project file has the version, copy it into the installer project.
$version = ([xml](Get-Content $appProject -Raw)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { throw "No <Version> in $appProject" }
$installer = Get-Content $installerProject -Raw
$versioned = $installer -replace '(?<=<ProductInformation>[\s\S]*?<Version>)[^<]*(?=</Version>)', $version
if ($versioned -ne $installer) {
  [IO.File]::WriteAllText($installerProject, $versioned)
  Write-Host "Set the installer version to $version in install-forge.ifp, commit that."
}

# InstallForge packages a fixed list of files and folders. Don't let something new in the publish output silently go missing.
$packaged = ([xml]$versioned).InstallForgeProject.InstallerConfiguration.InstallationFiles.InstallationFile | ForEach-Object { $_.'#text' }
$notPackaged = Get-ChildItem $publishPath | ForEach-Object { Join-Path $publishDir $_.Name } | Where-Object { $_ -notin $packaged }
if ($notPackaged) { throw "install-forge.ifp doesn't package these published files, add them: $($notPackaged -join ', ')" }

$setup = "dist\lalamachine_$version.exe"
New-Item -ItemType Directory -Force (Join-Path $PSScriptRoot "dist") | Out-Null
# Not straight into dist\. A window showing that folder can hold the new file open, and the builder then can't add the setup's manifest.
$stagedSetup = Join-Path ([IO.Path]::GetTempPath()) "lalamachine_$version.exe"
Push-Location $PSScriptRoot
try {
  # The builder writes to stderr. Collect it rather than letting PowerShell turn it into an error.
  $ErrorActionPreference = "Continue"
  $output = & $builder --no-color -i $installerProject -o $stagedSetup 2>&1 | ForEach-Object { "$_" }
  $exitCode = $LASTEXITCODE
  $ErrorActionPreference = "Stop"
}
finally {
  Pop-Location
}

$output | Write-Host
if ($exitCode -ne 0) { exit $exitCode }
# A missing icon is only a warning, but the setup would come out wrong.
if ($output -match '^\[wrn\]') { throw "InstallForge reported warnings, see above." }
# Without its manifest, the setup doesn't ask for admin rights. The builder doesn't always warn about that.
if (-not [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes($stagedSetup)).Contains("requestedExecutionLevel")) { throw "The setup has no manifest: $stagedSetup" }

Move-Item $stagedSetup (Join-Path $PSScriptRoot $setup) -Force
Write-Host "Installer: $(Join-Path $PSScriptRoot $setup)"
