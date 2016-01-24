using System;
using System.Collections.Generic;
using NUnit.Framework;
using TechStackTracker.Sinks;

namespace TechStackTracker.UnitTests.Sinks
{
    [TestFixture]
    public class TestCsvStackReportSink
    {

        [Test]
        public void Test()
        {
            ////arrange
            //var report = new StackReport
            //{
            //    Results = new List<SolutionDependency>
            //    {
            //        new SolutionDependency {Name = "VisualStudio", Version = "14.0", Location = @"C:\App1.sln"},
            //        new SolutionDependency {Name = "VisualStudio", Version = "12.0", Location = @"C:\App2.sln"},
            //    }
            //};

            //string fileName = @"C:\test.csv";

            ////act
            //var sink = new CsvStackReportSink(fileName, report.Results);
            //sink.Flush();

            //assert
            throw new NotImplementedException();
        }
    }
}

