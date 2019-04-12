Set-Location .\Lalamachine
Remove-Item -Path Release -Recurse
dotnet.exe publish --configuration Release --output .\Release\Framework .\Lalamachine.csproj
dotnet.exe publish --configuration Release -r win10-x64 --self-contained --output .\Release\Native .\Lalamachine.csproj
Copy-Item .\assets -Destination .\Release\Native\assets -Recurse
Copy-Item .\Release\Framework\runtimes\win-x64\native\qml -Destination .\Release\Native\qml -Recurse
Copy-Item .\Release\Framework\runtimes\win-x64\native\plugins -Destination .\Release\Native\plugins -Recurse
Set-Location ..
