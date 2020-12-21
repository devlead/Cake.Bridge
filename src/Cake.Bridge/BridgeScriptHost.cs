using Cake.Core;
using Cake.Core.Scripting;
using Cake.Common.Diagnostics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cake.Bridge
{
    internal class BridgeScriptHost : ScriptHost
    {
        private IExecutionStrategy Strategy { get; }
        private ICakeReportPrinter Reporter { get; }
        private ICakeArguments Arguments { get; }

        public override async Task<CakeReport> RunTargetAsync(string target)
        {
            try
            {
                if (Arguments.HasArgument("exclusive") && !StringComparer.OrdinalIgnoreCase.Equals("false", Arguments.GetArguments("exclusive").FirstOrDefault()))
                {
                    Settings.UseExclusiveTarget();
                }
                Settings.SetTarget(target);
                var report = await Engine.RunTargetAsync(Context, Strategy, Settings);
                Reporter.Write(report);
                return report;
            }
            catch(Exception ex)
            {
                Context.Error(ex);
                Environment.Exit(1);
                return null;
            }
        }

        public BridgeScriptHost(ICakeEngine engine, ICakeContext context, IExecutionStrategy strategy, ICakeReportPrinter reporter, ICakeArguments arguments)
        : base(engine, context)
        {
            Strategy = strategy;
            Reporter = reporter;
            Arguments = arguments;
        }
    }
}
