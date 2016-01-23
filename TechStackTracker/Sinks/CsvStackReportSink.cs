using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TechStackTracker.Sinks
{
    public class CsvStackReportSink : IStackReportSink
    {
        private readonly List<StackItem> _reports;

        public CsvStackReportSink(List<StackItem> reports)
        {
            _reports = reports;
        }

        public void Flush()
        {
            var report = new StringBuilder();
            report.AppendLine("Stack Name, Version, Application, Location, Parser");

            _reports
                .GroupBy(r => r.TechnologyName)
                .ToList()
                .ForEach(g =>
                {
                    g.ToList()
                    .OrderBy(r=> r.VersionUsed)
                    .GroupBy(grpv => grpv.VersionUsed)
                    .ToList()
                    .ForEach(gv =>
                    {
                        gv.ToList()
                            .ForEach(r =>
                            {
                                report.AppendFormat("{0},{1},{2},{3},{4}{5}", r.TechnologyName, r.VersionUsed, r.SolutionName, r.SolutionLocation,  r.Parser,Environment.NewLine);
                            });
                    });
                });


            File.WriteAllText(@"C:\Users\rddag\Desktop\test.csv", report.ToString(), Encoding.UTF8);
        }
    }
}