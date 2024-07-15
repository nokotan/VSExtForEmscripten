using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#if VS2017
namespace Emscripten.Debugger.VSCodeDwarfDebug.Definition.vs2017
#else
namespace Emscripten.Debugger.VSCodeDwarfDebug.Definition
#endif
{
    [ExportDebugger(DebuggerSchemaName)]
    [AppliesTo(DebuggerSchemaName)]
    public class DebuggerLaunchProvider : DebugLaunchProviderBase
    {
#if VS2017
        internal const string DebuggerSchemaName = WasmDebuggerVS2017.SchemaName;
#else
        internal const string DebuggerSchemaName = Emscripten.Debugger.VSCodeDwarfDebug.SchemaName;
#endif

        internal static DebuggerLaunchProvider Instance;

        [ImportingConstructor]
        public DebuggerLaunchProvider(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
            Instance = this;
        }

        [ExportPropertyXamlRuleDefinition("Emscripten.Debugger.VSCodeDwarfDebug.Definition, Version=1.0.0.0, Culture=neutral", "XamlRuleToCode:Emscripten.Debugger.VSCodeDwarfDebug.xaml", "Project")]
        [AppliesTo(DebuggerSchemaName)]
        private object DebuggerXaml { get { throw new NotImplementedException(); } }

        [Import]
        private ProjectProperties ProjectProperties { get; set; }

        public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
        {
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<LaunchResult>> Launch(DebugLaunchSettings settings)
        {
            return LaunchAsync(settings);
        }

        public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
        {
            var settings = new DebugLaunchSettings(launchOptions);
#if VS2017
            var debuggerProperties = await ProjectProperties.GetWasmDebuggerVS2017PropertiesAsync();
#else
            var debuggerProperties = await ProjectProperties.GetEmscripten.Debugger.VSCodeDwarfDebugPropertiesAsync();
#endif
            var debugAdapterExecutable = await debuggerProperties.WasmDebuggerAdapterExecutable.GetEvaluatedValueAtEndAsync();

            if (!File.Exists(debugAdapterExecutable))
            {
                throw new FileNotFoundException($@"Failed to launch WebAssembly debugger. Debugger adapter executable cannot be found. Please check WasmDebuggerAdapterExecutable in the Debugger configuration. InputValue: {debugAdapterExecutable}");
            }

            var config = WebAssemblyDebuggerConfig.GenerateChromeLaunchConfig(
                inspectedPage: await debuggerProperties.WasmDebuggerInspectedPage.GetEvaluatedValueAtEndAsync(),
                webRoot: await debuggerProperties.WasmDebuggerServerWorkingDirectory.GetEvaluatedValueAtEndAsync(),
                debugAdapterExecutable: debugAdapterExecutable
            );

            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            settings.LaunchOptions |= DebugLaunchOptions.StopDebuggingOnEnd;

            settings.Executable = debugAdapterExecutable; // dummy

            settings.LaunchDebugEngineGuid = new Guid("9849C080-ECCF-46EE-9758-9F6F9ED68693");
            settings.Options = JsonSerializer.Serialize(config);

            var serverProcess = new Process();

            serverProcess.StartInfo.FileName = await debuggerProperties.WasmDebuggerServerExecutable.GetEvaluatedValueAtEndAsync();
            serverProcess.StartInfo.Arguments = await debuggerProperties.WasmDebuggerServerArguments.GetEvaluatedValueAtEndAsync();
            serverProcess.StartInfo.WorkingDirectory = await debuggerProperties.WasmDebuggerServerWorkingDirectory.GetEvaluatedValueAtEndAsync();
            serverProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            serverProcess.Start();

            var serverProcessSetting = new DebugLaunchSettings(launchOptions);

            serverProcessSetting.LaunchOperation = DebugLaunchOperation.AlreadyRunning;
            serverProcessSetting.LaunchOptions = DebugLaunchOptions.CannotDebugAlone | DebugLaunchOptions.TerminateOnStop;
            serverProcessSetting.Executable = serverProcess.StartInfo.FileName;
            serverProcessSetting.ProcessId = serverProcess.Id;

            return new IDebugLaunchSettings[] { settings, serverProcessSetting };
        }
    }
}
