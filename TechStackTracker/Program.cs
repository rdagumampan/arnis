using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechStackTracker.Sinks;
using TechStackTracker.Trackers;

namespace TechStackTracker
{
    class Program
    {
        private static string _workingDirectory = @"C:\Users\rddag\DSS";

        static void Main(string[] args)
        {
            Console.Write("Press key to run...");

            var vsTracker = new VisualStudioStackTracker(_workingDirectory);
            //var stackReport = vsTracker.Run();

            //if (stackReport.Errors.ToList().Any())
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine();
            //    Console.WriteLine("Unknown files found: {0}", stackReport.Errors.Count());
            //    stackReport.Errors.ForEach(f =>
            //    {
            //        Console.WriteLine("\\t{0}", f);
            //    });
            //    Console.ForegroundColor = ConsoleColor.White;
            //}

            var referenceTracker = new ReferencedAssembliesTracker(_workingDirectory);
            var stackReport2 = referenceTracker.Run();

            //string fileName = @"C:\Users\rddag\Desktop\test.csv";
            //var stackSink = new CsvStackReportSink(fileName, stackReport.Results);
            //stackSink.Flush();

            Console.Read();
        }
    }
}
