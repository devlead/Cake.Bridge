using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Core;
using Cake.Core.IO;
using System;
using System.Linq;
using static CakeBridge;

namespace CakeConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var target = Context.Argument<string>("target", "Build");
            FilePath solution = null;

            Setup(context =>
            {
                context.Information("Setting up...");
                solution = Context
                            .GetFiles("./src/*.sln")
                            .FirstOrDefault() ??
                            Context
                            .GetFiles("../../src/*.sln")
                            .FirstOrDefault();
                if (solution == null)
                    throw new Exception("Failed to find solution");
            });

            Teardown(context =>
            {
                context.Information("Tearing down...");
            });

            var restore = Task("Restore")
            .Does(() =>
            {
                Context.DotNetCoreRestore(solution.FullPath);
            });

            var build = Task("Build")
                .IsDependentOn(restore)
                .Does(() =>
                {
                    Context.DotNetCoreBuild(solution.FullPath);
                });

            RunTarget(target);
        }
    }
}