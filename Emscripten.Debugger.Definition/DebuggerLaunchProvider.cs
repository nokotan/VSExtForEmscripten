using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;

#if VS2017
namespace Emscripten.Debugger.Definition.vs2017
#else
namespace Emscripten.Debugger.Definition
#endif
{
    [ExportDebugger(DebuggerSchemaName)]
    [AppliesTo(DebuggerSchemaName)]
    public class DebuggerLaunchProvider : DebugLaunchProviderBase
    {
#if VS2017
        internal const string DebuggerSchemaName = WasmDebuggerVS2017.SchemaName;
#else
        internal const string DebuggerSchemaName = WasmDebugger.SchemaName;
#endif

        [ImportingConstructor]
        public DebuggerLaunchProvider(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        [ExportPropertyXamlRuleDefinition("Emscripten.Debugger.Definition, Version=1.0.0.0, Culture=neutral", "XamlRuleToCode:WasmDebugger.xaml", "Project")]
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
#if VS2017
            var debuggerProperties = await ProjectProperties.GetWasmDebuggerVS2017PropertiesAsync();
#else
            var debuggerProperties = await ProjectProperties.GetWasmDebuggerPropertiesAsync();
#endif
            var inspectedPage = await debuggerProperties.WasmDebuggerInspectedPage.GetEvaluatedValueAtEndAsync();
            var debugAdapterExecutable = await debuggerProperties.WasmDebuggerAdapterExecutable.GetEvaluatedValueAtEndAsync();

            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;

            settings.Executable = @"C:\Windows\System32\cmd.exe"; // dummy

            settings.LaunchDebugEngineGuid = new Guid("A18E581E-F120-4E9F-A0D4-D284EB773257");
            settings.Options = $@"{{ ""type"": ""wasm-chrome"", ""url"": ""{inspectedPage}"", ""$adapter"":""{debugAdapterExecutable.Replace('\\', '/')}"" }}";

            var serverProcessSetting = new DebugLaunchSettings(launchOptions);

            serverProcessSetting.LaunchOperation = DebugLaunchOperation.CreateProcess;

            serverProcessSetting.Executable = await debuggerProperties.WasmDebuggerServerExecutable.GetEvaluatedValueAtEndAsync();
            serverProcessSetting.Arguments = await debuggerProperties.WasmDebuggerServerArguments.GetEvaluatedValueAtEndAsync();
            serverProcessSetting.CurrentDirectory = await debuggerProperties.WasmDebuggerServerWorkingDirectory.GetEvaluatedValueAtEndAsync();

            serverProcessSetting.LaunchDebugEngineGuid = DebuggerEngines.NativeOnlyEngine;

            return new IDebugLaunchSettings[] { settings, serverProcessSetting };
        }
    }
}
