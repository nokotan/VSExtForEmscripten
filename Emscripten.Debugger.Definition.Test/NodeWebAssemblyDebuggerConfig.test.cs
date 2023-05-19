using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emscripten.Debugger.Definition;
using System;

namespace Emscripten.Debugger.Definition.Test
{
    [TestClass]
    public class NodeWebAssemblyDebuggerConfigTest
    {
        [TestMethod]
        public void GenerateNodeLaunchConfigFromEmptyStrings()
        {
            var config = NodeWebAssemblyDebuggerConfig.GenerateNodeLaunchConfig(
                program: "",
                nodeExecutable: "",
                nodeWorkingDirectory: "",
                debugAdapterExecutable: ""
            );

            Assert.AreEqual(config.type, "wasm-node");
            Assert.AreEqual(config.cwd, null);
        }

        [TestMethod]
        public void GenerateNodeLaunchConfigFromValidWorkingDirectory()
        {
            var config = NodeWebAssemblyDebuggerConfig.GenerateNodeLaunchConfig(
                program: "",
                nodeExecutable: "",
                nodeWorkingDirectory: "/foo/bar",
                debugAdapterExecutable: ""
            );

            Assert.AreEqual(config.cwd, "/foo/bar");
        }
    }
}
