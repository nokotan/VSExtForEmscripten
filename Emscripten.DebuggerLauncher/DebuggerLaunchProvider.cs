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

            // 1: Debug Adapter Server Process
            var serverSettings = new DebugLaunchSettings(launchOptions);
            serverSettings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            serverSettings.LaunchOptions = DebugLaunchOptions.CannotDebugAlone | DebugLaunchOptions.TerminateOnStop;
            serverSettings.Executable = @"WebAssembly Debug Server";
            serverSettings.LaunchDebugEngineGuid = new Guid("9849C080-ECCF-46EE-9758-9F6F9ED68693");
            serverSettings.Options = "{}";

            // 2: Debug Adapter Client Process
            var clientSettings = new DebugLaunchSettings(launchOptions);
            var clientConfig = WebAssemblyDebuggerConfig.GenerateChromeLaunchConfig(
                inspectedPage: await debuggerProperties.WasmDebuggerInspectedPage.GetEvaluatedValueAtEndAsync(),
                chromeFlags: await debuggerProperties.WasmDebuggerChromeFlags.GetEvaluatedValueAtEndAsync(),
                chromeUserDataDirectory: await debuggerProperties.WasmDebuggerChromeUserDataDirectory.GetEvaluatedValueAtEndAsync(),
                chromeIgnoreDefaultFlags: await debuggerProperties.WasmDebuggerChromeIgnoreDefaultFlags.GetEvaluatedValueAtEndAsync(),
                debugAdapterExecutable: null
            );

            clientSettings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            clientSettings.Executable = @"WebAssembly Debug Client";
            clientSettings.LaunchDebugEngineGuid = new Guid("9849C080-ECCF-46EE-9758-9F6F9ED68693");

            clientConfig.type = "pwa-chrome";
            clientConfig.enableDWARF = true;
            clientConfig.adapterExecutable = null;
            clientConfig.port = 8123;

            clientSettings.LaunchOptions |= DebugLaunchOptions.StopDebuggingOnEnd;
            clientSettings.Options = JsonConvert.SerializeObject(clientConfig);

            // 3: WebServer Process
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

            return new IDebugLaunchSettings[] { serverSettings, clientSettings, webServerProcessSetting };
        }
    }
}
