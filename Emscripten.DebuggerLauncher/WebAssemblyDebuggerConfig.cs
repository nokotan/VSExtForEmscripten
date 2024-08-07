﻿using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Emscripten.DebuggerLauncher
{
    public class WasmDebuggerConfig
    {
        public string type { get; set; }
        public string url { get; set; }
        [JsonProperty("$adapter")]
        public string adapterExecutable { get; set; }
        public string[] flags { get; set; }
        public string userDataDir { get; set; }
        public bool? ignoreDefaultFlags { get; set; }

        public static WasmDebuggerConfig GenerateChromeLaunchConfig(
                string inspectedPage,
                string chromeFlags,
                string chromeUserDataDirectory,
                string chromeIgnoreDefaultFlags,
                string debugAdapterExecutable
            )
        {
            var splittedFlags = chromeFlags.Split(';');

            // remove empty string away
            splittedFlags = splittedFlags.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            var config = new WasmDebuggerConfig()
            {
                type = "wasm-chrome",
                url = inspectedPage,
                flags = splittedFlags,
                adapterExecutable = debugAdapterExecutable?.Replace('\\', '/'),
            };

            if (!string.IsNullOrWhiteSpace(chromeUserDataDirectory))
            {
                config.userDataDir = chromeUserDataDirectory;
            }

            if (!string.IsNullOrWhiteSpace(chromeIgnoreDefaultFlags))
            {
                config.ignoreDefaultFlags = bool.Parse(chromeIgnoreDefaultFlags);
            }

            return config;
        }
    }

    public class NodeWasmDebuggerConfig
    {
        public string type { get; set; }
        public string program { get; set; }
        [JsonProperty("$adapter")]
        public string adapterExecutable { get; set; }
        public string node { get; set; }
        public string cwd { get; set; }

        public static NodeWasmDebuggerConfig GenerateNodeLaunchConfig(
                string program,
                string nodeExecutable,
                string nodeWorkingDirectory,
                string debugAdapterExecutable
            )
        {
            var config = new NodeWasmDebuggerConfig()
            {
                type = "wasm-node",
                program = program,
                node = nodeExecutable,
                adapterExecutable = debugAdapterExecutable.Replace('\\', '/'),
            };

            if (!string.IsNullOrWhiteSpace(nodeWorkingDirectory))
            {
                config.cwd = nodeWorkingDirectory;
            }

            return config;
        }
    }

    public class VSCodeDebuggerConfig
    {
        public string type { get; set; }
        public string url { get; set; }
        
        [JsonProperty("$debugServer")]
        public int? port { get; set; }

        public bool? enableDWARF { get; set; }

        public static VSCodeDebuggerConfig GenerateChromeLaunchConfig(
                string inspectedPage
            )
        {
            var config = new VSCodeDebuggerConfig()
            {
                type = "pwa-chrome",
                url = inspectedPage
            };

            return config;
        }
    }
}
