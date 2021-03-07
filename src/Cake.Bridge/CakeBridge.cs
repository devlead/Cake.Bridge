using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cake.Bridge;
using Cake.Core;
using Cake.Core.Scripting;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;
using Cake.Core.Configuration;

// ReSharper disable once CheckNamespace
public static class CakeBridge
{
    private static IScriptHost _scriptHost = GetScriptHost();

    public static IScriptHost ScriptHost => _scriptHost;
    public static ICakeContext Context => ScriptHost.Context;
    public static IReadOnlyList<ICakeTaskInfo> Tasks => ScriptHost.Tasks;

    public static CakeTaskBuilder Task(string name) => ScriptHost.Task(name);

    public static void Setup(Action<ICakeContext> action) => ScriptHost.Setup(action);

    public static void Setup<TData>(Func<ISetupContext, TData> action)
        where TData : class
        => ScriptHost.Setup(action);

    public static void Teardown(Action<ITeardownContext> action) => ScriptHost.Teardown(action);
    
    public static void TaskSetup(Action<ITaskSetupContext> action) => ScriptHost.TaskSetup(action);

    public static void TaskTeardown(Action<ITaskTeardownContext> action) => ScriptHost.TaskTeardown(action);

    public static CakeReport RunTarget(string target) => ScriptHost.RunTarget(target);

    public static Task<CakeReport> RunTargetAsync(string target) => ScriptHost.RunTargetAsync(target);

    public static void TaskSetup<TData>(Action<ITaskSetupContext, TData> action)
        where TData : class
        => ScriptHost.TaskSetup(action);

    public static void TaskTeardown<TData>(Action<ITaskTeardownContext, TData> action)
        where TData : class
        => ScriptHost.TaskTeardown(action);

    public static void Teardown<TData>(Action<ITeardownContext, TData> action)
        where TData : class
        => ScriptHost.Teardown(action);

    public static IScriptHost GetScriptHost()
    {
        
        IFileSystem fileSystem = new FileSystem();
        ICakeDataService data = new CakeDataService();
        ICakeEnvironment environment = new CakeEnvironment(
            new CakePlatform(),
            new CakeRuntime()
            );
        var console = new CakeConsole(environment);
        ICakeLog log = new CakeBuildLog(console);
        IGlobber globber = new Globber(fileSystem, environment);
        ICakeArguments arguments = new CakeArguments(BridgeArgumentParser.GetParsedCommandLine());
        ICakeConfiguration configuration = new CakeConfiguration(new Dictionary<string, string>());
        IToolLocator tools = new ToolLocator(
                    environment,
                    new ToolRepository(environment),
                    new ToolResolutionStrategy(
                        fileSystem,
                        environment,
                        globber,
                        configuration,
                        log
                        )
                    );
        ICakeContext context = new CakeContext(
                fileSystem,
                environment,
                globber,
                log,
                arguments,
                new ProcessRunner(fileSystem, environment, log, tools, configuration),
                new WindowsRegistry(),
                tools,
                data,
                configuration
            );

        return new BridgeScriptHost(
                new CakeEngine(data, log),
                context,
                new DefaultExecutionStrategy(log),
                new CakeReportPrinter(console, context),
                arguments
                );
    }

    /// <summary>
    /// Replaces the <see cref="ScriptHost"/> with a new one
    /// </summary>
    public static void ResetScriptHost()
    {
        Interlocked.Exchange(ref _scriptHost, GetScriptHost());
    }
}
