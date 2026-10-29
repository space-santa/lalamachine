$ErrorActionPreference = "Stop"

# Publishes the app and builds the installer into dist\.
# install-forge.ifp uses paths relative to the repository root, InstallForge resolves them against the current directory.

$appProject = Join-Path $PSScriptRoot "Lalamachine.Wpf\Lalamachine.Wpf.csproj"
$installerProject = Join-Path $PSScriptRoot "install-forge.ifp"
$publishDir = "Lalamachine.Wpf\bin\Release\net10.0-windows\win-x64\publish"

$builder = (Get-Command ifbuildx86.exe -ErrorAction SilentlyContinue).Source
if (-not $builder) { $builder = Join-Path ${env:ProgramFiles(x86)} "solicus\InstallForge\bin\ifbuildx86.exe" }
if (-not (Test-Path $builder)) { throw "Building the installer needs InstallForge 1.6 or later (https://installforge.net)." }

dotnet publish $appProject -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# The app's project file has the version, copy it into the installer project.
$version = ([xml](Get-Content $appProject -Raw)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { throw "No <Version> in $appProject" }
$installer = Get-Content $installerProject -Raw
$versioned = $installer -replace '(?<=<ProductInformation>[\s\S]*?<Version>)[^<]*(?=</Version>)', $version
if ($versioned -ne $installer) {
  [IO.File]::WriteAllText($installerProject, $versioned)
  Write-Host "Set the installer version to $version in install-forge.ifp, commit that."
}

# InstallForge packages a fixed list of files. Don't let a new file in the publish output silently go missing.
$packaged = ([xml]$versioned).InstallForgeProject.InstallerConfiguration.InstallationFiles.InstallationFile | ForEach-Object { $_.'#text' }
$notPackaged = Get-ChildItem (Join-Path $PSScriptRoot $publishDir) -File | ForEach-Object { Join-Path $publishDir $_.Name } | Where-Object { $_ -notin $packaged }
if ($notPackaged) { throw "install-forge.ifp doesn't package these published files, add them: $($notPackaged -join ', ')" }

$setup = "dist\lalamachine_$version.exe"
New-Item -ItemType Directory -Force (Join-Path $PSScriptRoot "dist") | Out-Null
Push-Location $PSScriptRoot
try {
  # The builder writes to stderr. Collect it rather than letting PowerShell turn it into an error.
  $ErrorActionPreference = "Continue"
  $output = & $builder --no-color -i $installerProject -o $setup 2>&1 | ForEach-Object { "$_" }
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

Write-Host "Installer: $(Join-Path $PSScriptRoot $setup)"
