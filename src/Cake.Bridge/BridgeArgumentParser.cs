using System;
using System.Collections.Generic;
using System.Linq;

namespace Cake.Bridge
{
    internal class BridgeArgumentParser
    {
        public static ILookup<string, string> GetParsedCommandLine()
            => ParseCommandLine()
                .ToLookup(
                    key => key.Key,
                    value => value.Value,
                    StringComparer.OrdinalIgnoreCase
                );
                
        private static IEnumerable<KeyValuePair<string, string>> ParseCommandLine()
        {
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

                yield return new KeyValuePair<string, string>(key, value.Trim('"'));
            }
        }
    }
}
