Add-Type -AssemblyName System.IO.Compression.FileSystem
Function Unzip
{
    param([string]$zipfile, [string]$outpath)

    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

Function NugetInstall
{
    param(
        [string]$PackageId,
        [string]$PackageVersion,
        [string]$ToolsPath
    )
    (New-Object System.Net.WebClient).DownloadFile("https://www.nuget.org/api/v2/package/$PackageId/$PackageVersion", "$ToolsPath\$PackageId.zip")
     Unzip "$ToolsPath\$PackageId.zip" "$ToolsPath/$PackageId.$PackageVersion"
     Remove-Item "$ToolsPath\$PackageId.zip"
}

[string] $CakeVersion       = "0.34.1"
[string] $BridgeVersion     = "0.0.12-alpha"
[string] $VSWhereVersion    = "2.7.1"

[string] $PSScriptRoot      = Split-Path $MyInvocation.MyCommand.Path -Parent
[string] $ToolsPath         = Join-Path $PSScriptRoot "tools"
[string] $CakeCorePath      = Join-Path $ToolsPath "Cake.Core.$CakeVersion/lib/net46/Cake.Core.dll"
[string] $CakeCommonPath    = Join-Path $ToolsPath "Cake.Common.$CakeVersion/lib/net46/Cake.Common.dll"
[string] $CakeBridgePath    = Join-Path $ToolsPath "Cake.Bridge.$BridgeVersion/lib/net46/Cake.Bridge.dll"
[string] $VSWherePath       = Join-Path $ToolsPath "vswhere.$VSWhereVersion/tools/vswhere.exe"

if (!(Test-Path $ToolsPath))
{
    New-Item -Path $ToolsPath -Type directory | Out-Null
}

if (!(Test-Path $CakeCorePath))
{
   NugetInstall 'Cake.Core' $CakeVersion $ToolsPath
}

if (!(Test-Path $CakeCommonPath))
{
   NugetInstall 'Cake.Common' $CakeVersion $ToolsPath
}

if (!(Test-Path $CakeBridgePath))
{
   NugetInstall 'Cake.Bridge' $BridgeVersion $ToolsPath
}

if (!(Test-Path $VSWherePath))
{
   NugetInstall 'vswhere' $VSWhereVersion $ToolsPath
}


$ids                        = 'Community', 'Professional', 'Enterprise', 'BuildTools' | ForEach { 'Microsoft.VisualStudio.Product.' + $_ }
$instance                   = &$VSWherePath -latest -products $ids -requires 'Microsoft.Component.MSBuild' -format json `
                                | ConvertFrom-Json `
                                | Select-Object -First 1

[string] $CSIPath           = join-path $instance.installationPath 'MSBuild\15.0\Bin\Roslyn\csi.exe'

if (!(test-path $CSIPath)) {
  $CSIPath           = join-path $instance.installationPath 'MSBuild\Current\Bin\Roslyn\csi.exe'
  if (!(test-path $CSIPath)) {
   $CSIPath
    exit 404
  }
}

&$CSIPath .\build.csx $args
exit $LASTEXITCODE