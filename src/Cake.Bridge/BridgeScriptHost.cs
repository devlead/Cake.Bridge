using Cake.Core;
using Cake.Core.Scripting;
using Cake.Common.Diagnostics;
using System;

namespace Cake.Bridge
{
    internal class BridgeScriptHost : ScriptHost
    {
        private IExecutionStrategy Strategy { get; }
        private ICakeReportPrinter Reporter { get; }

        public override CakeReport RunTarget(string target)
        {
            try
            {
                var report = Engine.RunTarget(Context, Strategy, target);
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

        public BridgeScriptHost(ICakeEngine engine, ICakeContext context, IExecutionStrategy strategy, ICakeReportPrinter reporter)
        : base(engine, context)
    {
            Strategy = strategy;
            Reporter = reporter;
        }
    }
}
