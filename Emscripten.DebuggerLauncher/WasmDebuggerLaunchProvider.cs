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

namespace Emscripten.DebuggerLauncher
{
    [ExportDebugger(DebuggerSchemaName)]
    [AppliesTo(DebuggerSchemaName)]
    public class WasmDebuggerLaunchProvider : DebugLaunchProviderBase
    {
        internal const string DebuggerSchemaName = WasmDebugger.SchemaName;

        [ImportingConstructor]
        public WasmDebuggerLaunchProvider(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        [ExportPropertyXamlRuleDefinition("Emscripten.DebuggerLauncher, Version=1.0.0.0, Culture=neutral", "XamlRuleToCode:WasmDebugger.xaml", "Project")]
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
            var debuggerProperties = await ProjectProperties.GetWasmDebuggerPropertiesAsync();
            var debugAdapterExecutable = await debuggerProperties.WasmDebuggerAdapterExecutable.GetEvaluatedValueAtEndAsync();

            if (!File.Exists(debugAdapterExecutable))
            {
                throw new FileNotFoundException($@"Failed to launch WebAssembly debugger. Debugger adapter executable cannot be found. Please check WasmDebuggerAdapterExecutable in the Debugger configuration. InputValue: {debugAdapterExecutable}");
            }

            // 1: Debug Adapter Server Process
            var debugServerSettings = new DebugLaunchSettings(launchOptions);
            var debugServerConfig = WebAssemblyDebuggerConfig.GenerateChromeLaunchConfig(
                inspectedPage: await debuggerProperties.WasmDebuggerInspectedPage.GetEvaluatedValueAtEndAsync(),
                chromeFlags: await debuggerProperties.WasmDebuggerChromeFlags.GetEvaluatedValueAtEndAsync(),
                chromeUserDataDirectory: await debuggerProperties.WasmDebuggerChromeUserDataDirectory.GetEvaluatedValueAtEndAsync(),
                chromeIgnoreDefaultFlags: await debuggerProperties.WasmDebuggerChromeIgnoreDefaultFlags.GetEvaluatedValueAtEndAsync(),
                debugAdapterExecutable
            );

            debugServerSettings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            debugServerSettings.Executable = @"WebAssembly Debugger";
            debugServerSettings.LaunchDebugEngineGuid = new Guid("A18E581E-F120-4E9F-A0D4-D284EB773257");

            debugServerSettings.LaunchOptions |= DebugLaunchOptions.StopDebuggingOnEnd;
            debugServerSettings.Options = JsonConvert.SerializeObject(debugServerConfig);

            // 2: WebServer Process
            var webServerProcessSetting = new DebugLaunchSettings(launchOptions);
            var webServerProcess = new Process();

            webServerProcess.StartInfo.FileName = await debuggerProperties.WasmDebuggerServerExecutable.GetEvaluatedValueAtEndAsync();
            webServerProcess.StartInfo.Arguments = await debuggerProperties.WasmDebuggerServerArguments.GetEvaluatedValueAtEndAsync();
            webServerProcess.StartInfo.WorkingDirectory = await debuggerProperties.WasmDebuggerServerWorkingDirectory.GetEvaluatedValueAtEndAsync();
            webServerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            webServerProcess.Start();

            webServerProcessSetting.LaunchOperation = DebugLaunchOperation.AlreadyRunning;
            webServerProcessSetting.LaunchOptions = DebugLaunchOptions.CannotDebugAlone | DebugLaunchOptions.TerminateOnStop;
            webServerProcessSetting.Executable = webServerProcess.ProcessName;
            webServerProcessSetting.ProcessId = webServerProcess.Id;

            webServerProcessSetting.LaunchDebugEngineGuid = DebuggerEngines.NativeOnlyEngine;

            return new IDebugLaunchSettings[] { debugServerSettings, webServerProcessSetting };
        }
    }
}
