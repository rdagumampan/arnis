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
        private readonly List<Solution> _reports;

        public CsvStackReportSink(string fileName, List<Solution> reports)
        {
            _fileName = fileName;
            _reports = reports;
        }

        public void Flush()
        {
            var report = new StringBuilder();
            report.AppendLine("DepName, DepVersion, AppName, AppLocation");

            _reports
                .SelectMany(r=> r.Dependencies
                .Select(d=> new
                {
                    SlnName = r.Name,
                    SlnLocation = r.Location,
                    Name = d.Name,
                    Version = d.Version,
                }))
                .GroupBy(r => r.Name)
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
                                report.AppendFormat("{0},{1},{2},{3},{4}", r.Name, r.Version, r.SlnName, r.SlnLocation, Environment.NewLine);
                            });
                    });
                });

            File.WriteAllText(_fileName, report.ToString(), Encoding.UTF8);
        }
    }
}