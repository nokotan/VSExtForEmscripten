// ***********************************************************************************************
// (c) 2012 Gavin Pugh http://www.gavpugh.com/ - Released under the open-source zlib license
// ***********************************************************************************************

// Emscripten Linker task. Switches are data-driven via PropXmlParse.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

using Microsoft.Build.Framework;
using Microsoft.Build.CPPTasks;
using Microsoft.Build.Utilities;

namespace Emscripten.Build.CPPTasks
{
    public class EmscriptenLink : TrackedVCToolTask
    {
        private string m_toolFileName;
        private PropXmlParse m_propXmlParse;

        public bool BuildingInIDE { get; set; }

        [Required]
        public string EmscriptenToolPath { get; set; }

        [Required]
        public string PropertyXmlFile { get; set; }

        [Required]
        public string EchoCommandLines { get; set; }

        [Required]
        public virtual string OutputFile { get; set; }

        [Required]
        public virtual ITaskItem[] Sources { get; set; }


        public EmscriptenLink()
            : base(new ResourceManager("Emscripten.Build.CPPTasks.Properties.Resources", Assembly.GetExecutingAssembly()))
        {

        }

        protected override bool ValidateParameters()
        {
            m_propXmlParse = new PropXmlParse(PropertyXmlFile);

            m_toolFileName = Path.GetFileNameWithoutExtension(EmscriptenToolPath);

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

            foreach (ITaskItem sourceFile in Sources)
            {
                templateStr.Append(Utils.PathSanitize(sourceFile.GetMetadata("Identity")));
                templateStr.Append(" ");
            }

            templateStr.Append(m_propXmlParse.ProcessProperties(Sources[0]));

            return templateStr.ToString();
        }

        private void CleanUnusedTLogFiles()
        {
            try
            {
                // These tlog files are seemingly unused dep-wise, but cause problems when I add them to the proper TLog list
                // Incremental builds keep appending to them, so this keeps them from just growing and growing.
                string ignoreReadLogPath = Path.GetFullPath(TrackerLogDirectory + "\\" + m_toolFileName + ".read.1.tlog");
                string ignoreWriteLogPath = Path.GetFullPath(TrackerLogDirectory + "\\" + m_toolFileName + ".write.1.tlog");

                File.Delete(ignoreReadLogPath);
                File.Delete(ignoreWriteLogPath);
            }
            finally
            {

            }
        }

        protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            CleanUnusedTLogFiles();

            if (EchoCommandLines == "true")
            {
                Log.LogMessage(MessageImportance.High, pathToTool + " " + responseFileCommands);
            }

            return base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
        }

        // Called when linker outputs a line
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            base.LogEventsFromTextOutput(Utils.EmscriptenOutputReplace(singleLine), messageImportance);
        }

        protected override void PostProcessSwitchList()
        {

        }

        public override bool AttributeFileTracking
        {
            get
            {
                return true;
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
            set
            {
            }
        }

        protected override Encoding ResponseFileEncoding
        {
            get
            {
                return Encoding.ASCII;
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
                return this.Sources;
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
                return m_toolFileName + "-link.command.1.tlog";
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
