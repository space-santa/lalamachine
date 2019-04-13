heat.exe dir ..\Lalamachine\Release\Native -cg LalaGroup -out LalaGroup.wxs

(Get-Content .\LalaGroup.wxs) | ForEach-Object { $_.replace('PUT-GUID-HERE', [guid]::NewGuid()) } | Set-Content .\LalaGroup.wxs
(Get-Content .\LalaGroup.wxs) | ForEach-Object { $_.replace('SourceDir', "..\Lalamachine\Release\native") } | Set-Content .\LalaGroup.wxs

# Needs to replace the directory with installdir.
# Needs to update the app-icon.

