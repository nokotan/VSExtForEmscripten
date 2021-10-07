using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;

namespace Emscripten.Debugger.Definition
{
    [ExportDebugger(WasmDebugger.SchemaName)]
    [AppliesTo(WasmDebugger.SchemaName)]
    public class DebuggerLaunchProvider : DebugLaunchProviderBase
    {
        [ImportingConstructor]
        public DebuggerLaunchProvider(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        [ExportPropertyXamlRuleDefinition("Emscripten.Debugger.Definition, Version=1.0.0.0, Culture=neutral", "XamlRuleToCode:WasmDebugger.xaml", "Project")]
        [AppliesTo(WasmDebugger.SchemaName)]
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
            var debuggerProperties = await ProjectProperties.GetWasmDebuggerPropertiesAsync();
            var inspectedPage = await debuggerProperties.WasmDebuggerInspectedPage.GetEvaluatedValueAtEndAsync();
            var debugAdapterExecutable = await debuggerProperties.WasmDebuggerAdapterExecutable.GetEvaluatedValueAtEndAsync();

            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;

            settings.Executable = @"C:\Windows\System32\cmd.exe"; // dummy

            settings.LaunchDebugEngineGuid = new Guid("A18E581E-F120-4E9F-A0D4-D284EB773257");
            settings.Options = $@"{{ ""type"": ""wasm"", ""url"": ""{inspectedPage}"", ""$adapter"":{debugAdapterExecutable} }}";

            var serverProcessSetting = new DebugLaunchSettings(launchOptions);

            serverProcessSetting.LaunchOperation = DebugLaunchOperation.CreateProcess;

            serverProcessSetting.Executable = await debuggerProperties.WasmDebuggerServerExecutable.GetEvaluatedValueAtEndAsync();
            serverProcessSetting.CurrentDirectory = await debuggerProperties.WasmDebuggerServerArguments.GetEvaluatedValueAtEndAsync();
            serverProcessSetting.Arguments = await debuggerProperties.WasmDebuggerServerWorkingDirectory.GetEvaluatedValueAtEndAsync();

            serverProcessSetting.LaunchDebugEngineGuid = DebuggerEngines.NativeOnlyEngine;

            return new IDebugLaunchSettings[] { settings, serverProcessSetting };
        }
    }
}