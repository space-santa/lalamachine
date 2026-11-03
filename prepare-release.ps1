param([string]$Version)

$ErrorActionPreference = "Stop"

# Turns the fragments in changes\ into a new entry in CHANGELOG, and sets the version in the app's project file.
# Without a version it only shows what the entry would look like and changes nothing.

$appProject = Join-Path $PSScriptRoot "Lalamachine.Desktop/Lalamachine.Desktop.csproj"
$changelog = Join-Path $PSScriptRoot "CHANGELOG"
$changesDir = Join-Path $PSScriptRoot "changes"

# The types of fragments in the order they go in the entry, with what goes in front of their text.
$types = [ordered]@{ feature = ""; bugfix = "Bugfix: " }

$fragments = @(Get-ChildItem $changesDir -File | Where-Object { $_.Name -ne "README.md" })
if ($fragments.Count -eq 0) { throw "There are no fragments in changes\, so there's nothing for the changelog." }

# It would be deleted without ending up in the changelog.
$unknown = @($fragments | Where-Object { $_.Name -notmatch "^.+\.($($types.Keys -join '|'))\.md$" })
if ($unknown.Count -gt 0) { throw "These don't look like <name>.<type>.md, with a type of $($types.Keys -join ' or '): $($unknown.Name -join ', ')" }

$current = ([xml][IO.File]::ReadAllText($appProject)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $current) { throw "No <Version> in $appProject" }
if ($Version) {
  if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw "Give the version with three numbers, like $current." }
  if ([version]$Version -le [version]$current) { throw "The version has to be higher than the current $current." }
}

$items = foreach ($type in $types.Keys) {
  foreach ($fragment in $fragments | Where-Object { $_.Name -like "*.$type.md" } | Sort-Object Name) {
    # ReadAllText because Windows PowerShell's Get-Content doesn't read these as UTF-8.
    $text = ([IO.File]::ReadAllText($fragment.FullName).Trim() -split '\s*\r?\n\s*') -join ' '
    if (-not $text) { throw "$($fragment.Name) is empty." }
    "  * $($types[$type])$text"
  }
}

$date = (Get-Date).ToString("ddd MMM dd yyyy", [Globalization.CultureInfo]::InvariantCulture)
$heading = "* $date space-santa $(if ($Version) { $Version } else { '<version>' })"
$entry = (@($heading) + $items) -join "`n"

if (-not $Version) {
  Write-Host "The version is $current now. With the changes\ as they are, the next entry in CHANGELOG would be:`n"
  Write-Host $entry
  return
}

[IO.File]::WriteAllText($changelog, "$entry`n`n" + [IO.File]::ReadAllText($changelog))
[IO.File]::WriteAllText($appProject, ([IO.File]::ReadAllText($appProject) -replace '(?<=<Version>)[^<]*(?=</Version>)', $Version))
$fragments | Remove-Item

Write-Host $entry
Write-Host "`nLook it over and commit it, then run build-release.ps1."
