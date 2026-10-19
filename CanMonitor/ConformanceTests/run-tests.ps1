param(
    [ValidateSet('x64', 'x86', 'All')]
    [string]$Platform = 'All',
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All'
)
$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild)
{
    throw 'Install Visual Studio MSBuild and the .NET Framework 4.8 targeting pack.'
}
$architectures = if ($Platform -eq 'All') { @('x64', 'x86') } else { @($Platform) }
$variants = if ($Configuration -eq 'All') { @('Debug', 'Release') } else { @($Configuration) }
$solutionDirectory = (Resolve-Path "$PSScriptRoot/..").Path + '\'
foreach ($architecture in $architectures)
{
    foreach ($variant in $variants)
    {
        & $msbuild "$PSScriptRoot/ConformanceTests.csproj" /t:Rebuild "/p:Platform=$architecture" "/p:Configuration=$variant" "/p:SolutionDir=$solutionDirectory" /nologo /v:minimal
        if ($LASTEXITCODE -ne 0)
        {
            throw "Conformance test build failed: $architecture $variant"
        }
        & "$PSScriptRoot/build/$architecture/$variant/ConformanceTests.exe"
        if ($LASTEXITCODE -ne 0)
        {
            throw "Conformance tests failed: $architecture $variant"
        }
    }
}
