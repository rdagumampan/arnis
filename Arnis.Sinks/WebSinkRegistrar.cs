using System.Net;
using Arnis.Core;
using RestSharp;

namespace Arnis.Sinks
{
    public class WebSinkRegistrar : ISinkRegistrar
    {
        private readonly string serviceUri = "http://arnisapi.azurewebsites.net";
        //private readonly string serviceUri = "http://localhost:5000";

        public Registration Register(string emailAddress)
        {
            var serviceClient = new RestClient(serviceUri);

            var request = new RestRequest(@"api/accounts", Method.POST);
            request.AddJsonBody(new
            {
                email = emailAddress
            });
            request.RequestFormat = DataFormat.Json;

            var response = serviceClient.Post<Registration>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                ConsoleEx.Error($"Registration failed.");
                ConsoleEx.Error($"Status: {response.StatusCode}, Response: {response.Content}");
                return null;
            }
        }
    }
}