using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;

namespace TechStackTracker.Trackers
{
    public class VsMapping
    {
        public string Name { get; set; }
        public string VsVersion { get; set; }
        public string VsSlnFileFormatVersion { get; set; }
        public string VsMsBuildVersion { get; set; }
    }

    //http://pascoal.net/2011/05/getting-visual-studio-version-of-a-solution-file/
    //https://en.wikipedia.org/wiki/Microsoft_Visual_Studio
    //https://regex101.com/
    public class VisualStudioStackTracker : IStackTracker
    {
        private readonly string _workingDirectory;

        public VisualStudioStackTracker(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public string Name { get;} = "VisualStudio";
        public string Description { get; } = "Track all projects that used Microsoft Visual Studio IDE.";

        public StackReport Run()
        {
            var results = new List<StackItem>();
            var files = Directory.EnumerateFiles(_workingDirectory, "*.sln", SearchOption.AllDirectories).ToList();

            //tracks all with latest versions from 2014
            var stage1TrackedFiles = TrackWithVisualStudioVersion(files);
            results.AddRange(stage1TrackedFiles);

            //tracks all that were skipped in stage 1
            var skippedStaged1 = files.Where(f => !results.Exists(r => r.SolutionLocation == f)).ToList();
            var stage2TrackedFiles = TrackWithSolutionFormat(skippedStaged1);
            results.AddRange(stage2TrackedFiles);

            //tracks all that were skipped in stage 2
            var skippedStaged2 = files.Where(f => !results.Exists(r => r.SolutionLocation == f)).ToList();
            var stage3TrackedFiles = TrackWithMsBuild(skippedStaged2);
            results.AddRange(stage3TrackedFiles);

            //record all skipped files
            var skippedStage3 = files.Where(f => !results.Exists(r => r.SolutionLocation == f)).ToList();

            return new StackReport
            {
                Results = results,
                Errors = skippedStage3
            };
        }

        private List<VsMapping> GetVsMapping()
        {
            var vsMapping = new List<VsMapping>
            {
                new VsMapping {Name = "VS 2002", VsVersion = "7.0", VsSlnFileFormatVersion = "7.0", VsMsBuildVersion = "7"},
                new VsMapping {Name = "VS 2003", VsVersion = "7.1", VsSlnFileFormatVersion = "8.0", VsMsBuildVersion = "8"},
                new VsMapping {Name = "VS 2005", VsVersion = "8.0", VsSlnFileFormatVersion = "9.0", VsMsBuildVersion = "9"},
                new VsMapping {Name = "VS 2008", VsVersion = "9.0", VsSlnFileFormatVersion = "10.0", VsMsBuildVersion = "10"},
                new VsMapping {Name = "VS 2010", VsVersion = "10.0", VsSlnFileFormatVersion = "11.0", VsMsBuildVersion = "11"},
                new VsMapping {Name = "VS 2012", VsVersion = "11.0", VsSlnFileFormatVersion = "12.0", VsMsBuildVersion = "NA"},
                new VsMapping {Name = "VS 2013", VsVersion = "12.0", VsSlnFileFormatVersion = "NA", VsMsBuildVersion = "NA"},
                new VsMapping {Name = "VS 2015", VsVersion = "14.0", VsSlnFileFormatVersion = "NA", VsMsBuildVersion = "NA"},
            };

            return vsMapping;
        }

        private List<StackItem> TrackWithVisualStudioVersion(List<string> files)
        {
            List<StackItem> report = new List<StackItem>();

            files.ForEach(f =>
            {
                string contents = File.ReadAllText(f);

                var versionMappings = GetVsMapping();
                versionMappings.ForEach(v =>
                {
                    var regex = new Regex($"(\\W|^)VisualStudioVersion\\s=\\s{v.VsVersion}");
                    if (regex.IsMatch(contents))
                    {
                        report.Add(new StackItem
                        {
                            TechnologyName = Name,
                            VersionUsed = v.Name,
                            SolutionName = Path.GetFileNameWithoutExtension(f),
                            SolutionLocation = f,
                            Parser = "VsVersion"
                        });
                    }
                });
            });

            return report;
        }

        private List<StackItem> TrackWithSolutionFormat(List<string> files)
        {
            List<StackItem> report = new List<StackItem>();

            files.ForEach(f =>
            {
                string contents = File.ReadAllText(f);

                var versionMappings = GetVsMapping();
                versionMappings.ForEach(v =>
                {
                    var regex = new Regex($"(\\W |^)Microsoft\\sVisual\\sStudio\\sSolution\\sFile,\\sFormat\\sVersion\\s{v.VsSlnFileFormatVersion}",RegexOptions.Multiline);
                    if (regex.IsMatch(contents))
                    {
                        report.Add(new StackItem
                        {
                            TechnologyName = Name,
                            VersionUsed = v.Name,
                            SolutionName = Path.GetFileNameWithoutExtension(f),
                            SolutionLocation = f,
                            Parser = "SlnFormat"
                        });
                    }
                });
            });

            return report;
        }

        private List<StackItem> TrackWithMsBuild(List<string> files)
        {
            List<StackItem> report = new List<StackItem>();

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

                var versionMappings = GetVsMapping();
                var versionInfo = versionMappings.SingleOrDefault(v => v.VsMsBuildVersion == version.ToString());

                if(null != versionInfo)
                {
                    report.Add(new StackItem
                    {
                        TechnologyName = Name,
                        VersionUsed = versionInfo.Name,
                        SolutionName = Path.GetFileNameWithoutExtension(f),
                        SolutionLocation = f,
                         Parser = "MsBuild"
                    });
                }

            });
            return report;
        }
    }
}