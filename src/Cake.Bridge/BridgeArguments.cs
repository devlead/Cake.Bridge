using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Core;

namespace Cake.Bridge
{
    internal class BridgeArguments : ICakeArguments
    {
        private IDictionary<string, string> Arguments { get; }
        public bool HasArgument(string name)
        {
            return Arguments.ContainsKey(name);
        }

        public string GetArgument(string name)
        {
            string value;
            return Arguments.TryGetValue(name, out value) ? value : null;
        }

        public BridgeArguments()
        {
            Arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // Naive PoC  :)
            var args = Environment.GetCommandLineArgs();
            for (int index = 0, peek = 1; index < args.Length; index++, peek++)
            {
                var arg = args[index];
                if (arg.FirstOrDefault() != '-')
                    continue;

                var key = string.Concat(arg.SkipWhile(c => c == '-').TakeWhile(c => c != '='));
                var value = string.Concat(arg.SkipWhile(c => c != '=').Skip(1));
                if (string.IsNullOrEmpty(value) && (peek < args.Length) && args[peek].FirstOrDefault() != '-')
                {
                    index = peek;
                    value = args[peek];
                }

                Arguments[key] = value.Trim('"');
            }
        }
    }
}
