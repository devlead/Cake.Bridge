dotnet tool restore
dotnet script .\build.csx $args
exit $LASTEXITCODE