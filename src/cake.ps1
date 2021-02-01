######################################################################
## NAMESPACE IMPORTS
######################################################################
using namespace Cake.Bridge
using namespace Cake.Common
using namespace Cake.Common.Diagnostics
using namespace Cake.Common.IO
using namespace Cake.Common.Tools.DotNetCore
using namespace Cake.Common.Tools.DotNetCore.Build
using namespace Cake.Common.Tools.DotNetCore.Pack
using namespace Cake.Core
using namespace Cake.Core.IO
using namespace Cake.Core.Scripting
using namespace System
using namespace System.Linq
$ErrorActionPreference = "Stop"

######################################################################
## FETCH DEPENDENCIES
######################################################################
[string] $CakeVersion       = '1.0.0-rc0002'
[string] $BridgeVersion     = '0.0.16-alpha'

[string] $PSScriptRoot      = Split-Path $MyInvocation.MyCommand.Path -Parent
[string] $ToolsPath         = Join-Path $PSScriptRoot "tools"
[string] $CakeCorePath      = Join-Path $ToolsPath "Cake.Core.$CakeVersion/lib/net46/Cake.Core.dll"
[string] $CakeCommonPath    = Join-Path $ToolsPath "Cake.Common.$CakeVersion/lib/net46/Cake.Common.dll"
[string] $CakeBridgePath    = Join-Path $ToolsPath "Cake.Bridge.$BridgeVersion/lib/net46/Cake.Bridge.dll"

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

######################################################################
## Reference DEPENDENCIES
######################################################################
Add-Type -Path $CakeCorePath
Add-Type -Path $CakeCommonPath
Add-Type -Path $CakeBridgePath

######################################################################
## GLOBALS / HELPERS
######################################################################
[IScriptHost]   $bridge      = [CakeBridge]::GetScriptHost()
[ICakeContext]  $context    = $bridge.Context

Function Task  {
     [OutputType('Cake.Core.CakeTaskBuilder')]
     [cmdletbinding()]
     Param (
         [parameter(ValueFromPipeline)]
         [String]$TaskName
     )

     return $bridge.Task($TaskName)
}
Function Does  {
     [OutputType('Cake.Core.CakeTaskBuilder')]
     [cmdletbinding()]
     Param (
         [parameter(ValueFromPipeline)]
         $Task,
         [System.Action] $Action
     )

     return [CakeTaskBuilderExtensions]::Does($Task, $Action)
}

Function IsDependentOn  {
    [OutputType('Cake.Core.CakeTaskBuilder')]
    [cmdletbinding()]
    Param (
        [parameter(ValueFromPipeline)]
        $Task,
        $Dependency
    )

    return [CakeTaskBuilderExtensions]::IsDependentOn($Task, $Dependency)
}

Function RunTarget  {
    [cmdletbinding()]
    Param (
        [parameter(ValueFromPipeline)]
        $Task
    )

    $bridge.RunTarget($Task.Task.Name) | Out-Null
}

Function Setup {
    param([Action[ICakeContext]] $action)
    $bridge.Setup($action)
}

Function Teardown {
    param([Action[ITeardownContext]] $action)
    $bridge.Teardown($action)
}