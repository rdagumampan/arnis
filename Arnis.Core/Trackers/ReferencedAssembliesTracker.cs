using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Arnis.Core.Trackers
{
    public class ReferencedAssembliesTracker : ITracker
    {
        private readonly string _workingDirectory;
        public string Name { get; } = "ReferencedAssembliesTracker";
        public string Description { get; } = "Tracks all referenced assemblies in a cs project.";

        public ReferencedAssembliesTracker(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public TrackerResult Run()
        {
            var stackReport = new TrackerResult();

            var solutionFiles = Directory.EnumerateFiles(_workingDirectory, "*.sln", SearchOption.AllDirectories).ToList();

            solutionFiles.ForEach(s=>
            {
                try
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
                        try
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
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            return stackReport;
        }

        //thanks @granadacoder for this very cool snippet
        //https://granadacoder.wordpress.com/2012/10/11/how-to-find-references-in-a-c-project-file-csproj-using-linq-xml/
        private List<Dependency> ExtractReferencedAssemblies(string projectFile)
        {
            var dependecies = new List<Dependency>();

            try
            {
                XDocument xml = XDocument.Load(projectFile);
                XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

                var targetFrameworkVersion =
                    (from l in xml.Descendants(ns + "PropertyGroup")
                     from i in l.Elements(ns + "TargetFrameworkVersion")
                     select new
                     {
                         x = i.Value
                     }).First().x.ToString();

                var rawRefences = 
                    from l in xml.Descendants(ns + "ItemGroup")
                        from i in l.Elements(ns + "Reference")
                            select new
                            {
                                ProjectFile = projectFile,
                                Reference = i.Attribute("Include").Value,
                                ReferenceType = (i.Element(ns + "HintPath") == null) ? "GAC" : "DLL",
                                Location = (i.Element(ns + "HintPath") == null) ? string.Empty : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectFile), i.Element(ns + "HintPath").Value))
                            };

                dependecies.AddRange(from v in rawRefences
                    let referenceInfo = v.Reference.Split(',').ToList()
                    let referenceName = referenceInfo[0]
                    let versionInfo = referenceInfo.Count > 1 ? (referenceInfo[1].Contains("Version") ? referenceInfo[1].Split('=')[1] : "<Missing>") : targetFrameworkVersion
                                     select new Dependency
                    {
                        Name = referenceName, Version = versionInfo, Location = v.Location
                    });

                dependecies.ForEach(f =>
                {
                    if (!string.IsNullOrEmpty(f.Location))
                    {
                        var exists = File.Exists(f.Location);
                        if (!exists)
                        {
                            Console.ForegroundColor= ConsoleColor.Red;
                            Console.WriteLine("Missing dependency file: dll: {0}, targetProject: {1}", f.Location, projectFile);
                        }
                    }
                });
                Console.ForegroundColor = ConsoleColor.White;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                
            }

            return dependecies;
        }
    }
}

