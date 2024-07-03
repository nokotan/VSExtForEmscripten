using Microsoft.VisualStudio.Debugger.DebugAdapterHost.Interfaces;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using System;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace Kamenokosoft.ChromeCXXDebugger
{


    public class StartDebuggingRequestHandler : ICustomProtocolExtension
    {
        private IDebugAdapterHostContext context;

        private IProtocolHostOperations host;

        public void Initialize(IDebugAdapterHostContext context)
        {
            // Save the context object provided by the Debug Adapter Host so we can use it to access services later
            this.context = context;
        }

        public void RegisterCustomMessages(ICustomMessageRegistry registry, IProtocolHostOperations hostOperations)
        {
            host = hostOperations;

            // Register our custom request
            registry.RegisterClientRequestType<StartDebuggingRequest, StartDebuggingArgs, StartDebuggingResponse>(this.OnStartDebuggingRequest);
            
            // Advertise support for the custom request
            registry.SetInitializeRequestProperty("supportsStartDebuggingRequest", true);
        }

        #region StartDebugging

        private void OnStartDebuggingRequest(IRequestResponder<StartDebuggingArgs, StartDebuggingResponse> responder)
        {
            var settings = new DebugLaunchSettings(DebugLaunchOptions.DetachOnStop);

            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            settings.LaunchDebugEngineGuid = new Guid("9849C080-ECCF-46EE-9758-9F6F9ED68693");
            settings.Executable = "WebAssembly DWARF Debug";
            responder.Arguments.Config.ConfigurationProperties["$debugServer"] = 8123;
            settings.Options = JsonConvert.SerializeObject(responder.Arguments.Config);

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await DebugEngineLauncher.LaunchAsync(settings);
            }).Task.Forget();

            responder.SetResponse(new StartDebuggingResponse());
        }

        internal class ConfigurationProperty
        {
            [JsonExtensionData(ReadData = true, WriteData = true)]
            public JObject ConfigurationProperties { get; set; }
        }

        internal class StartDebuggingArgs
        {
            [JsonProperty("request")]
            public string Request { get; set; }

            [JsonProperty("configuration")]
            public ConfigurationProperty Config { get; set; }
        }

        internal class StartDebuggingResponse : ResponseBody
        {
        }

        internal class StartDebuggingRequest : DebugClientRequestWithResponse<StartDebuggingArgs, StartDebuggingResponse>
        {
            public StartDebuggingRequest() : base("startDebugging")
            {
            }
        }

        #endregion
    }
}
