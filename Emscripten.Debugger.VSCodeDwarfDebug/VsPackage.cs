namespace Kamenokosoft.Emscripten.Debugger.VSCodeDwarfDebug
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
        public const string PackageGuid = "63C31418-1D96-4EFF-A3D4-5839A01EA1C9";

        /// <summary>
        /// The default namespace this project compiles with, so that manifest
        /// resource names can be calculated for embedded resources.
        /// </summary>
        internal const string DefaultNamespace = "Emscripten.Debugger.VSCodeDwarfDebug";
    }
}