heat.exe dir ..\Lalamachine\Release\Native -cg LalaGroup -out LalaGroup.wxs

$XMLfile = '.\LalaGroup.wxs'
[XML]$xml = Get-Content $XMLfile
$target = $xml.Wix.Fragment.DirectoryRef | Where-Object { $_.id -eq "TARGETDIR" }
$targetId = $target.directory.id

$cleanGuids = (Get-Content .\LalaGroup.wxs) | ForEach-Object { 
    $_ -replace 'PUT-GUID-HERE', [guid]::NewGuid()
}

$cleanDirs = $cleanGuids | ForEach-Object { 
    $_ -replace $targetId, "INSTALLDIR"
}

$cleanDirs | ForEach-Object { 
    $_ -replace 'SourceDir', "..\Lalamachine\Release\native"
} | Set-Content .\LalaGroup.wxs

# Now copy this into the lalamachine.exe element:
# <Shortcut Id="ExeShortcut" Directory="ProgramMenuDir" Name="lalamachine" Advertise="yes" Icon="StartMenuIcon.exe" IconIndex="0" />
# And remove the targetdir fragment.
                     