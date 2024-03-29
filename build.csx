//////////////////////////////////////////////////////////////////////
// DEPENDENCIES
//////////////////////////////////////////////////////////////////////
#r "nuget: Cake.Bridge, 0.0.21-alpha"

//////////////////////////////////////////////////////////////////////
// NAMESPACE IMPORTS
//////////////////////////////////////////////////////////////////////
using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Core;
using Cake.Core.IO;
using System;
using System.Linq;
using static CakeBridge;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Context.Argument<string>("target", "Default");

//////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
    context.Information("Setting up...");

    var releaseNotes = Context.ParseReleaseNotes("./ReleaseNotes.md");
    var releaseVersion =releaseNotes.Version.ToString();

    var buildData = new BuildData(
            Context.MakeAbsolute(Context.Directory("./nuget")),
            Context.GetFiles("./src/*.sln")
                    .FirstOrDefault()
                    ?? throw new Exception("Failed to find solution"),
            releaseVersion + "-alpha",
            releaseVersion,
            releaseVersion,
            string.Join("\n", releaseNotes.Notes.ToArray()).Replace("\"", "\"\""),
            Context.Argument<string>("configuration", "Release"));


    context.Information("Executing build {0} ({1})...", buildData.SemVersion, buildData.Configuration);

    return buildData;
});

Teardown(context =>
{
    context.Information("Tearing down...");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

var clean = Task("Clean")
    .Does<BuildData>(buildData =>
    {
        Context.CleanDirectories("./src/**/bin/" + buildData.Configuration);
        Context.CleanDirectories("./src/**/obj/" + buildData.Configuration);
        Context.CleanDirectory(buildData.NugetRoot);
    });

var restore = Task("Restore")
    .IsDependentOn(clean)
    .Does<BuildData>(buildData =>
    {
        Context.DotNetRestore(buildData.Solution.FullPath);
    });

var build = Task("Build")
    .IsDependentOn(restore)
    .Does<BuildData>(buildData =>
    {
        Context.DotNetBuild(buildData.Solution.FullPath, new DotNetBuildSettings {
            Configuration = buildData.Configuration,
            MSBuildSettings = buildData.MSBuildSettings
        });
    });

var pack = Task("Pack")
    .IsDependentOn(build)
    .Does<BuildData>(buildData =>
    {
        if (!Context.DirectoryExists(buildData.NugetRoot))
        {
            Context.CreateDirectory(buildData.NugetRoot);
        }

        foreach(var project in Context
                                .GetFiles("./src/**/Cake.*.csproj")
                                .Where(file=>!file.FullPath.EndsWith("Tests")))
        {
            Context.DotNetPack(project.FullPath, new DotNetPackSettings {
                Configuration = buildData.Configuration,
                OutputDirectory = buildData.NugetRoot,
                NoBuild = true,
                MSBuildSettings = buildData.MSBuildSettings
            });
        }
    });

Task("Default")
    .IsDependentOn(pack);


//////////////////////////////////////////////////////////////////////
// Shared Build Data helper class
//////////////////////////////////////////////////////////////////////
public class BuildData
{
    public DirectoryPath NugetRoot { get; }
    public FilePath Solution { get; }
    public string SemVersion { get; }
    public string AssemblyVersion { get; }
    public string FileVersion { get; }
    public string ReleaseNotes { get; }
    public string Configuration { get; }
    public DotNetMSBuildSettings MSBuildSettings { get; }
    public BuildData(
        DirectoryPath nugetRoot,
        FilePath solution,
        string semVersion,
        string assemblyVersion,
        string fileVersion,
        string releaseNotes,
        string configuration
        )
    {
        NugetRoot = nugetRoot;
        Solution = solution;
        SemVersion = semVersion;
        AssemblyVersion = assemblyVersion;
        FileVersion = fileVersion;
        ReleaseNotes = releaseNotes;
        Configuration = configuration;
        MSBuildSettings = new DotNetMSBuildSettings()
                        .WithProperty("Version", SemVersion)
                        .WithProperty("AssemblyVersion", AssemblyVersion)
                        .WithProperty("FileVersion", FileVersion)
                        .WithProperty("PackageReleaseNotes", string.Concat("\"", ReleaseNotes, "\""))
                        .WithProperty("EmbedUntrackedSources", "true")
                        .WithProperty("ContinuousIntegrationBuild", !Context.BuildSystem().IsLocalBuild  ? "true" : "false");
    }
}

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);