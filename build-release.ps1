$PathToPublish = "..\Lalamachine.Wpf\bin\Release\net8.0-windows\win-x64\publish"

dotnet publish .\Lalamachine.Wpf\Lalamachine.Wpf.csproj -c Release
Remove-Item "${PathToPublish}\\assets" -Recurse -Force
Copy-Item ..\Lalamachine.Wpf\assets $PathToPublish -Recurse -Force
