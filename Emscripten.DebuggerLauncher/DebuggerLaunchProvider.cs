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
            var settings = new DebugLaunchSettings(launchOptions);
#if VS2017
            var debuggerProperties = await ProjectProperties.GetWasmDebuggerVS2017PropertiesAsync();
#else
            var debuggerProperties = await ProjectProperties.GetWasmDebuggerPropertiesAsync();
#endif
            var debugAdapterExecutable = await debuggerProperties.WasmDebuggerAdapterExecutable.GetEvaluatedValueAtEndAsync();

            if (!File.Exists(debugAdapterExecutable))
            {
                throw new FileNotFoundException($@"Failed to launch WebAssembly debugger. Debugger adapter executable cannot be found. Please check WasmDebuggerAdapterExecutable in the Debugger configuration. InputValue: {debugAdapterExecutable}");
            }

            var config = WebAssemblyDebuggerConfig.GenerateChromeLaunchConfig(
                inspectedPage: await debuggerProperties.WasmDebuggerInspectedPage.GetEvaluatedValueAtEndAsync(),
                chromeFlags: await debuggerProperties.WasmDebuggerChromeFlags.GetEvaluatedValueAtEndAsync(),
                chromeUserDataDirectory: await debuggerProperties.WasmDebuggerChromeUserDataDirectory.GetEvaluatedValueAtEndAsync(),
                chromeIgnoreDefaultFlags: await debuggerProperties.WasmDebuggerChromeIgnoreDefaultFlags.GetEvaluatedValueAtEndAsync(),
                debugAdapterExecutable: debugAdapterExecutable
            );

            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;

            settings.Executable = @"C:\Windows\System32\cmd.exe"; // dummy

            settings.LaunchDebugEngineGuid = new Guid("A18E581E-F120-4E9F-A0D4-D284EB773257");
            settings.Options = JsonConvert.SerializeObject(config);

            var serverProcess = new Process();

            serverProcess.StartInfo.FileName = await debuggerProperties.WasmDebuggerServerExecutable.GetEvaluatedValueAtEndAsync();
            serverProcess.StartInfo.Arguments = await debuggerProperties.WasmDebuggerServerArguments.GetEvaluatedValueAtEndAsync();
            serverProcess.StartInfo.WorkingDirectory = await debuggerProperties.WasmDebuggerServerWorkingDirectory.GetEvaluatedValueAtEndAsync();
            serverProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            serverProcess.Start();

            var serverProcessSetting = new DebugLaunchSettings(launchOptions);

            serverProcessSetting.LaunchOperation = DebugLaunchOperation.AlreadyRunning;
            serverProcessSetting.LaunchOptions |= DebugLaunchOptions.CannotDebugAlone;
            serverProcessSetting.Executable = serverProcess.StartInfo.FileName;
            serverProcessSetting.ProcessId = serverProcess.Id;

            serverProcessSetting.LaunchDebugEngineGuid = DebuggerEngines.NativeOnlyEngine;

            return new IDebugLaunchSettings[] { settings, serverProcessSetting };
        }
    }
}
