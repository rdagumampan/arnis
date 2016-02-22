using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Arnis.Core;
using RestSharp;

namespace Arnis.Sinks
{
    public class AzureApiSink : ISink
    {
        private string serviceUri;

        public AzureApiSink()
        {
            serviceUri = "http://arnis.azurewebsites.net";
        }

        public void Flush(Workspace workspace)
        {
            ConsoleEx.Info($"Running sink: {this.GetType().Name}");

            var serviceClient = new RestClient(serviceUri);

            var request = new RestRequest(@"api\workspace", Method.POST);
            request.AddJsonBody(workspace);
            request.RequestFormat = DataFormat.Json;

            var response = serviceClient.Post(request);
            if(response.StatusCode == HttpStatusCode.Created)
            {
                ConsoleEx.Ok("Workspace analysis report published.");
                string location = response.Headers.FirstOrDefault(h=> h.Name == "Location").Value.ToString().ToLower();
                ConsoleEx.Ok($"Check out {location}");
            }
            else
            {
                ConsoleEx.Error($"Workspace not published to target sink.");
                ConsoleEx.Error($"Status: {response.StatusCode}, Response: {response.Content}");
            }
        }
    }
}
