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
        public string Name => this.GetType().Name;
        public string Description { get; } = "Tracks all referenced assemblies in a c# project.";

        public ReferencedAssembliesTracker()
        {
        }

        public TrackerResult Run(string workspace, List<string> skipList)
        {
            var stackReport = new TrackerResult();

            var solutionFiles = Directory.EnumerateFiles(workspace, "*.sln", SearchOption.AllDirectories).ToList();

            solutionFiles.ForEach(s=>
            {
                try
                {
                    var solution = new Solution
                    {
                        Name = Path.GetFileNameWithoutExtension(s),
                        Location = s,
                    };
                    stackReport.Solutions.Add(solution);

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
                            ConsoleEx.Error(ex.Message);
                        }

                    });
                }
                catch (Exception ex)
                {
                    ConsoleEx.Error(ex.Message);
                }
            });

            return stackReport;
        }

        //thanks @granadacoder where i based the strategy to extract assembly references
        //https://granadacoder.wordpress.com/2012/10/11/how-to-find-references-in-a-c-project-file-csproj-using-linq-xml/
        private List<Dependency> ExtractReferencedAssemblies(string projectFile)
        {
            var dependecies = new List<Dependency>();

            try
            {
                if (File.Exists(projectFile))
                {
                    XDocument xml = XDocument.Load(projectFile);
                    XNamespace ns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

                    var targetFrameworkVersion =
                        (from l in xml.Descendants(ns + "PropertyGroup")
                             from i in l.Elements(ns + "TargetFrameworkVersion")
                             select new
                             {
                                 targetFrameworkVersion = i.Value
                             }
                        ).FirstOrDefault()?
                        .targetFrameworkVersion
                        .Replace("v",string.Empty);

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
                                                 let versionInfo = referenceInfo.Count > 1 ? 
                                                    (referenceInfo[1].Contains("Version") ? 
                                                        referenceInfo[1].Split('=')[1] : 
                                                        "<Missing>") : 
                                                    targetFrameworkVersion
                                                 select new Dependency
                                                 {
                                                     Name = referenceName,
                                                     Version = versionInfo,
                                                     Location = v.Location
                                                 }
                                         );
                }
                else
                {
                    ConsoleEx.Warn($"Missing file: {projectFile}");
                }
            }
            catch (Exception ex)
            {
                ConsoleEx.Error(ex.Message);                
            }

            return dependecies;
        }
    }
}

