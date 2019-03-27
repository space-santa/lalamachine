$LalaServer = Start-Process ./LalaServer/LalaServer.exe -NoNewWindow -PassThru
Start-Process lalamachine.exe -Wait
Stop-Process $LalaServer
