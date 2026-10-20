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
    throw 'Install MSBuild and the .NET Framework 4.8.1 targeting pack.'
}
$architectures = if ($Platform -eq 'All') { @('x64', 'x86') } else { @($Platform) }
$configurations = if ($Configuration -eq 'All') { @('Debug', 'Release') } else { @($Configuration) }
foreach ($architecture in $architectures)
{
    foreach ($variant in $configurations)
    {
        & $msbuild "$PSScriptRoot/PdoEditorTests.csproj" /t:Rebuild "/p:Platform=$architecture" "/p:Configuration=$variant" /nologo /v:minimal
        if ($LASTEXITCODE -ne 0)
        {
            throw "PDO editor test build failed: $architecture $variant"
        }
        & "$PSScriptRoot/build/$architecture/$variant/PdoEditorTests.exe"
        if ($LASTEXITCODE -ne 0)
        {
            throw "PDO editor tests failed: $architecture $variant"
        }
    }
}
