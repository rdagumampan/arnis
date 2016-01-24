using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace TechStackTracker.Trackers
{
    public class ReferencedAssembliesTracker : IStackTracker
    {
        private readonly string _workingDirectory;
        public string Name { get; } = "ReferencedAssembliesTracker";
        public string Description { get; } = "Tracks all referenced assemblies in a cs project.";

        public ReferencedAssembliesTracker(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public StackReport Run()
        {
            var stackReport = new StackReport();

            var solutionFiles = Directory.EnumerateFiles(_workingDirectory, "*.sln", SearchOption.AllDirectories).ToList();

            solutionFiles.ForEach(s=>
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


                projectFiles.ForEach(p=>
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

                    var projectDependencies = ExtractReferencedAssemblies(projectFile);
                    project.Dependencies.AddRange(projectDependencies);
                });
            });

            return stackReport;
        }

        //thanks @granadacoder for this very cool snippet
        //https://granadacoder.wordpress.com/2012/10/11/how-to-find-references-in-a-c-project-file-csproj-using-linq-xml/
        private List<ProjectDependency> ExtractReferencedAssemblies(string projectFile)
        {
            var dependecies = new List<ProjectDependency>();

            try
            {
                XDocument xml = XDocument.Load(projectFile);
                XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

                var rawRefences = 
                    from l in xml.Descendants(ns + "ItemGroup")
                        from i in l.Elements(ns + "Reference")
                            select new
                            {
                                ProjectFile = projectFile,
                                Reference = i.Attribute("Include").Value,
                                ReferenceType = (i.Element(ns + "HintPath") == null) ? "GAC" : "DLL",
                                Location = (i.Element(ns + "HintPath") == null) ? string.Empty : i.Element(ns + "HintPath").Value
                            };

                dependecies.AddRange(from v in rawRefences
                    let referenceInfo = v.Reference.Split(',').ToList()
                    let referenceName = referenceInfo[0]
                    let versionInfo = referenceInfo.Count > 1 ? (referenceInfo[1].Contains("Version") ? referenceInfo[1].Split('=')[1] : string.Empty) : string.Empty
                    select new ProjectDependency
                    {
                        Name = referenceName, Version = versionInfo, Location = v.Location
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                
            }

            return dependecies;
        }
    }
}

