using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arnis.Core;

namespace Arnis.Sinks
{
    public class AzureApiSink : ISink
    {
        public void Flush(Workspace workspace)
        {
            //var data = Newtonsoft.Json.JsonConvert.SerializeObject(workspace);

            //var baseUri = "http://localhost:5000";
            //var serviceClient = new RestClient(baseUri);

            //var serviceRequest = new RestRequest(@"api\workspace", Method.POST);
            //serviceRequest.AddJsonBody(workspace);
            //serviceRequest.RequestFormat = DataFormat.Json;

            //var restResponse = serviceClient.Post(serviceRequest);
        }
    }
}
