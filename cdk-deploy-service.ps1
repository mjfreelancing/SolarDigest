dotnet build
Clear-History
cls
cdk bootstrap "aws://550269505143/ap-southeast-2"
cdk deploy --app "dotnet exec ./SolarDigest.Deploy/bin/Debug/net5.0/SolarDigest.Deploy.dll --service" --require-approval never --verbose
pause
