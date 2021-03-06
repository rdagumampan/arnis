﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Arnis.Core.Trackers
{
    //http://pascoal.net/2011/05/getting-visual-studio-version-of-a-solution-file/
    //https://en.wikipedia.org/wiki/Microsoft_Visual_Studio
    //https://regex101.com/
    public class VisualStudioTracker : ITracker
    {

        public VisualStudioTracker()
        {
        }

        public string Name => this.GetType().Name;
        public string Description { get; } = "Track all projects that used Microsoft Visual Studio IDE.";

        readonly TrackerResult _trackerResult = new TrackerResult();
        public TrackerResult Run(string workspace, List<string> skipList)
        {
            var solutionFiles = Directory.EnumerateFiles(workspace, "*.sln", SearchOption.AllDirectories).ToList();

            //skip all files within the skip list
            var solutionFilesStage1 = solutionFiles.Where(f => !skipList.Exists(f.Contains))
                .ToList();

            //tracks all with latest versions from 2013 - 2015
            solutionFilesStage1.ForEach(f =>
            {
                var versionMap = TrackWithVisualStudioVersion(f);
                if(null != versionMap)
                {
                    var solution = new Solution
                    {
                        Name = Path.GetFileNameWithoutExtension(f),
                        Location = f
                    };

                    solution.Dependencies.Add(new Dependency
                    {
                        Name = Name.Replace("Tracker", ""),
                        Version = versionMap.ProductVersion
                    });

                    _trackerResult.Solutions.Add(solution);
                }

            });

            //tracks all that were skipped in stage 1 from 2003 - 2012
            var solutionFilesStage2 = solutionFilesStage1.Where(f => !_trackerResult.Solutions.Exists(r => r.Location == f)).ToList();
            solutionFilesStage2.ForEach(f=>
            {
                var versionMap = TrackWithSolutionFormat(f);
                if (null != versionMap)
                {
                    var solution = new Solution
                    {
                        Name = Path.GetFileNameWithoutExtension(f),
                        Location = f
                    };

                    solution.Dependencies.Add(new Dependency
                    {
                        Name = Name.Replace("Tracker",""),
                        Version = versionMap.ProductVersion
                    });

                    _trackerResult.Solutions.Add(solution);
                }
            });


            //record all skipped files and save as error
            var skippedFilesStage2 = solutionFilesStage1
                .Where(f => !_trackerResult.Solutions.Exists(r => r.Location == f))
                .Select(f=> $"WARN: Skipped file {f}")
                .ToList();
            _trackerResult.Logs = skippedFilesStage2;

            return _trackerResult;
        }

        //only VS2013 - VS2015 contains VisualStudioVersion attribute in sln files
        private VisualStudioVersionMap TrackWithVisualStudioVersion(string solutionFile)
        {
            VisualStudioVersionMap visualStudioMap = null;
            string contents = File.ReadAllText(solutionFile);

            var versionMappings = GetVsVersionMapping();
            versionMappings.ForEach(v =>
            {
                var regex = new Regex($"(\\W|^)VisualStudioVersion\\s=\\s{v.VsVisualStudioVersion}");
                if (regex.IsMatch(contents))
                {
                    visualStudioMap = v;
                }
            });

            return visualStudioMap;
        }

        //VS2012 - VS2015 uses the same solution file format version which is 12.00
        private VisualStudioVersionMap TrackWithSolutionFormat(string f)
        {
            VisualStudioVersionMap visualStudioMap = null;
            string contents = File.ReadAllText(f);

            var versionMappings = GetVsVersionMapping();
            versionMappings.ForEach(v =>
            {
                var regex = new Regex($"(\\W |^)Microsoft\\sVisual\\sStudio\\sSolution\\sFile,\\sFormat\\sVersion\\s{v.VsSlnFileFormatVersion}", RegexOptions.Multiline);
                if (regex.IsMatch(contents))
                {
                    visualStudioMap = v;
                }
            });

            return visualStudioMap;
        }

        //VS2012 - VS2015 uses the same solution file version (visa MSBUILD parser) which is 12.00
        //made this obsolete since using the first line in SLN file returns same result but without MSBUILD dependency
        [Obsolete]
        private List<Dependency> TrackWithMsBuild(List<string> files)
        {
            List<Dependency> report = new List<Dependency>();

            files.ForEach(f =>
            {
                var solutionParser = Type.GetType("Microsoft.Build.Construction.SolutionParser, Microsoft.Build, version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false, false);
                if (solutionParser == null)
                {
                    throw new Exception("Can't load msbuild assembly. Is .Net FX 4.0 installed?");
                }

                var solutionParserSolutionReader = solutionParser.GetProperty("SolutionReader", BindingFlags.NonPublic | BindingFlags.Instance);
                var solutionParserVersion = solutionParser.GetProperty("version", BindingFlags.NonPublic | BindingFlags.Instance);
                var solutionParserParseSolution = solutionParser.GetMethod("ParseSolution", BindingFlags.NonPublic | BindingFlags.Instance);
                solutionParserVersion = solutionParser.GetProperty("Version", BindingFlags.NonPublic | BindingFlags.Instance);

                var solutionParserInstance = solutionParser.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0].Invoke(null);
                using (var streamReader = new StreamReader(f))
                {
                    solutionParserSolutionReader.SetValue(solutionParserInstance, streamReader, null);
                    solutionParserParseSolution.Invoke(solutionParserInstance, null);
                }

                var version = Convert.ToInt32(solutionParserVersion.GetValue(solutionParserInstance, null));

                var versionMappings = GetVsVersionMapping();
                var versionInfo = versionMappings.SingleOrDefault(v => v.VsMsBuildVersion == version.ToString());

                if (null != versionInfo)
                {
                    report.Add(new Dependency
                    {
                        Name = Name.Replace("Tracker", ""),
                        Version = versionInfo.ProductVersion,
                    });
                }
                else
                {
                    string message = $"Version not available";
                    _trackerResult.Logs.Add($"WARN: {message}");
                }

            });
            return report;
        }

        private List<VisualStudioVersionMap> GetVsVersionMapping()
        {
            var vsMapping = new List<VisualStudioVersionMap>
            {
                new VisualStudioVersionMap {ProductVersion = "2002", VsVisualStudioVersion = "7.0", VsSlnFileFormatVersion = "7.0", VsMsBuildVersion = "7"},
                new VisualStudioVersionMap {ProductVersion = "2003", VsVisualStudioVersion = "7.1", VsSlnFileFormatVersion = "8.0", VsMsBuildVersion = "8"},
                new VisualStudioVersionMap {ProductVersion = "2005", VsVisualStudioVersion = "8.0", VsSlnFileFormatVersion = "9.0", VsMsBuildVersion = "9"},
                new VisualStudioVersionMap {ProductVersion = "2008", VsVisualStudioVersion = "9.0", VsSlnFileFormatVersion = "10.0", VsMsBuildVersion = "10"},
                new VisualStudioVersionMap {ProductVersion = "2010", VsVisualStudioVersion = "10.0", VsSlnFileFormatVersion = "11.0", VsMsBuildVersion = "11"},
                new VisualStudioVersionMap {ProductVersion = "2012", VsVisualStudioVersion = "11.0", VsSlnFileFormatVersion = "12.0", VsMsBuildVersion = "12"},
                new VisualStudioVersionMap {ProductVersion = "2013", VsVisualStudioVersion = "12.0", VsSlnFileFormatVersion = "NA", VsMsBuildVersion = "NA"},
                new VisualStudioVersionMap {ProductVersion = "2015", VsVisualStudioVersion = "14.0", VsSlnFileFormatVersion = "NA", VsMsBuildVersion = "NA"},
            };

            return vsMapping;
        }
    }
}