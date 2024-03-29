﻿// ***********************************************************************************************
// (c) 2012 Gavin Pugh http://www.gavpugh.com/ - Released under the open-source zlib license
// -----------------------------------------------------------------------------------------------
// This file is modified by [kamenokonokotan](https://github.com/nokotan) and redistributed under
// the MIT License. See LICENSE in the project root for license information.
// ***********************************************************************************************

// This file is just a copy of the .NET Reflector code of 'TrackedVCToolTask' from 'Microsoft.Build.CPPTasks.Common'.
// Unfortunately, I need to Skip the ExecuteTool() virtual function it overrides. This was about the most straightforward
// way I thought of to do it.
// An alternative way would have been to reimplement what ToolTask.ExecuteTool() does, but it does operate on
// private members. So it'd mean more code to be maintained. All this file is, is a straight copy with some
// functions commented out.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Security;
using System.Diagnostics;
using System.Threading;

using Microsoft.Win32.SafeHandles;
using Microsoft.Build.Framework;
using Microsoft.Build.CPPTasks;
using Microsoft.Build.Utilities;
using Microsoft.Build.Shared;


namespace Emscripten.Build.CPPTasks
{
    public abstract class CustomTrackedVCToolTask : VCToolTask
    {
        // Fields
        private ITaskItem[] excludedInputPaths;
        private bool minimalRebuildFromTracking;
        private string pathOverride;
        private string pathToLog;
        private string rootSource;
        private bool skippedExecution;
        private CanonicalTrackedInputFiles sourceDependencies;
        private ITaskItem[] sourcesCompiled;
        private ITaskItem tlogCommandFile;
        private ITaskItem[] tlogReadFiles;
        private ITaskItem[] tlogWriteFiles;
        private ITaskItem[] trackedInputFilesToIgnore;
        private ITaskItem[] trackedOutputFilesToIgnore;
        private bool trackFileAccess;
        //		private AutoResetEvent unicodeOutputEnded;
        //		private SafeFileHandle unicodePipeReadHandle;
        //		private SafeFileHandle unicodePipeWriteHandle;

        // Methods
        protected CustomTrackedVCToolTask(ResourceManager taskResources)
            : base(taskResources)
        {
            this.pathToLog = string.Empty;
        }

        protected virtual void AddTaskSpecificOutputs(ITaskItem[] sources, CanonicalTrackedOutputFiles compactOutputs)
        {
        }

        public virtual string ApplyPrecompareCommandFilter(string cmdString)
        {
            return cmdString;
        }

        protected virtual void AssignDefaultTLogPaths()
        {
            if (this.TLogReadFiles == null)
            {
                this.TLogReadFiles = new ITaskItem[this.ReadTLogNames.Length];
                for (int i = 0; i < this.ReadTLogNames.Length; i++)
                {
                    this.TLogReadFiles[i] = new TaskItem(Path.Combine(this.TrackerIntermediateDirectory, this.ReadTLogNames[i]));
                }
            }
            if (this.TLogWriteFiles == null)
            {
                this.TLogWriteFiles = new ITaskItem[this.WriteTLogNames.Length];
                for (int j = 0; j < this.WriteTLogNames.Length; j++)
                {
                    this.TLogWriteFiles[j] = new TaskItem(Path.Combine(this.TrackerIntermediateDirectory, this.WriteTLogNames[j]));
                }
            }
            if (this.TLogCommandFile == null)
            {
                this.TLogCommandFile = new TaskItem(Path.Combine(this.TrackerIntermediateDirectory, this.CommandTLogName));
            }
        }

        protected virtual ITaskItem[] AssignOutOfDateSources(ITaskItem[] sources)
        {
            return sources;
        }

        protected /*internal*/ virtual bool ComputeOutOfDateSources()
        {
            if (this.TrackerIntermediateDirectory != null)
            {
                string trackerIntermediateDirectory = this.TrackerIntermediateDirectory;
            }
            if (this.MinimalRebuildFromTracking || this.TrackFileAccess)
            {
                this.AssignDefaultTLogPaths();
            }
            if (this.MinimalRebuildFromTracking && !this.ForcedRebuildRequired())
            {
                CanonicalTrackedOutputFiles outputs = new CanonicalTrackedOutputFiles(this, this.TLogWriteFiles);
                this.sourceDependencies = new CanonicalTrackedInputFiles(this, this.TLogReadFiles, this.TrackedInputFiles, this.ExcludedInputPaths, outputs, this.UseMinimalRebuildOptimization, this.MaintainCompositeRootingMarkers);
                ITaskItem[] sourcesOutOfDateThroughTracking = this.SourceDependencies.ComputeSourcesNeedingCompilation(false);
                List<ITaskItem> sourcesWithChangedCommandLines = this.GenerateSourcesOutOfDateDueToCommandLine();
                this.SourcesCompiled = this.MergeOutOfDateSourceLists(sourcesOutOfDateThroughTracking, sourcesWithChangedCommandLines);
                if (this.SourcesCompiled.Length == 0)
                {
                    this.SkippedExecution = true;
                    return this.SkippedExecution;
                }
                this.SourcesCompiled = this.AssignOutOfDateSources(this.SourcesCompiled);
                this.SourceDependencies.RemoveEntriesForSource(this.SourcesCompiled);
                this.SourceDependencies.SaveTlog();
                outputs.RemoveEntriesForSource(this.SourcesCompiled);
                outputs.SaveTlog();
            }
            else
            {
                this.SourcesCompiled = this.TrackedInputFiles;
                if ((this.SourcesCompiled == null) || (this.SourcesCompiled.Length == 0))
                {
                    this.SkippedExecution = true;
                    return this.SkippedExecution;
                }
            }
            if (this.TrackFileAccess)
            {
                this.RootSource = FileTracker.FormatRootingMarker(this.SourcesCompiled);
            }
            this.SkippedExecution = false;
            return this.SkippedExecution;
        }

        public override bool Execute()
        {
            //			this.BeginUnicodeOutput();
            bool flag = false;
            try
            {
                flag = base.Execute();
            }
            finally
            {
                //				this.EndUnicodeOutput();
            }
            return flag;
        }

        protected virtual bool ForcedRebuildRequired()
        {
            string path = null;
            try
            {
                path = this.TLogCommandFile.GetMetadata("FullPath");
            }
            catch (Exception exception)
            {
                if (!(exception is InvalidOperationException) && !(exception is NullReferenceException))
                {
                    throw;
                }
                base.Log.LogWarningWithCodeFromResources("TrackedVCToolTask.RebuildingDueToInvalidTLog", new object[] { exception.Message });
                return true;
            }
            if (!File.Exists(path))
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingNoCommandTLog", new object[] { this.TLogCommandFile.GetMetadata("FullPath") });
                return true;
            }
            return false;
        }

        protected virtual List<ITaskItem> GenerateSourcesOutOfDateDueToCommandLine()
        {
            IDictionary<string, string> dictionary = this.MapSourcesToCommandLines();
            List<ITaskItem> list = new List<ITaskItem>();
            if (dictionary.Count == 0)
            {
                foreach (ITaskItem item in this.TrackedInputFiles)
                {
                    list.Add(item);
                }
                return list;
            }
            if (this.MaintainCompositeRootingMarkers)
            {
                string str = this.ApplyPrecompareCommandFilter(base.GenerateCommandLine());
                string str2 = null;
                if (dictionary.TryGetValue(FileTracker.FormatRootingMarker(this.TrackedInputFiles), out str2))
                {
                    str2 = this.ApplyPrecompareCommandFilter(str2);
                    if ((str2 == null) || !str.Equals(str2, StringComparison.Ordinal))
                    {
                        base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingAllSourcesCommandLineChanged", new object[0]);
                        foreach (ITaskItem item2 in this.TrackedInputFiles)
                        {
                            list.Add(item2);
                        }
                    }
                    return list;
                }
                foreach (ITaskItem item3 in this.TrackedInputFiles)
                {
                    list.Add(item3);
                }
                return list;
            }
            string str3 = this.SourcesPropertyName ?? "Sources";
            string str4 = base.GenerateCommandLineExceptSwitches(new string[] { str3 });
            foreach (ITaskItem item4 in this.TrackedInputFiles)
            {
                string str5 = this.ApplyPrecompareCommandFilter(str4 + " " + item4.GetMetadata("FullPath").ToUpperInvariant());
                string str6 = null;
                if (dictionary.TryGetValue(FileTracker.FormatRootingMarker(item4), out str6))
                {
                    str6 = this.ApplyPrecompareCommandFilter(str6);
                    if ((str6 == null) || !str5.Equals(str6, StringComparison.Ordinal))
                    {
                        list.Add(item4);
                    }
                }
                else
                {
                    list.Add(item4);
                }
            }
            return list;
        }

        protected override void LogPathToTool(string toolName, string pathToTool)
        {
            base.LogPathToTool(toolName, this.pathToLog);
        }

        protected IDictionary<string, string> MapSourcesToCommandLines()
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string metadata = this.TLogCommandFile.GetMetadata("FullPath");
            if (File.Exists(metadata))
            {
                using (StreamReader reader = File.OpenText(metadata))
                {
                    bool flag = false;
                    string key = string.Empty;
                    for (string str3 = reader.ReadLine(); str3 != null; str3 = reader.ReadLine())
                    {
                        if (str3.Length == 0)
                        {
                            flag = true;
                            break;
                        }
                        if (str3[0] == '^')
                        {
                            if (str3.Length == 1)
                            {
                                flag = true;
                                break;
                            }
                            key = str3.Substring(1);
                        }
                        else
                        {
                            string str4 = null;
                            if (!dictionary.TryGetValue(key, out str4))
                            {
                                dictionary[key] = str3;
                            }
                            else
                            {
                                IDictionary<string, string> dictionary2;
                                string str5;
                                (dictionary2 = dictionary)[str5 = key] = dictionary2[str5] + "\r\n" + str3;
                            }
                        }
                    }
                    if (flag)
                    {
                        base.Log.LogWarningWithCodeFromResources("TrackedVCToolTask.RebuildingDueToInvalidTLogContents", new object[] { metadata });
                        dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            return dictionary;
        }

        protected ITaskItem[] MergeOutOfDateSourceLists(ITaskItem[] sourcesOutOfDateThroughTracking, List<ITaskItem> sourcesWithChangedCommandLines)
        {
            if (sourcesWithChangedCommandLines.Count == 0)
            {
                return sourcesOutOfDateThroughTracking;
            }
            if (sourcesOutOfDateThroughTracking.Length == 0)
            {
                if (sourcesWithChangedCommandLines.Count == this.TrackedInputFiles.Length)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingAllSourcesCommandLineChanged", new object[0]);
                }
                else
                {
                    foreach (ITaskItem item in sourcesWithChangedCommandLines)
                    {
                        base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingSourceCommandLineChanged", new object[] { item.GetMetadata("FullPath") });
                    }
                }
                return sourcesWithChangedCommandLines.ToArray();
            }
            if (sourcesOutOfDateThroughTracking.Length == this.TrackedInputFiles.Length)
            {
                return this.TrackedInputFiles;
            }
            if (sourcesWithChangedCommandLines.Count == this.TrackedInputFiles.Length)
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingAllSourcesCommandLineChanged", new object[0]);
                return this.TrackedInputFiles;
            }
            Dictionary<ITaskItem, bool> dictionary = new Dictionary<ITaskItem, bool>();
            foreach (ITaskItem item2 in sourcesOutOfDateThroughTracking)
            {
                dictionary[item2] = false;
            }
            foreach (ITaskItem item3 in sourcesWithChangedCommandLines)
            {
                if (!dictionary.ContainsKey(item3))
                {
                    dictionary.Add(item3, true);
                }
            }
            List<ITaskItem> list = new List<ITaskItem>();
            foreach (ITaskItem item4 in this.TrackedInputFiles)
            {
                bool flag = false;
                if (dictionary.TryGetValue(item4, out flag))
                {
                    list.Add(item4);
                    if (flag)
                    {
                        base.Log.LogMessageFromResources(MessageImportance.Low, "TrackedVCToolTask.RebuildingSourceCommandLineChanged", new object[] { item4.GetMetadata("FullPath") });
                    }
                }
            }
            return list.ToArray();
        }

        protected virtual void RemoveTaskSpecificInputs(CanonicalTrackedInputFiles compactInputs)
        {
        }

        protected virtual void RemoveTaskSpecificOutputs(CanonicalTrackedOutputFiles compactOutputs)
        {
        }

        protected override bool SkipTaskExecution()
        {
            return this.ComputeOutOfDateSources();
        }

        protected void WriteSourcesToCommandLinesTable(IDictionary<string, string> sourcesToCommandLines)
        {
            using (StreamWriter writer = new StreamWriter(this.TLogCommandFile.GetMetadata("FullPath"), false, Encoding.Unicode))
            {
                foreach (KeyValuePair<string, string> pair in sourcesToCommandLines)
                {
                    writer.WriteLine("^" + pair.Key);
                    writer.WriteLine(pair.Value);
                }
            }
        }

        // Properties
        public virtual bool AttributeFileTracking
        {
            get
            {
                return false;
            }
        }

        protected abstract string CommandTLogName { get; }

        public ITaskItem[] ExcludedInputPaths
        {
            get
            {
                return this.excludedInputPaths;
            }
            set
            {
                this.excludedInputPaths = value;
            }
        }

        protected virtual bool MaintainCompositeRootingMarkers
        {
            get
            {
                return false;
            }
        }

        public bool MinimalRebuildFromTracking
        {
            get
            {
                return this.minimalRebuildFromTracking;
            }
            set
            {
                this.minimalRebuildFromTracking = value;
            }
        }

        public string PathOverride
        {
            get
            {
                return this.pathOverride;
            }
            set
            {
                this.pathOverride = value;
            }
        }

        protected abstract string[] ReadTLogNames { get; }

        protected string RootSource
        {
            get
            {
                return this.rootSource;
            }
            set
            {
                this.rootSource = value;
            }
        }

        [Output]
        public bool SkippedExecution
        {
            get
            {
                return this.skippedExecution;
            }
            set
            {
                this.skippedExecution = value;
            }
        }

        protected CanonicalTrackedInputFiles SourceDependencies
        {
            get
            {
                return this.sourceDependencies;
            }
            set
            {
                this.sourceDependencies = value;
            }
        }

        [Output]
        public ITaskItem[] SourcesCompiled
        {
            get
            {
                return this.sourcesCompiled;
            }
            set
            {
                this.sourcesCompiled = value;
            }
        }

        protected virtual string SourcesPropertyName
        {
            get
            {
                return "Sources";
            }
        }

        public ITaskItem TLogCommandFile
        {
            get
            {
                return this.tlogCommandFile;
            }
            set
            {
                this.tlogCommandFile = value;
            }
        }

        public ITaskItem[] TLogReadFiles
        {
            get
            {
                return this.tlogReadFiles;
            }
            set
            {
                this.tlogReadFiles = value;
            }
        }

        public ITaskItem[] TLogWriteFiles
        {
            get
            {
                return this.tlogWriteFiles;
            }
            set
            {
                this.tlogWriteFiles = value;
            }
        }

        public string ToolArchitecture { get; set; }

        protected virtual ExecutableType? ToolType
        {
            get
            {
                return null;
            }
        }

        protected abstract ITaskItem[] TrackedInputFiles { get; }

        public ITaskItem[] TrackedInputFilesToIgnore
        {
            get
            {
                return this.trackedInputFilesToIgnore;
            }
            set
            {
                this.trackedInputFilesToIgnore = value;
            }
        }

        public ITaskItem[] TrackedOutputFilesToIgnore
        {
            get
            {
                return this.trackedOutputFilesToIgnore;
            }
            set
            {
                this.trackedOutputFilesToIgnore = value;
            }
        }

        public string TrackerFrameworkPath { get; set; }

        protected abstract string TrackerIntermediateDirectory { get; }

        public string TrackerSdkPath { get; set; }

        public bool TrackFileAccess
        {
            get
            {
                return this.trackFileAccess;
            }
            set
            {
                this.trackFileAccess = value;
            }
        }

        protected virtual bool UseMinimalRebuildOptimization
        {
            get
            {
                return false;
            }
        }

        protected virtual bool UseUnicodeOutput
        {
            get
            {
                return false;
            }
        }

        protected abstract string[] WriteTLogNames { get; }
    }

}
