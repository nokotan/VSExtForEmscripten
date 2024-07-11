#if VS2017
namespace Emscripten.DebuggerLauncher.vs2017
#else
namespace Emscripten.DebuggerLauncher
#endif
{
    using Microsoft.VisualStudio.Shell;
    using System.Runtime.InteropServices;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(PackageGuid)]
    public sealed class VsPackage : Package
    {
        /// <summary>
        /// The GUID for this package.
        /// </summary>
        public const string PackageGuid = "10240961-E5D1-4EAA-AB9C-5D85F6C07A39";

        /// <summary>
        /// The default namespace this project compiles with, so that manifest
        /// resource names can be calculated for embedded resources.
        /// </summary>
        internal const string DefaultNamespace = "Emscripten.DebuggerLauncher";
    }
}