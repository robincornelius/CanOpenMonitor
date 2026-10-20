param(
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All',
    [ValidateSet('net481', 'net8', 'All')]
    [string]$Framework = 'net481'
)

$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild)
{
    throw 'Install Visual Studio with MSBuild, the v145 C++ tools, Windows SDK and the required .NET targeting packs.'
}
$targets = if ($Framework -eq 'All') { @('net481', 'net8') } else { @($Framework) }
$variants = if ($Configuration -eq 'All') { @('Debug', 'Release') } else { @($Configuration) }
foreach ($target in $targets)
{
    $properties = if ($target -eq 'net8') { @('/p:BuildNet8=true', '/p:BuildNet481=false') } else { @('/p:BuildNet8=false', '/p:BuildNet481=true') }
    foreach ($variant in $variants)
    {
        & $msbuild "$PSScriptRoot/CanMonitor/CanMonitor.sln" /t:Restore,Rebuild /p:Platform=x64 "/p:Configuration=$variant" @properties /nr:false /m /nologo /v:minimal
        if ($LASTEXITCODE -ne 0)
        {
            throw "CanMonitor build failed: $target $variant"
        }
        if ($target -eq 'net8')
        {
            # Drivers are native and shared by both runtimes; managed outputs stay separate.
            $output = "$PSScriptRoot/CanMonitor/bin/x64/$variant"
            New-Item "$output/net8.0/drivers" -ItemType Directory -Force | Out-Null
            Copy-Item "$output/drivers/*.dll" "$output/net8.0/drivers" -Force
            foreach ($native in @('nanomsg.dll', 'ftd2xx.dll'))
            {
                if (Test-Path "$output/$native")
                {
                    Copy-Item "$output/$native" "$output/net8.0" -Force
                }
            }
        }
    }
}
