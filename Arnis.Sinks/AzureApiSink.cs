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
            var serviceClient = new RestClient(serviceUri);

            var serviceRequest = new RestRequest(@"api\workspaces", Method.POST);
            serviceRequest.AddJsonBody(workspace);
            serviceRequest.RequestFormat = DataFormat.Json;

            var restResponse = serviceClient.Post(serviceRequest);
            if(restResponse.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine("Workspace analysis published.");
            }
        }
    }
}
