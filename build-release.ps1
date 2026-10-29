$ErrorActionPreference = "Stop"

# Publishes to Lalamachine.Wpf\bin\Release\net10.0-windows\win-x64\publish, which is what install-forge.ifp packages.
dotnet publish "$PSScriptRoot\Lalamachine.Wpf\Lalamachine.Wpf.csproj" -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
