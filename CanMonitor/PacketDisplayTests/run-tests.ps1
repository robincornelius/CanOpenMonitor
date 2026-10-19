param(
    [ValidateSet('x64', 'x86', 'All')]
    [string]$Platform = 'All',
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All',
    [string]$ApplicationDirectory,
    [switch]$OdEditorOnly
)
$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild)
{
    throw 'Install MSBuild and the .NET Framework 4.8 targeting pack.'
}
if ($ApplicationDirectory -and ($Platform -eq 'All' -or $Configuration -eq 'All'))
{
    throw 'Specify one architecture and configuration matching ApplicationDirectory.'
}
$architectures = if ($Platform -eq 'All') { @('x64', 'x86') } else { @($Platform) }
$configurations = if ($Configuration -eq 'All') { @('Debug', 'Release') } else { @($Configuration) }
foreach ($architecture in $architectures)
{
    foreach ($variant in $configurations)
    {
        & $msbuild "$PSScriptRoot/PacketDisplayTests.csproj" /t:Rebuild "/p:Platform=$architecture" "/p:Configuration=$variant" /nologo /v:minimal
        if ($LASTEXITCODE -ne 0)
        {
            throw "Display test build failed: $architecture $variant"
        }
        $mode = if ($OdEditorOnly) { '--od-editor' } else { '--application' }
        $arguments = if ($ApplicationDirectory) { @($mode, $ApplicationDirectory) } else { @() }
        & "$PSScriptRoot/build/$architecture/$variant/PacketDisplayTests.exe" @arguments
        if ($LASTEXITCODE -ne 0)
        {
            throw "Display tests failed: $architecture $variant"
        }
    }
}
