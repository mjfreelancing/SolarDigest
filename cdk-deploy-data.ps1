dotnet build
Clear-History
cls
cdk bootstrap "aws://550269505143/ap-southeast-2" --profile personal 
cdk deploy --app "dotnet exec ./SolarDigest.Deploy/bin/Debug/net5.0/SolarDigest.Deploy.dll --data" --profile personal --require-approval never --verbose