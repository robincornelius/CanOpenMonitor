param(
    [ValidateSet('x64', 'x86', 'All')]
    [string]$Platform = 'All',
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All',
    [string]$ApplicationDirectory,
    [switch]$OdEditorOnly,
    [switch]$BugOnly
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
        $mode = if ($BugOnly) { '--bugs' } elseif ($OdEditorOnly) { '--od-editor' } else { '--application' }
        $arguments = if ($ApplicationDirectory) { @($mode, $ApplicationDirectory) } else { @() }
        if ($BugOnly)
        {
            if (-not $ApplicationDirectory) { throw 'BugOnly requires ApplicationDirectory.' }
            $nativePlatform = if ($architecture -eq 'x86') { 'Win32' } else { 'x64' }
            $arguments += [IO.Path]::GetFullPath("$PSScriptRoot/../../libCanOpenSimple2/canfestivaldrivers/tests/build/$nativePlatform/$variant")
        }
        & "$PSScriptRoot/build/$architecture/$variant/PacketDisplayTests.exe" @arguments
        if ($LASTEXITCODE -ne 0)
        {
            throw "Display tests failed: $architecture $variant"
        }
    }
}
