dotnet build
Clear-History
cls
cdk synth --app "dotnet exec ./SolarDigest.Deploy/bin/Debug/net5.0/SolarDigest.Deploy.dll --data"
pause
