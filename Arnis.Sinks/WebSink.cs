using System.Linq;
using System.Net;
using Arnis.Core;
using RestSharp;

namespace Arnis.Sinks
{
    public class WebSink : ISink
    {
        private readonly string serviceUri = "http://arnisapi.azurewebsites.net";
        //private readonly string serviceUri = "http://localhost:5000";

        public WebSink()
        {
        }

        public string Name => this.GetType().Name;
        public string Description => "Sinks the tracker results into Arnis web api endpoint.";

        public void Flush(Workspace workspace)
        {
            ConsoleEx.Info($"Running sink: {this.GetType().Name}");

            var request = new RestRequest(@"api/workspaces", Method.POST);
            request.AddJsonBody(workspace);
            request.RequestFormat = DataFormat.Json;

            var restClient = new RestClient(serviceUri);
            var response = restClient.Post(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                ConsoleEx.Ok("Workspace analysis report published.");
                var locationHeader = response.Headers.FirstOrDefault(h=> h.Name == "Location");
                if (locationHeader != null)
                {
                    string workspaceLocation = locationHeader.Value.ToString().ToLower();
                    ConsoleEx.Ok($"Visit {workspaceLocation}");
                }
            }
            else
            {
                ConsoleEx.Error($"Workspace not published to target sink.");
                ConsoleEx.Error($"Status: {response.StatusCode}, Response: {response.Content}");
            }
        }
    }
}
