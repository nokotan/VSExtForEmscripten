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
    public class VSCodeDebuggerLaunchProvider : DebugLaunchProviderBase
    {
        internal const string DebuggerSchemaName = VSCodeDebugger.SchemaName;

        internal static VSCodeDebuggerLaunchProvider Instance;

        [ImportingConstructor]
        public VSCodeDebuggerLaunchProvider(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
            Instance = this;
        }

        [ExportPropertyXamlRuleDefinition("Emscripten.DebuggerLauncher, Version=1.0.0.0, Culture=neutral", "XamlRuleToCode:Emscripten.DebuggerLauncher.VSCodeDwarfDebug.xaml", "Project")]
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
            var debuggerProperties = await ProjectProperties.GetVSCodeDebuggerPropertiesAsync();

            // 1: Debug Adapter Server Process
            var serverSettings = new DebugLaunchSettings(launchOptions);
            serverSettings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            serverSettings.LaunchOptions = DebugLaunchOptions.CannotDebugAlone | DebugLaunchOptions.TerminateOnStop;
            serverSettings.Executable = @"WebAssembly Debug Server";
            serverSettings.LaunchDebugEngineGuid = new Guid("9849C080-ECCF-46EE-9758-9F6F9ED68693");
            serverSettings.Options = "{}";

            // 2: Debug Adapter Client Process
            var clientSettings = new DebugLaunchSettings(launchOptions);
            var clientConfig = VSCodeDebuggerConfig.GenerateChromeLaunchConfig(
                inspectedPage: await debuggerProperties.VSCodeDebuggerInspectedPage.GetEvaluatedValueAtEndAsync()
            );

            clientSettings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            clientSettings.Executable = @"WebAssembly Debug Client";
            clientSettings.LaunchDebugEngineGuid = new Guid("9849C080-ECCF-46EE-9758-9F6F9ED68693");

            clientConfig.type = "pwa-chrome";
            clientConfig.enableDWARF = true;
            clientConfig.port = 8123;

            clientSettings.LaunchOptions |= DebugLaunchOptions.StopDebuggingOnEnd;
            clientSettings.Options = JsonConvert.SerializeObject(clientConfig);

            // 3: WebServer Process
            var webServerProcessSetting = new DebugLaunchSettings(launchOptions);
            var webServerProcess = new Process();

            webServerProcess.StartInfo.FileName = await debuggerProperties.VSCodeDebuggerServerExecutable.GetEvaluatedValueAtEndAsync();
            webServerProcess.StartInfo.Arguments = await debuggerProperties.VSCodeDebuggerServerArguments.GetEvaluatedValueAtEndAsync();
            webServerProcess.StartInfo.WorkingDirectory = await debuggerProperties.VSCodeDebuggerServerWorkingDirectory.GetEvaluatedValueAtEndAsync();
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
