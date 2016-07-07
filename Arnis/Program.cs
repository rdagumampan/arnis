using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Arnis.Core;
using Arnis.Sinks;

namespace Arnis
{
    static class Program
    {
        //example 1: simple
        //>arnis /ws:"c:\github\arnis"
        //>arnis /ws:"c:\github\arnis" /sf:"c:\skip.txt"

        //example 1: publish into web sink
        //>arnis /web /r /u:email@website.com
        //>arnis /web /apk:"XXXX-XXXX" /ws:"c:\github\arnis"

        static void Main(string[] args)
        {
            try
            {
                var settings = args.Select(a =>
                {
                    var parameter = a.Split('=');
                    return new
                    {
                        Key = parameter[0].Replace("/", ""),
                        Value = parameter.Length == 2 ? parameter[1] : null
                    };
                }).ToDictionary(m => m.Key, m => m.Value);

                PrintSettings(settings);

                //capture /sf
                var skipList = GetSkipList(settings);

                //capture /web
                var slashWebExist = settings.ContainsKey("web");
                if (slashWebExist)
                {
                    //capture /web /r
                    var slashRExist = settings.ContainsKey("r");
                    if (slashRExist)
                    {
                        //capture /web /r /u
                        var slashUExist = settings.ContainsKey("u");
                        if (slashUExist)
                        {
                            var userName = settings.SingleOrDefault(s => s.Key == "u").Value;

                            var sinkRegistrar = new WebSinkRegistrar();
                            var registration = sinkRegistrar.Register(userName);

                            if (null != registration)
                            {
                                string response =
                                      $"{Environment.NewLine}API Key: {registration.ApiKey}"
                                    + $"{Environment.NewLine}Workspace: {registration.Workspace}"
                                    + $"{Environment.NewLine}Location: {registration.WorkspaceUri}"
                                    + $"{Environment.NewLine}Location: {registration.AccountUri}";

                                ConsoleEx.Ok($"Done! Please keep your API key secret.{response}");
                            }
                        }
                    }

                    //capture /web /apk /ws
                    var slashApkExist = settings.ContainsKey("apk");
                    if (slashApkExist)
                    {
                        var apiKey = settings.SingleOrDefault(s => s.Key == "apk").Value;

                        //validate working folder /ws is required parameter
                        string slashWs = settings.SingleOrDefault(s => s.Key == "ws").Value;
                        if (null == slashWs)
                        {
                            throw new ArgumentException("Missing parameter", "ws");
                        }

                        //run all trackers, at this point we have all the dependencies
                        var trackerResult = TrackDependencies(slashWs, skipList);

                        //create the workspace details
                        string worspaceName = new DirectoryInfo(slashWs.TrimEnd(Path.DirectorySeparatorChar)).Name;
                        var workspace = new Workspace
                        {
                            ApiKey = apiKey,
                            Name = worspaceName,
                            Solutions = trackerResult.Solutions,
                            Logs = trackerResult.Logs
                        };

                        var webSink = new WebSink();
                        webSink.Flush(workspace);
                    }
                }
                else
                {
                    //capture /ws
                    string slashWs = settings.SingleOrDefault(s => s.Key == "ws").Value;
                    if (null == slashWs)
                    {
                        throw new ArgumentException("Missing parameter", "ws");
                    }

                    //fall back to default csv as default sink 
                    //run all trackers, at this point we have all the dependencies
                    var trackerResult = TrackDependencies(slashWs, skipList);

                    //create the workspace details
                    string worspaceName = new DirectoryInfo(slashWs.TrimEnd(Path.DirectorySeparatorChar)).Name;
                    var workspace = new Workspace
                    {
                        Name = worspaceName.ToLower(),
                        Solutions = trackerResult.Solutions,
                        Logs = trackerResult.Logs
                    };

                    //run default sink
                    SinkDependencies("csv", workspace);
                }

            }
            catch (Exception ex)
            {
                ConsoleEx.Error("Arnis.NET breaks ;(. \n" + ex.Message);
            }
        }

        private static List<string> GetSkipList(Dictionary<string, string> settings)
        {
            var skipList = new List<string>();
            var slashSfExists = settings.ContainsKey("sf");
            if (slashSfExists)
            {
                var skipFile = Path.Combine(Environment.CurrentDirectory, "skip.data");
                if (File.Exists(skipFile))
                {
                    skipList = File.ReadAllLines(skipFile).ToList();

                    if (!skipList.Any())
                    {
                        ConsoleEx.Warn("Skip file is empty, but it's ok.");
                    }
                }
                else
                {
                    ConsoleEx.Warn("Skip file doesn't exists, but we'll continue anyway");
                }
            }

            return skipList;
        }

        private static void PrintSettings(Dictionary<string, string> settings)
        {
            if (settings.Any())
            {
                ConsoleEx.Info("Running Arnis.NET on ff settings:");
                settings.ToList().ForEach(s => { ConsoleEx.Info($"{s.Key}: {s.Value}"); });
            }
        }

        private static TrackerResult TrackDependencies(string workspace, List<string> skipList)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var assemblies = GetTrackerAssemblies(path);
            var trackers = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ITracker).IsAssignableFrom(p) && !p.IsInterface);

            var trackerTasks = trackers.Select(t =>
            {
                var tracker = Activator.CreateInstance(t) as ITracker;
                return Task.Factory.StartNew(() => tracker?.Run(workspace, skipList));
            });
            Task.WaitAll(trackerTasks.ToArray());

            var trackerResultRaw = trackerTasks.Select(t => t.Result).ToList();
            var trackerResult = new TrackerResult
            {
                Solutions = trackerResultRaw.SelectMany(t => t.Solutions).ToList(),
                Logs = trackerResultRaw.SelectMany(t => t.Logs).ToList()
            };

            if (trackerResult.Logs.ToList().Any())
            {
                var errors = trackerResult.Logs.Count(m => m.Contains("ERROR"));
                var warnings = trackerResult.Logs.Count(m => m.Contains("WARN"));
                ConsoleEx.Info($"Tracker found {errors} errors, {warnings} warnings");
            }
            return trackerResult;
        }

        private static IEnumerable<Assembly> GetTrackerAssemblies(string path)
        {
            return Directory.GetFiles(path, "*.dll")
                .Where(f =>
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f);
                    return fileNameWithoutExtension != null && 
                          (fileNameWithoutExtension.Contains(".Core") || fileNameWithoutExtension.Contains(".Trackers"));
                })
                .Select(Assembly.LoadFile);
        }

        private static void SinkDependencies(string sink, Workspace workspace)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var assemblies = GetSinkAssemblies(path);
            var sinks = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p =>
                    typeof(ISink).IsAssignableFrom(p)
                    && !p.IsInterface
                    && p.Name.ToLower() == $"{sink}sink".ToLower());

            var sinkTasks = sinks.Select(t =>
            {
                var tracker = Activator.CreateInstance(t) as ISink;
                return Task.Factory.StartNew(() => tracker?.Flush(workspace));
            });

            Task.WaitAll(sinkTasks.ToArray());
        }

        private static IEnumerable<Assembly> GetSinkAssemblies(string path)
        {
            return Directory.GetFiles(path, "*.dll")
                .Where(f =>
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f);
                    return fileNameWithoutExtension != null && 
                           (fileNameWithoutExtension.Contains(".Core") || fileNameWithoutExtension.Contains(".Sinks"));
                })
                .Select(Assembly.LoadFile);
        }
    }
}
