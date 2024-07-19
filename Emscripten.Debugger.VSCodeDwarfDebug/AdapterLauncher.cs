using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emscripten.DebuggerLauncher;
using Microsoft.VisualStudio.Debugger.DebugAdapterHost.Interfaces;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using static System.Windows.Forms.Design.AxImporter;

namespace Kamenokosoft.Emscripten.Debugger.VSCodeDwarfDebug
{
    public class AdapterLauncher : IAdapterLauncher, IDebugAdapterHostComponent
    {
        private IDebugAdapterHostContext context;

        private Process debugServerProcess;

        public void Initialize(IDebugAdapterHostContext context)
        {
            // Save the context object provided by the Debug Adapter Host so we can use it to access services later
            this.context = context;

            context.Events.DebuggingEnded += OnDebugSessionEnded;
        }

        public void UpdateLaunchOptions(IAdapterLaunchInfo launchInfo)      
        {
            if (launchInfo.LaunchJson.Contains("__pendingTarget"))
            {
                // child target. ignore
            } 
            else 
            {
                LaunchDebugServer(launchInfo);
            }
        }

        private void OnDebugSessionEnded(object sender, EventArgs e)
        {
            if (debugServerProcess != null && !debugServerProcess.HasExited)
            {
                debugServerProcess.Kill();
            }
        }

        private void LaunchDebugServer(IAdapterLaunchInfo launchInfo)
        {
            var AdapterExecutable = launchInfo.GetMetricString("Adapter");
            var AdapterArguments = launchInfo.GetMetricString("AdapterArgs");

            var AdapterServerProcess = new Process();

            AdapterServerProcess.StartInfo.FileName = AdapterExecutable;
            AdapterServerProcess.StartInfo.Arguments = AdapterArguments;
            AdapterServerProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // AdapterServerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            AdapterServerProcess.Start();

            debugServerProcess = AdapterServerProcess;
        }

        public ITargetHostProcess LaunchAdapter(IAdapterLaunchInfo launchInfo, ITargetHostInterop targetInterop)
        {
            // nop
            return null;
        } 
    }
}
