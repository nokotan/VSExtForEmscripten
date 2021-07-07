// ***********************************************************************************************
// (c) 2012 Gavin Pugh http://www.gavpugh.com/ - Released under the open-source zlib license
// ***********************************************************************************************

// Emscripten Static library Building task. No supported switches currently.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Diagnostics;

using Microsoft.Build.Framework;
using Microsoft.Build.CPPTasks;
using Microsoft.Build.Utilities;

namespace Emscripten.Build.CPPTasks
{
    public class EmscriptenLib : TrackedVCToolTask
    {
        private string m_toolFileName;

        public bool BuildingInIDE { get; set; }

        [Required]
        public string EmscriptenToolPath { get; set; }

        [Required]
        public string PropertyXmlFile { get; set; }

        [Required]
        public virtual string OutputFile { get; set; }

        [Required]
        public string EchoCommandLines { get; set; }

        [Required]
        public virtual ITaskItem[] Sources { get; set; }


        public EmscriptenLib()
            : base(new ResourceManager("Emscripten.Build.CPPTasks.Properties.Resources", Assembly.GetExecutingAssembly()))
        {

        }

        protected override bool ValidateParameters()
        {
            m_toolFileName = Path.GetFileNameWithoutExtension(ToolName);

            return base.ValidateParameters();
        }

#if !VS2010DLL
        protected override string GenerateResponseFileCommands(CommandLineFormat format, EscapeFormat escapeFormat)
        {
            return GenerateResponseFileCommands();
        }
#endif

        protected override string GenerateResponseFileCommands()
        {
            StringBuilder templateStr = new StringBuilder(Utils.EST_MAX_CMDLINE_LEN);
            templateStr.Append("rcs " + Utils.PathSanitize(OutputFile) + " ");

            foreach (ITaskItem sourceFile in Sources)
            {
                templateStr.Append(Utils.PathSanitize(sourceFile.GetMetadata("Identity")));
                templateStr.Append(" ");
            }

            return templateStr.ToString();
        }

        protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            if (EchoCommandLines == "true")
            {
                Log.LogMessage(MessageImportance.High, pathToTool + " " + responseFileCommands);
            }

            return base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
        }

        protected override void RemoveTaskSpecificOutputs(CanonicalTrackedOutputFiles compactOutputs)
        {
            // Incremental builds, for whatever reason leave the .a lib output file out of this file.
            // This clears the list (which either is empty, or already has it), and puts it back in.
            
            foreach (KeyValuePair<string, Dictionary<string, DateTime>> pair in compactOutputs.DependencyTable)
            {
                pair.Value.Clear();
                pair.Value.Add(Path.GetFullPath(OutputFile).ToUpperInvariant(), DateTime.Now);
            }
        }

        protected override bool MaintainCompositeRootingMarkers
        {
            get
            {
                return true;
            }
        }

        public virtual string PlatformToolset
        {
            get
            {
                return "emcc";
            }
        }

        protected override Encoding ResponseFileEncoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        protected override string ToolName
        {
            get
            {
                return EmscriptenToolPath;
            }
        }

        protected override ITaskItem[] TrackedInputFiles
        {
            get
            {
                return Sources;
            }
        }

        protected override string TrackerIntermediateDirectory
        {
            get
            {
                if (this.TrackerLogDirectory != null)
                {
                    return this.TrackerLogDirectory;
                }
                return string.Empty;
            }
        }

        public virtual string TrackerLogDirectory
        {
            get
            {
                if (base.IsPropertySet("TrackerLogDirectory"))
                {
                    return base.ActiveToolSwitches["TrackerLogDirectory"].Value;
                }
                return null;
            }
            set
            {
                base.ActiveToolSwitches.Remove("TrackerLogDirectory");
                ToolSwitch switch2 = new ToolSwitch(ToolSwitchType.Directory)
                {
                    DisplayName = "Tracker Log Directory",
                    Description = "Tracker log directory.",
                    ArgumentRelationList = new ArrayList(),
                    Value = VCToolTask.EnsureTrailingSlash(value)
                };
                base.ActiveToolSwitches.Add("TrackerLogDirectory", switch2);
                base.AddActiveSwitchToolValue(switch2);
            }
        }

        protected override string CommandTLogName
        {
            get
            {
                return (m_toolFileName + "-lib.command.1.tlog");
            }
        }

        protected override string[] ReadTLogNames
        {
            get
            {
                return new string[] {
                    "cmd-python.read.1.tlog"
                };
            }
        }

        protected override string[] WriteTLogNames
        {
            get
            {
                return new string[] {
                    "cmd-python.write.1.tlog"
                };
            }
        }
    }
}
