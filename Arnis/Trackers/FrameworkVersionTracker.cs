using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Arnis.Trackers
{
    public class FrameworkVersionTracker: IStackTracker
    {
        private readonly string _workingDirectory;
        public string Name { get; } = "FrameworkVersionTracker";
        public string Description { get; } = "Tracks target framework version for each project";

        public FrameworkVersionTracker(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public StackReport Run()
        {
            var stackReport = new StackReport();

            var solutionFiles = Directory.EnumerateFiles(_workingDirectory, "*.sln", SearchOption.AllDirectories).ToList();

            solutionFiles.ForEach(s =>
            {
                var solution = new Solution
                {
                    Name = Path.GetFileNameWithoutExtension(s),
                    Location = s,
                };
                stackReport.Results.Add(solution);

                var solutionFileContent = File.ReadAllText(s);
                var projRegex = new Regex("Project\\(\"\\{[\\w-]*\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs)proj)\"", RegexOptions.Compiled);

                var matches = projRegex.Matches(solutionFileContent).Cast<Match>();
                var projectFiles = matches.Select(x => x.Groups[2].Value).ToList();

                projectFiles.ForEach(p =>
                {
                    var project = new Project
                    {
                        Name = Path.GetFileNameWithoutExtension(p),
                        Location = p
                    };
                    solution.Projects.Add(project);

                    string projectFile;
                    if (!Path.IsPathRooted(p))
                    {
                        projectFile = Path.Combine(Path.GetDirectoryName(s), p);
                    }
                    else
                    {
                        projectFile = Path.GetFullPath(p);
                    }

                    var xml = XDocument.Load(projectFile);
                    var ns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

                    var targetFrameworkVersion =
                        (from l in xml.Descendants(ns + "PropertyGroup")
                        from i in l.Elements(ns + "TargetFrameworkVersion")
                        select new
                        {
                            x =i.Value
                        }).First().x.ToString();

                    var projectDependencies = new ProjectDependency
                    {
                        Name = ".NetFramework",
                        Version = targetFrameworkVersion,
                        Location = ""
                    };

                    project.Dependencies.Add(projectDependencies);


                });
            });

            return stackReport;
        }
    }
}
