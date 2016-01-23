using System;
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
            var stackReport = vsTracker.Run();

            var stackSink = new CsvStackReportSink(stackReport);
            stackSink.Flush();

            Console.Read();
        }
    }
}
