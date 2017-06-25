using System;
using System.Collections.Generic;
using Cake.Bridge;
using Cake.Core;
using Cake.Core.Scripting;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;
using Cake.Diagnostics;

public static class CakeBridge
{
    private static readonly ICakeLog log = new CakeBuildLog(new Cake.CakeConsole());
    private static readonly IFileSystem fileSystem = new FileSystem();
    private static readonly ICakeEnvironment environment = new CakeEnvironment(
        new CakePlatform(),
        new CakeRuntime(),
        log
        );
    private static readonly IGlobber globber = new Globber(fileSystem, environment);
    private static readonly ICakeContext context = new CakeContext(
            fileSystem,
            environment,
            globber,
            log,
            new BridgeArguments(),
            new ProcessRunner(environment, log),
            new WindowsRegistry(),
            new ToolLocator(
                environment,
                new ToolRepository(environment),
                new ToolResolutionStrategy(
                    fileSystem,
                    environment,
                    globber,
                    new BridgeConfiguration()
                    )
                )
        );

    public static IScriptHost ScriptHost = new BridgeScriptHost(
        new CakeEngine(log),
        context,
        new DefaultExecutionStrategy(log),
        new BridgeReportPrinter(context)
        );

    public static ICakeContext Context => ScriptHost.Context;
    public static IReadOnlyList<CakeTask> Tasks => ScriptHost.Tasks;

    public static CakeTaskBuilder<ActionTask> Task(string name)
    {
        return ScriptHost.Task(name);
    }

    public static void Setup(Action<ICakeContext> action)
    {
        ScriptHost.Setup(action);
    }

    public static void Teardown(Action<ITeardownContext> action)
    {
        ScriptHost.Teardown(action);
    }

    public static void TaskSetup(Action<ITaskSetupContext> action)
    {
        ScriptHost.TaskSetup(action);
    }

    public static void TaskTeardown(Action<ITaskTeardownContext> action)
    {
        ScriptHost.TaskTeardown(action);
    }

    public static CakeReport RunTarget(string target)
    {
        return ScriptHost.RunTarget(target);
    }
}