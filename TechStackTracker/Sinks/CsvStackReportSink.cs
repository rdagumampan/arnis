using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TechStackTracker.Sinks
{
    public class CsvStackReportSink : IStackReportSink
    {
        private readonly string _fileName;
        private readonly List<StackItem> _reports;

        public CsvStackReportSink(string fileName, List<StackItem> reports)
        {
            _fileName = fileName;
            _reports = reports;
        }

        public void Flush()
        {
            var report = new StringBuilder();
            report.AppendLine("Name, Version, Application, Location");

            _reports
                .GroupBy(r => r.ComponentName)
                .ToList()
                .ForEach(g =>
                {
                    g.ToList()
                    .OrderBy(r => r.ComponentVersion)
                    .GroupBy(grpv => grpv.ComponentVersion)
                    .ToList()
                    .ForEach(gv =>
                    {
                        gv.ToList()
                            .ForEach(r =>
                            {
                                report.AppendFormat("{0},{1},{2},{3},{4}", r.ComponentName, r.ComponentVersion, r.SolutionName, r.SolutionLocation, Environment.NewLine);
                            });
                    });
                });


            File.WriteAllText(_fileName, report.ToString(), Encoding.UTF8);
        }
    }
}