$version = "4.99.1"
$PathToPublish = "..\Lalamachine.Wpf\bin\Release\netcoreapp3.0\win-x64\publish"

dotnet publish ../Lalamachine.Wpf/Lalamachine.Wpf.csproj -r win10-x64 -c Release -p:PublishSingleFile=true
Remove-Item "${PathToPublish}\\assets" -Recurse -Force
Copy-Item ..\Lalamachine.Wpf\assets $PathToPublish -Recurse -Force

[XML]$xml = Get-Content "$pwd\\main.wxs"
$xml.Wix.Product.SetAttribute("Version", $version)
$xml.Save("$pwd\\main.wxs")

heat.exe dir $PathToPublish -cg DependencyGroup -dr INSTALLDIR -gg -g1 -out DependencyGroup.wxs
(Get-Content .\DependencyGroup.wxs) | ForEach-Object {
    $_ -replace "SourceDir", $PathToPublish
} | Set-Content .\DependencyGroup.wxs

candle.exe main.wxs DependencyGroup.wxs
light.exe -ext WixUIExtension -out "$pwd\\msi\\lalamachine-${version}.msi" main.wixobj DependencyGroup.wixobj
