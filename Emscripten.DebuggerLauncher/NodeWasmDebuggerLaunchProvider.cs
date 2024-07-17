using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;

namespace Emscripten.DebuggerLauncher
{
    [ExportDebugger(DebuggerSchemaName)]
    [AppliesTo(DebuggerSchemaName)]
    public class NodeWasmDebuggerLaunchProvider : DebugLaunchProviderBase
    {
        internal const string DebuggerSchemaName = NodeWasmDebugger.SchemaName;

        [ImportingConstructor]
        public NodeWasmDebuggerLaunchProvider(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        [ExportPropertyXamlRuleDefinition("Emscripten.DebuggerLauncher, Version=1.0.0.0, Culture=neutral", "XamlRuleToCode:NodeWasmDebugger.xaml", "Project")]
        [AppliesTo(DebuggerSchemaName)]
        private object DebuggerXaml { get { throw new NotImplementedException(); } }

        [Import]
        private ProjectProperties ProjectProperties { get; set; }

        public override async Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
        {
            var debuggerProperties = await ProjectProperties.GetNodeWasmDebuggerPropertiesAsync();
            var debugAdapterExecutable = await debuggerProperties.NodeWasmDebuggerAdapterExecutable.GetEvaluatedValueAtEndAsync();

            return File.Exists(debugAdapterExecutable);
        }

        public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
        {
            var settings = new DebugLaunchSettings(launchOptions);

            var debuggerProperties = await ProjectProperties.GetNodeWasmDebuggerPropertiesAsync();
            var debugAdapterExecutable = await debuggerProperties.NodeWasmDebuggerAdapterExecutable.GetEvaluatedValueAtEndAsync();

            var config = NodeWasmDebuggerConfig.GenerateNodeLaunchConfig(
                program: await debuggerProperties.NodeWasmDebuggerProgram.GetEvaluatedValueAtEndAsync(),
                nodeExecutable: await debuggerProperties.NodeWasmDebuggerNodeExecutable.GetEvaluatedValueAtEndAsync(),
                nodeWorkingDirectory: await debuggerProperties.NodeWasmDebuggerWorkingDirectory.GetEvaluatedValueAtEndAsync(),
                debugAdapterExecutable: debugAdapterExecutable
            );

            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;

            settings.Executable = @"WebAssembly Debugger";

            settings.LaunchDebugEngineGuid = new Guid("A18E581E-F120-4E9F-A0D4-D284EB773257");
            settings.Options = JsonConvert.SerializeObject(config);

            return new IDebugLaunchSettings[] { settings };
        }
    }
}
