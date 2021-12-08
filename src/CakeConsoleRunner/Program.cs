using System.Linq;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Core;
using CakeConsoleRunner;
using static CakeBridge;

var target = Context.Argument<string>("target", "Build");

Setup(context =>
{
    context.Information("Setting up...");

    return new BuildData(
        Context
            .GetFiles("./src/*.sln")
            .FirstOrDefault()
        ??
        Context
            .GetFiles("../../src/*.sln")
            .FirstOrDefault()
        ??
        Context
            .GetFiles("../../../../*.sln")
            .FirstOrDefault());
});

Teardown(context =>
{
    context.Information("Tearing down...");
});

var restore = Task("Restore")
    .Does<BuildData>(buildData =>
    {
        Context.DotNetRestore(buildData.Solution.FullPath);
    });

var build = Task("Build")
    .IsDependentOn(restore)
    .Does<BuildData>(buildData =>
    {
        Context.DotNetBuild(buildData.Solution.FullPath);
    });

RunTarget(target);