using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winterfern.Trackers;
using Winterfern.Sinks;

namespace Winterfern
{
    class Program
    {
        private static string _workingDirectory = @"C:\Users\rddag\DSS";

        static void Main(string[] args)
        {
            Console.Write("Press key to run...");

            var skipFile = Path.Combine(Environment.CurrentDirectory, "skip.data");
            var skipList = File.ReadAllLines(skipFile).ToList();

            var vsTracker = new VisualStudioStackTracker(_workingDirectory, skipList);
            var vsStackReport = vsTracker.Run();

            var referenceTracker = new ReferencedAssembliesTracker(_workingDirectory);
            var refStackReport = referenceTracker.Run();

            //consolidate
            var fullStackReport = new StackReport
            {
                Results = vsStackReport.Results.Union(refStackReport.Results).ToList(),
                Errors = vsStackReport.Errors.Union(refStackReport.Errors).ToList()
            };

            if (fullStackReport.Errors.ToList().Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("Unknown files found: {0}", fullStackReport.Errors.Count());
                fullStackReport.Errors.ForEach(f =>
                {
                    Console.WriteLine("\\t{0}", f);
                });
                Console.ForegroundColor = ConsoleColor.White;
            }

            string fileName = @"C:\Users\rddag\Desktop\test.csv";
            var stackSink = new CsvStackReportSink(fileName, fullStackReport.Results);
            stackSink.Flush();

            Console.Read();
        }
    }
}
