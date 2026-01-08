@ECHO OFF

SET Arbor.Build.Vcs.Branch.Name=%GITHUB_REF%
SET Arbor.Build.NuGet.Package.Artifacts.CreateOnAnyBranchEnabled=true
CALL dotnet arbor-build

EXIT /B %ERRORLEVEL%