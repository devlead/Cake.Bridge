using System;
using System.Collections.Generic;
using Cake.Core.Configuration;

namespace Cake.Bridge
{
    internal class BridgeConfiguration : ICakeConfiguration
    {
        private IDictionary<string, string> Configuration { get; }

        public string GetValue(string key)
        {
            return Configuration.TryGetValue(key, out string value) ? value : null;
        }

        public BridgeConfiguration(IDictionary<string, string> configuration = null)
        {
            Configuration = configuration ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
