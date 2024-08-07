﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emscripten.DebuggerLauncher;
using System;

namespace Emscripten.DebuggerLauncher.Test
{
    [TestClass]
    public class WebAssemblyDebuggerConfigTest
    {
        [TestMethod]
        public void GenerateChromeLaunchConfigFromEmptyStrings()
        {
            var config = WasmDebuggerConfig.GenerateChromeLaunchConfig(
                inspectedPage: "",
                chromeFlags: "",
                chromeUserDataDirectory: "",
                chromeIgnoreDefaultFlags: "",
                debugAdapterExecutable: ""
            );

            Assert.AreEqual(config.type, "wasm-chrome");
            Assert.AreEqual(config.flags.Length, 0);
            Assert.AreEqual(config.userDataDir, null);
            Assert.AreEqual(config.ignoreDefaultFlags, null);
        }

        [TestMethod]
        public void GenerateChromeLaunchConfigFromValidFlags()
        {
            var config = WasmDebuggerConfig.GenerateChromeLaunchConfig(
                inspectedPage: "",
                chromeFlags: "--isolated;--mute;",
                chromeUserDataDirectory: "",
                chromeIgnoreDefaultFlags: "",
                debugAdapterExecutable: ""
            );

            Assert.AreEqual(config.flags.Length, 2);
            Assert.AreEqual(config.flags[0], "--isolated");
            Assert.AreEqual(config.flags[1], "--mute");
        }

        [TestMethod]
        public void GenerateChromeLaunchConfigFromValidUserDataDirectory()
        {
            var config = WasmDebuggerConfig.GenerateChromeLaunchConfig(
                inspectedPage: "",
                chromeFlags: "",
                chromeUserDataDirectory: "/foo/bar",
                chromeIgnoreDefaultFlags: "",
                debugAdapterExecutable: ""
            );

            Assert.AreEqual(config.userDataDir, "/foo/bar");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void GenerateChromeLaunchConfigFromValidChromeIgnoreDefaultFlags()
        {
            var _config = WasmDebuggerConfig.GenerateChromeLaunchConfig(
                inspectedPage: "",
                chromeFlags: "",
                chromeUserDataDirectory: "",
                chromeIgnoreDefaultFlags: "yo",
                debugAdapterExecutable: ""
            );
        }
    }
}
