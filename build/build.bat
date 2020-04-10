@ECHO OFF

SET Arbor.Build.Vcs.Branch.Name=%GITHUB_REF%
CALL dotnet arbor-build

EXIT /B %ERRORLEVEL%