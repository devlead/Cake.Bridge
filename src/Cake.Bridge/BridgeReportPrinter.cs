using System;
using System.Globalization;
using System.Linq;
using Cake.Core;
using Cake.Core.Diagnostics;
/*
namespace Cake.Bridge
{
    internal class BridgeReportPrinter : ICakeReportPrinter
    {
        static readonly int[] defaultMaxTaskLength = new[] { 29 };
        private ICakeContext Context { get; }

        public BridgeReportPrinter(ICakeContext context)
        {
            Context = context;
        }

        public void Write(CakeReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            var maxTaskNameLength = report
                .Select(item => item.TaskName.Length)
                .Concat(defaultMaxTaskLength)
                .Max() + 1;

            string itemFormat = $"{{0,-{maxTaskNameLength}}}{{1,-20}}\r\n";
            var divider = new string('-', 20 + maxTaskNameLength);

            Console.Write(
                report
                .Where(item => item.ExecutionStatus != CakeTaskExecutionStatus.Delegated || Context.Log.Verbosity >= Verbosity.Verbose)
                .Aggregate(
                    // Header
                    new System.Text.StringBuilder()
                        .AppendLine(" ")
                        .AppendFormat(itemFormat, "Task", "Duration")
                        .AppendLine(divider),
                    // Tasks
                    (sb, item) => sb
                        .AppendFormat(
                            itemFormat,
                            item.TaskName,
                            (item.ExecutionStatus == CakeTaskExecutionStatus.Skipped)
                                ? "Skipped"
                                : item.Duration.ToString("c", CultureInfo.InvariantCulture)
                        ),
                    // Footer
                    sb=>sb
                        .AppendLine(divider)
                        .AppendFormat(
                            itemFormat,
                            "Total:",
                            TimeSpan
                                .FromTicks(report.Sum(item => item.Duration.Ticks))
                                .ToString("c", CultureInfo.InvariantCulture)
                        )
                        .ToString()
                    )
                );
            
        }
    }
}
*/