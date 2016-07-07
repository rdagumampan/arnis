using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Arnis.Core.Trackers
{
    public class FrameworkVersionTracker: ITracker
    {
        public string Name => this.GetType().Name;
        public string Description { get; } = "Tracks target framework version for each project";

        readonly TrackerResult _trackerResult = new TrackerResult();
        public TrackerResult Run(string workspace, List<string> skipList)
        {
            var solutionFiles = Directory.EnumerateFiles(workspace, "*.sln", SearchOption.AllDirectories).ToList();
            solutionFiles.ForEach(s =>
            {
                try
                {
                    var solution = new Solution
                    {
                        Name = Path.GetFileNameWithoutExtension(s),
                        Location = s,
                    };
                    _trackerResult.Solutions.Add(solution);

                    var solutionFileContent = File.ReadAllText(s);
                    var projRegex = new Regex("Project\\(\"\\{[\\w-]*\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs)proj)\"", RegexOptions.Compiled);

                    var matches = projRegex.Matches(solutionFileContent).Cast<Match>();
                    var projectFiles = matches.Select(x => x.Groups[2].Value).ToList();

                    projectFiles.ForEach(p =>
                    {
                        try
                        {
                            var project = new Project
                            {
                                Name = Path.GetFileNameWithoutExtension(p),
                                Location = p
                            };
                            solution.Projects.Add(project);

                            string projectFile = string.Empty;
                            if (!Path.IsPathRooted(p))
                            {
                                var directoryName = Path.GetDirectoryName(s);
                                if (directoryName != null)
                                    projectFile = Path.Combine(directoryName, p);
                            }
                            else
                            {
                                projectFile = Path.GetFullPath(p);
                            }

                            if (File.Exists(projectFile))
                            {
                                var xml = XDocument.Load(projectFile);
                                var ns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

                                var targetFrameworkVersion =
                                    (from l in xml.Descendants(ns + "PropertyGroup")
                                         from i in l.Elements(ns + "TargetFrameworkVersion")
                                         select new
                                         {
                                             targetFrameworkVersion = i.Value
                                         }
                                     ).FirstOrDefault()?
                                    .targetFrameworkVersion.ToString()
                                    .Replace("v",string.Empty);

                                var projectDependencies = new Dependency
                                {
                                    Name = ".NetFramework",
                                    Version = targetFrameworkVersion,
                                    Location = ""
                                };

                                project.Dependencies.Add(projectDependencies);
                            }
                            else
                            {
                                string message = $"Missing file: {projectFile}";
                                _trackerResult.Logs.Add($"WARN: {message}");

                                ConsoleEx.Warn(message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _trackerResult.Logs.Add($"ERROR: {ex.Message}");
                            ConsoleEx.Error(ex.Message);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _trackerResult.Logs.Add($"ERROR: {ex.Message}");
                    ConsoleEx.Error(ex.Message);
                }
            });

            return _trackerResult;
        }
    }
}
