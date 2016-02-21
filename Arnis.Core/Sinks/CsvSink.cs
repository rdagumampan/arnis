using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Arnis.Core.Sinks
{
    public class CsvSink : ISink
    {
        private readonly string _fileName;
        private readonly List<Solution> _reports;

        public CsvSink(string fileName, List<Solution> reports)
        {
            _fileName = fileName;
            _reports = reports;
        }

        public void Flush()
        {
            var report = new StringBuilder();
            report.AppendLine("Dependency, Version, SolutionName, ProjectName, SolutionLocation, ProjectLocation");

            var solutionDependencies = _reports
                .SelectMany(s => s.Dependencies
                .Select(sd => new
                {
                    Name = sd.Name,
                    Version = sd.Version,
                    SolutionName = s.Name,
                    ProjectName = string.Empty,
                    SolutionLocation = s.Location,
                    ProjectLocation = string.Empty,
                }));

            var projectDependencies = _reports
                .SelectMany(s => s.Projects
                    .SelectMany(p => p.Dependencies
                        .Select(pd => new
                        {
                            Name = pd.Name,
                            Version = pd.Version,
                            SolutionName = s.Name,
                            ProjectName = p.Name,
                            SolutionLocation = s.Location,
                            ProjectLocation = p.Location,
                        })));

            var consolidatedDependencies = solutionDependencies.Union(projectDependencies);

            consolidatedDependencies.GroupBy(r => r.Name)
                .ToList()
                .ForEach(g =>
                {
                    g.ToList()
                    .OrderBy(r => r.Version)
                    .GroupBy(grpv => grpv.Version)
                    .ToList()
                    .ForEach(gv =>
                    {
                        gv.ToList()
                            .ForEach(r =>
                            {
                                report.AppendFormat("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",{6}", r.Name, r.Version, r.SolutionName, r.ProjectName, r.SolutionLocation, r.ProjectLocation, Environment.NewLine);
                            });
                    });
                });

            File.WriteAllText(_fileName, report.ToString(), Encoding.UTF8);
        }
    }
}