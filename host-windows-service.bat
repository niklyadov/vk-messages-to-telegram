set serviceName=VkToTg
@REM set hostedPath=%cd%\Hosted
set hostedPath=C:\Hosted
set servicePath=%hostedPath%\%serviceName%

net stop %serviceName%

del /y /S %servicePath%\*

dotnet publish -c Release --no-self-contained --output %servicePath% %serviceName%.sln
sc.exe delete %serviceName%
sc.exe create %serviceName% binPath="%servicePath%\%serviceName%.exe"

copy /y "%hostedPath%\%serviceName%-config.json" "%servicePath%\appsettings.json"

net start %serviceName%
pause