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
        //arnis /ws:"c:\github\arnis" /sf:"c:\skip.txt"
        static void Main(string[] args)
        {
            try
            {
                Regex regex = new Regex(@"/(?<name>.+?):(?<val>.+)");
                Dictionary<string, string> settings = args.Select(s =>
                    regex.Match(s)).Where(m => m.Success)
                    .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);

                //validate working folder /ws is required parameter
                string ws = settings.SingleOrDefault(s => s.Key == "ws").Value;
                if (null == ws)
                {
                    throw new ArgumentException("Missing parameter", "ws");
                }

                if (settings.Any())
                {
                    ConsoleEx.Info("Running Arnis.NET on ff settings:");
                    settings.ToList().ForEach(s =>
                    {
                        ConsoleEx.Info($"\t{s.Key}: {s.Value}");
                    }
                    );
                }

                //validate skip folder /sf
                var skipList = new List<string>();
                string sf = settings.SingleOrDefault(s => s.Key == "sf").Value;
                if (null == sf)
                {
                    ConsoleEx.Warn("No skip file (skip.data) defined. /sf:<skipfile>");
                }

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

                ConsoleEx.Info("walk around, this may take some time...");

                //run all trackers
                var trackerResult = TrackDependencies(ws, skipList);

                //run all sinks
                string worspaceName = new DirectoryInfo(ws.TrimEnd(Path.DirectorySeparatorChar)).Name;
                var workspace = new Workspace
                {
                    Name = worspaceName.ToLower(),
                    Solutions = trackerResult.Solutions
                };

                SinkDependencies(workspace);
            }
            catch (Exception ex)
            {
                ConsoleEx.Error("Arnis.NET breaks ;(. \n" + ex.Message);
            }            
        }

        private static TrackerResult TrackDependencies(string workspace, List<string> skipList)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var assemblies = GetTrackerAssemblies(path);
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
                Solutions = trackerResultRaw.SelectMany(t => t.Solutions).ToList(),
                Logs = trackerResultRaw.SelectMany(t => t.Logs).ToList()
            };

            if (trackerResult.Logs.ToList().Any())
            {
                ConsoleEx.Error($"Unknown files found: {trackerResult.Logs.Count()}");
                trackerResult.Logs.ForEach(f => { ConsoleEx.Error($"\t{f}"); });
            }
            return trackerResult;
        }

        private static IEnumerable<Assembly> GetTrackerAssemblies(string path)
        {
            return Directory.GetFiles(path, "*.dll")
                .Where(f =>
                    Path.GetFileNameWithoutExtension(f).Contains(".Core")
                    || Path.GetFileNameWithoutExtension(f).Contains(".Trackers")
                )
                .Select(a => Assembly.LoadFile(a));
        }

        private static void SinkDependencies(Workspace workspace)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var assemblies = GetSinkAssemblies(path);
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

        private static IEnumerable<Assembly> GetSinkAssemblies(string path)
        {
            return Directory.GetFiles(path, "*.dll")
                .Where(f=> 
                    Path.GetFileNameWithoutExtension(f).Contains(".Core")
                    || Path.GetFileNameWithoutExtension(f).Contains(".Sinks")
                )
                .Select(a => Assembly.LoadFile(a));
        }
    }
}
