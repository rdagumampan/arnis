using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Winterfern.Sinks
{
    public class CsvStackReportSink : IStackReportSink
    {
        private readonly string _fileName;
        private readonly List<Solution> _reports;

        public CsvStackReportSink(string fileName, List<Solution> reports)
        {
            _fileName = fileName;
            _reports = reports;
        }

        public void Flush()
        {
            var report = new StringBuilder();
            report.AppendLine("Component, Version, SolutionName, SolutionLocation, ProjectName, ProjectLocation");

            var solutionDependencies = _reports
                .SelectMany(s => s.Dependencies
                .Select(sd => new
                {
                    SolutionName = s.Name,
                    SolutionLocation = s.Location,
                    ProjectName = string.Empty,
                    ProjectLocation = string.Empty,
                    Name = sd.Name,
                    Version = sd.Version,
                }));

            var projectDependencies = _reports
                .SelectMany(s => s.Projects
                    .SelectMany(p => p.Dependencies
                        .Select(pd => new
                        {
                            SolutionName = s.Name,
                            SolutionLocation = s.Location,
                            ProjectName = p.Name,
                            ProjectLocation = p.Location,
                            Name = pd.Name,
                            Version = pd.Version,
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
                                report.AppendFormat("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",{6}", r.Name, r.Version, r.SolutionName, r.SolutionLocation, r.ProjectName, r.ProjectLocation, Environment.NewLine);
                            });
                    });
                });

            File.WriteAllText(_fileName, report.ToString(), Encoding.UTF8);
        }
    }
}