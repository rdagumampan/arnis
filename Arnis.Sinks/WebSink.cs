using System.Linq;
using System.Net;
using Arnis.Core;
using RestSharp;

namespace Arnis.Sinks
{
    public class WebSink : ISink
    {
        private readonly string serviceUri = "http://arnis.azurewebsites.net";
        //private readonly string serviceUri = "http://localhost:5000";

        public WebSink()
        {
        }

        public string Name => this.GetType().Name;
        public string Description => "Sinks the tracker results into Arnis web api endpoint.";

        public void Flush(Workspace workspace)
        {
            ConsoleEx.Info($"Running sink: {this.GetType().Name}");

            var serviceClient = new RestClient(serviceUri);

            var request = new RestRequest(@"api\workspace", Method.POST);
            request.AddJsonBody(workspace);
            request.RequestFormat = DataFormat.Json;

            var response = serviceClient.Post(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                ConsoleEx.Ok("Workspace analysis report published.");
                string location = response.Headers.FirstOrDefault(h=> h.Name == "Location").Value.ToString().ToLower();
                ConsoleEx.Ok($"Visit {location}");
            }
            else
            {
                ConsoleEx.Error($"Workspace not published to target sink.");
                ConsoleEx.Error($"Status: {response.StatusCode}, Response: {response.Content}");
            }
        }
    }
}
