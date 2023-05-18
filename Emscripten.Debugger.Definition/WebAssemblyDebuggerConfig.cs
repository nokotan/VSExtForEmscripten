using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

#if VS2017
namespace Emscripten.Debugger.Definition.vs2017
#else
namespace Emscripten.Debugger.Definition
#endif
{
    public class WebAssemblyDebuggerConfig
    {
        public string type { get; set; }
        public string url { get; set; }
        [JsonPropertyName("$adapter")]
        public string adapterExecutable { get; set; }
        public string[] flags { get; set; }
        public string userDataDir { get; set; }
        public bool? ignoreDefaultFlags { get; set; }

        public static WebAssemblyDebuggerConfig GenerateChromeLaunchConfig(
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

            var config = new WebAssemblyDebuggerConfig()
            {
                type = "wasm-chrome",
                url = inspectedPage,
                flags = splittedFlags,
                adapterExecutable = debugAdapterExecutable.Replace('\\', '/'),
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
}
