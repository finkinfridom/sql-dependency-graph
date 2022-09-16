# SQL Dependency Graph

## How-to run tests

```
dotnet tool install -g dotnet-reportgenerator-globaltool
```

```
reportgenerator 
    -reports:"__tests__/**/TestResults/**/coverage.cobertura.xml" 
    -targetdir:"reports" 
    -reporttypes:Html
```