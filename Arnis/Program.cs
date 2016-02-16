using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Arnis.Core;
using Arnis.Trackers;
using Arnis.Sinks;

namespace Arnis
{
    static class Program
    {
        //arnis /wf:c:\users\rddag\dss /sf:c:\test.csv
        static void Main(string[] args)
        {
            try
            {
                Regex regex = new Regex(@"/(?<name>.+?):(?<val>.+)");
                Dictionary<string, string> settings = args.Select(s =>
                    regex.Match(s)).Where(m => m.Success)
                    .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);

                Console.WriteLine("Running Arnis on ff settings:");
                settings.ToList().ForEach(s =>
                {
                    Console.WriteLine("\t" + s.Key + "," + s.Value);
                }
                );

                //settings.Add("wf", @"C:\Users\rddag\DSS");
                //settings.Add("sf", @"C:\Users\rddag\Desktop\stackreport.dss.csv");

                //settings.Add("wf", @"C:\Users\rddag\Desktop\GitHub\arnis");
                //settings.Add("sf", @"C:\Users\rddag\Desktop\stackreport.arnis.csv");

                string wf = settings.SingleOrDefault(s => s.Key == "wf").Value;
                if (null == wf)
                {
                    throw new ArgumentException("Missing parameter", "wf");
                }

                string sf = settings.SingleOrDefault(s => s.Key == "sf").Value;
                if (null == sf)
                {
                    throw new ArgumentException("Missing parameter", "sf");
                }

                var skipList = new List<string>();
                string skf = settings.SingleOrDefault(s => s.Key == "skf").Value;
                if (null == skf)
                {
                    Console.WriteLine("No skip file (skip.data) defined. /skf:<skipfile>");
                }
                else
                {
                    var skipFile = Path.Combine(Environment.CurrentDirectory, "skip.data");
                    if (File.Exists(skipFile))
                    {
                        skipList = File.ReadAllLines(skipFile).ToList();

                        if (!skipList.Any())
                        {
                            Console.WriteLine("Warning: Skip file is empty, but it's ok.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Warning: Skip file doesn't exists, but we'll continue anyway");
                    }
                }

                Console.WriteLine("walk around, this may take some time...");

                //TODO: make this dynamic by reflecting all IStackTracker
                var vsTracker = new VisualStudioStackTracker(wf, skipList);
                var vsStackReport = Task.Run(() => vsTracker.Run()).Result;

                var referenceTracker = new ReferencedAssembliesTracker(wf);
                var refStackReport = Task.Run(() => referenceTracker.Run()).Result;

                var frameworkVersionTracker = new FrameworkVersionTracker(wf);
                var frameoworkVersionReport = Task.Run(() => frameworkVersionTracker.Run()).Result;

                //consolidate
                //TODO: make this dynamic by reflecting all IStackReportSink
                var fullStackReport = new StackReport
                {
                    Results = vsStackReport.Results
                        .Union(refStackReport.Results)
                        .Union(frameoworkVersionReport.Results)
                        .ToList(),
                    Errors = vsStackReport.Errors
                        .Union(refStackReport.Errors)
                        .Union(frameoworkVersionReport.Errors)
                        .ToList()
                };

                if (fullStackReport.Errors.ToList().Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine("Unknown files found: {0}", fullStackReport.Errors.Count());
                    fullStackReport.Errors.ForEach(f =>
                    {
                        Console.WriteLine("\t{0}", f);
                    });
                    Console.ForegroundColor = ConsoleColor.White;
                }

                var stackSink = new CsvStackReportSink(sf, fullStackReport.Results);
                stackSink.Flush();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Done! Check " + sf);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("Error running Arnis. \n" + ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.Read();
        }
    }
}
