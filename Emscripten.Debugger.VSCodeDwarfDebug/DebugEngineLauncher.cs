// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Microsoft.VisualStudio.Debugger.DebugAdapterHost.Interfaces;
using System.IO;
using System.Diagnostics;

namespace Kamenokosoft.Emscripten.Debugger.VSCodeDwarfDebug
{
    internal class LaunchedTarget : ITargetHostProcess, IDisposable
    {
        private Process target;

        public IntPtr Handle => target.Handle;

        public Stream StandardInput => target.StandardInput.BaseStream;

        public Stream StandardOutput => target.StandardOutput.BaseStream;

        public bool HasExited => target.HasExited;

        public event EventHandler Exited;
        public event DataReceivedEventHandler ErrorDataReceived;

        public LaunchedTarget(Process process)
        { 
            target = process;

            target.Exited += OnExited;
            target.ErrorDataReceived += OnErrorDataReceived;
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            target.Exited -= OnExited;
            target.ErrorDataReceived -= OnErrorDataReceived;

            ((IDisposable)target).Dispose();
        }

        private void OnExited(object sender, EventArgs e)
        {
            Exited(sender, e);
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            ErrorDataReceived(sender, e);
        }
    }

    internal sealed class DebugEngineLauncher
    {
        static public async System.Threading.Tasks.Task<ITargetHostProcess> LaunchAsync(DebugLaunchSettings setting)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            var obj = Package.GetGlobalService(typeof(SVsShellDebugger)) as IVsDebugger4;
            var vsDebugTargetInfo = new VsDebugTargetInfo4
            {
                LaunchFlags = (uint)setting.LaunchOptions,
                dlo = (uint)setting.LaunchOperation,
                guidLaunchDebugEngine = setting.LaunchDebugEngineGuid,
                bstrExe = setting.Executable,
                bstrOptions = setting.Options
            };
            var launchedProcess = new VsDebugTargetProcessInfo[1];

            obj.LaunchDebugTargets4(1, new VsDebugTargetInfo4[1] { vsDebugTargetInfo }, launchedProcess);

            return new LaunchedTarget(Process.GetProcessById((int)launchedProcess[0].dwProcessId));
        }
    }
}