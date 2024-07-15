using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;

#if VS2017
namespace Emscripten.DebuggerLauncher.vs2017
#else
namespace Emscripten.DebuggerLauncher
#endif
{
    [ExportDebugger(DebuggerSchemaName)]
    [AppliesTo(DebuggerSchemaName)]
    public class NodeDebuggerLaunchProvider : DebugLaunchProviderBase
    {
#if VS2017
        internal const string DebuggerSchemaName = WasmDebuggerVS2017.SchemaName;
#else
        internal const string DebuggerSchemaName = NodeWasmDebugger.SchemaName;
#endif

        [ImportingConstructor]
        public NodeDebuggerLaunchProvider(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        [ExportPropertyXamlRuleDefinition("Emscripten.DebuggerLauncher, Version=1.0.0.0, Culture=neutral", "XamlRuleToCode:NodeWasmDebugger.xaml", "Project")]
        [AppliesTo(DebuggerSchemaName)]
        private object DebuggerXaml { get { throw new NotImplementedException(); } }

        [Import]
        private ProjectProperties ProjectProperties { get; set; }

        public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
        {
            return Task.FromResult(true);
        }
       

        public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
        {
            var settings = new DebugLaunchSettings(launchOptions);

            var debuggerProperties = await ProjectProperties.GetNodeWasmDebuggerPropertiesAsync();
            var debugAdapterExecutable = await debuggerProperties.NodeWasmDebuggerAdapterExecutable.GetEvaluatedValueAtEndAsync();

            if (!File.Exists(debugAdapterExecutable))
            {
                throw new FileNotFoundException($@"Failed to launch WebAssembly debugger. Debugger adapter executable cannot be found. Please check WasmDebuggerAdapterExecutable in the Debugger configuration. InputValue: {debugAdapterExecutable}");
            }

            var config = NodeWebAssemblyDebuggerConfig.GenerateNodeLaunchConfig(
                program: await debuggerProperties.NodeWasmDebuggerProgram.GetEvaluatedValueAtEndAsync(),
                nodeExecutable: await debuggerProperties.NodeWasmDebuggerNodeExecutable.GetEvaluatedValueAtEndAsync(),
                nodeWorkingDirectory: await debuggerProperties.NodeWasmDebuggerWorkingDirectory.GetEvaluatedValueAtEndAsync(),
                debugAdapterExecutable: debugAdapterExecutable
            );

            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;

            settings.Executable = @"C:\Windows\System32\cmd.exe"; // dummy

            settings.LaunchDebugEngineGuid = new Guid("A18E581E-F120-4E9F-A0D4-D284EB773257");
            settings.Options = JsonConvert.SerializeObject(config);

            return new IDebugLaunchSettings[] { settings };
        }
    }
}
