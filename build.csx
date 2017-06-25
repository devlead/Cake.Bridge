//////////////////////////////////////////////////////////////////////
// DEPENDENCIES
//////////////////////////////////////////////////////////////////////
#r "tools/Cake.Core.0.20.0/lib/net45/Cake.Core.dll"
#r "tools/Cake.Common.0.20.0/lib/net45/Cake.Common.dll"
#r "tools/Cake.Bridge.0.0.2-alpha/lib/net45/Cake.Bridge.dll"

//////////////////////////////////////////////////////////////////////
// NAMESPACE IMPORTS
//////////////////////////////////////////////////////////////////////
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Tools.DotNetCore.Pack;
using Cake.Core;
using Cake.Core.IO;
using System;
using System.Linq;
using static CakeBridge;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Context.Argument<string>("target", "Default");
var configuration = Context.Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// GLOBALS
//////////////////////////////////////////////////////////////////////
DirectoryPath nugetRoot = Context.MakeAbsolute(Context.Directory("./nuget"));
FilePath solution = null;
string  semVersion,
        assemblyVersion,
        fileVersion;

//////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
    context.Information("Setting up...");
    solution = Context
                .GetFiles("./src/*.sln")
                .FirstOrDefault();
    if (solution == null)
        throw new Exception("Failed to find solution");


    var releaseNotes = Context.ParseReleaseNotes("./ReleaseNotes.md");
    assemblyVersion =releaseNotes.Version.ToString();
    fileVersion = assemblyVersion;
    semVersion = assemblyVersion + "-alpha";

    context.Information("Executing build {0}...", semVersion);
});

Teardown(context =>
{
    context.Information("Tearing down...");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

var clean = Task("Clean")
    .Does(() =>
    {
        Context.CleanDirectories("./src/**/bin/" + configuration);
        Context.CleanDirectories("./src/**/obj/" + configuration);
        Context.CleanDirectory(nugetRoot);
    });

var restore = Task("Restore")
    .IsDependentOn(clean)
    .Does(() =>
    {
        Context.DotNetCoreRestore(solution.FullPath);
    });

var build = Task("Build")
    .IsDependentOn(restore)
    .Does(() =>
    {
        Context.DotNetCoreBuild(solution.FullPath, new DotNetCoreBuildSettings {
            Configuration = configuration,
            ArgumentCustomization = args => args
                .Append("/p:Version={0}", semVersion)
                .Append("/p:AssemblyVersion={0}", assemblyVersion)
                .Append("/p:FileVersion={0}", fileVersion)
        });
    });

var pack = Task("Pack")
    .IsDependentOn(build)
    .Does(() =>
    {
        if (!Context.DirectoryExists(nugetRoot))
        {
            Context.CreateDirectory(nugetRoot);
        }

        foreach(var project in Context
                                .GetFiles("./src/**/Cake.*.csproj")
                                .Where(file=>!file.FullPath.EndsWith("Tests")))
        {
            Context.DotNetCorePack(project.FullPath, new DotNetCorePackSettings {
                Configuration = configuration,
                OutputDirectory = nugetRoot,
                NoBuild = true,
                ArgumentCustomization = args => args
                    .Append("/p:Version={0}", semVersion)
                    .Append("/p:AssemblyVersion={0}", assemblyVersion)
                    .Append("/p:FileVersion={0}", fileVersion)
            });
        }
    });

Task("Default")
    .IsDependentOn(pack);

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);