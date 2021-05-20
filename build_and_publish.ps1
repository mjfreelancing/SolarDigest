dotnet clean
dotnet build --force --configuration Release
cd SolarDigest.Api
dotnet publish --configuration Release
cd..
