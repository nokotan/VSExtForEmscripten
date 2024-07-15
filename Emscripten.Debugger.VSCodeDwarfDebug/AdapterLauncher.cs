// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using StreamJsonRpc;

namespace Kamenokosoft.Emscripten.Debugger.VSCodeDwarfDebug
{
    internal sealed class DebugEngineLauncher
    {
        static public async Task LaunchAsync(DebugLaunchSettings setting)
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
            var _unused = new VsDebugTargetProcessInfo[1];

            obj.LaunchDebugTargets4(1, new VsDebugTargetInfo4[1] { vsDebugTargetInfo }, _unused);
        }
    }
}