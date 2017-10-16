$csProjFiles = Get-ChildItem -Path $PSScriptRoot -Include *Tests.csproj -Recurse

foreach ($csProjFile in $csProjFiles)
{
    dotnet test $csProjFile
}