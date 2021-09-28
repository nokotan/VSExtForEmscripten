namespace Emscripten.Debugger.Definition
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
        public const string PackageGuid = "5A0257BF-5286-4FBD-99BB-10237E2F2DBE";

        /// <summary>
        /// The default namespace this project compiles with, so that manifest
        /// resource names can be calculated for embedded resources.
        /// </summary>
        internal const string DefaultNamespace = "Emscripten.ProjectType";
    }
}