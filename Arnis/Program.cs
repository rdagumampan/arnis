using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Arnis.Core;

namespace Arnis
{
    static class Program
    {
        //arnis /ws:"c:\github\arnis" /sf:"c:\skip.data"
        static void Main(string[] args)
        {
            try
            {
                Regex regex = new Regex(@"/(?<name>.+?):(?<val>.+)");
                Dictionary<string, string> settings = args.Select(s =>
                    regex.Match(s)).Where(m => m.Success)
                    .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);

                ConsoleEx.Info("Running Arnis.NET on ff settings:");
                settings.ToList().ForEach(s =>
                {
                    ConsoleEx.Info($"\t{s.Key}: {s.Value}");
                }
                );

                settings.Add("ws", @"C:\Users\rddag\Desktop\GitHub\arnis");
                settings.Add("sf", @"C:\Users\rddag\Desktop\teckstack.arnis.csv");

                //validate working folder /ws
                string ws = settings.SingleOrDefault(s => s.Key == "ws").Value;
                if (null == ws)
                {
                    throw new ArgumentException("Missing parameter", "ws");
                }

                //validate skip folder /sf
                var skipList = new List<string>();
                string sf = settings.SingleOrDefault(s => s.Key == "sf").Value;
                if (null == sf)
                {
                    ConsoleEx.Warn("No skip file (skip.data) defined. /sf:<skipfile>");
                }
                else
                {
                    var skipFile = Path.Combine(Environment.CurrentDirectory, "skip.data");
                    if (File.Exists(skipFile))
                    {
                        skipList = File.ReadAllLines(skipFile).ToList();

                        if (!skipList.Any())
                        {
                            ConsoleEx.Warn("Warning: Skip file is empty, but it's ok.");
                        }
                    }
                    else
                    {
                        ConsoleEx.Warn("Warning: Skip file doesn't exists, but we'll continue anyway");
                    }
                }

                ConsoleEx.Info("walk around, this may take some time...");

                //run all trackers
                var trackerResult = TrackDependencies(ws, skipList);

                //run all sinks
                SinkDependencies(trackerResult);
            }
            catch (Exception ex)
            {
                ConsoleEx.Error("Arnis.NET breaks ;(. \n" + ex.Message);
            }            
        }

        private static TrackerResult TrackDependencies(string workspace, List<string> skipList)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblies = Directory.GetFiles(path, "*.dll")
                .Where(f =>
                    Path.GetFileNameWithoutExtension(f).Contains(".Core")
                    || Path.GetFileNameWithoutExtension(f).Contains(".Trackers")
                    )
                .Select(a => Assembly.LoadFile(a));


            var trackers = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof (ITracker).IsAssignableFrom(p) && !p.IsInterface);

            var trackerTasks = trackers.Select(t =>
            {
                var tracker = Activator.CreateInstance(t) as ITracker;
                return Task.Factory.StartNew(() => tracker.Run(workspace, skipList));
            });

            Task.WaitAll(trackerTasks.ToArray());

            var trackerResultRaw = trackerTasks.Select(t => t.Result);
            var trackerResult = new TrackerResult
            {
                Results = trackerResultRaw.SelectMany(t => t.Results).ToList(),
                Errors = trackerResultRaw.SelectMany(t => t.Errors).ToList()
            };

            if (trackerResult.Errors.ToList().Any())
            {
                ConsoleEx.Error($"Unknown files found: {trackerResult.Errors.Count()}");
                trackerResult.Errors.ForEach(f => { ConsoleEx.Error($"\t{f}"); });
            }
            return trackerResult;
        }

        private static void SinkDependencies(TrackerResult trackerResult)
        {
            var workspace = new Workspace
            {
                Name = "arnisws",
                Solutions = trackerResult.Results
            };

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblies = Directory.GetFiles(path, "*.dll")
                .Where(f=> 
                    Path.GetFileNameWithoutExtension(f).Contains(".Core")
                    || Path.GetFileNameWithoutExtension(f).Contains(".Sinks")
                    )
                .Select(a => Assembly.LoadFile(a));

            var sinks = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ISink).IsAssignableFrom(p) && !p.IsInterface);

            var sinkTasks = sinks.Select(t =>
            {
                var tracker = Activator.CreateInstance(t) as ISink;
                return Task.Factory.StartNew(() => tracker.Flush(workspace));
            });

            Task.WaitAll(sinkTasks.ToArray());
       }
    }
}
