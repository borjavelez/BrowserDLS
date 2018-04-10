using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http.Headers;
using System.Text;
using System.Collections.Generic;

namespace LoadBalancer
{
    public static class LoadBalancer
    {

        // To test this function locally it is needed to install Azure Functions Core Tools.
        //https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local

        [FunctionName("LoadBalancer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string URL_MONITOR = "http://localhost:5128/api/LogMonitors";

            ///@--------START OF THE ROUND-ROBIN LOGIC OF THE LOAD BALANCER ------------

            int flag = 0;
            string urlChosen = "";
            string urlCloneInstance1 = "http://localhost:7303/api/Terms";
            string urlCloneInstance2 = "http://localhost:7303/api/Terms";

            List<string> urlsAllCloneInstances = new List<string>();
            urlsAllCloneInstances.Add(urlCloneInstance1);
            urlsAllCloneInstances.Add(urlCloneInstance2);

            //For adding new instances we need to add here above this line the url to the created list "urlsAllCloneInstances".
            int numberCloneInstances = urlsAllCloneInstances.Count();
            urlChosen = urlsAllCloneInstances[flag % numberCloneInstances]; //Round-robin
            // Update the value of the flag
            flag = flag < int.MaxValue ? flag++ : 0; //Preventing for overflow.

            ///@--------END OF THE ROUND-ROBIN LOGIC OF THE LOAD BALANCER --------------


            string valueTerm = "";
            string path = "";

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();
            valueTerm = data?.Value;
            path = data?.Path;
            HttpClient _httpClient = new HttpClient();

            string str = "{\"Value\":\"" + valueTerm + "\",\"Path\":\"" + path + "\",\"Time\":\"" + System.DateTime.Now.TimeOfDay.ToString() + "\"}";

            _httpClient.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ///@--------START OF THE POST ASYNC OPERATION ------------
            using (var content = new StringContent(str, Encoding.UTF8, "application/json"))
            {
                var result = await _httpClient.PostAsync($"{urlChosen}", content).ConfigureAwait(false);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return req.CreateResponse(HttpStatusCode.OK, "Term inserted");
                }
                else
                {
                    // Something wrong happened
                    ///@--------START OF MONITORING ------------

                    string resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    // ... post to Monitor
                    string logstr = "{\"Origin\":\"LoadBalancer\",\"Time\":\"" + System.DateTime.Now.TimeOfDay.ToString() + "\",\"Message\":\"" + resultContent + "\"} ";
                    // Put method with error handling
                    using (var contentLog = new StringContent(logstr, Encoding.UTF8, "application/json"))
                    {
                        var resultLog = await _httpClient.PostAsync($"{URL_MONITOR}", contentLog).ConfigureAwait(false);

                    }
                    
                    ///@--------END OF MONITORING ------------
                }
            }
            ///@--------END OF THE POST ASYNC OPERATION ------------

            return valueTerm == null ? req.CreateResponse(HttpStatusCode.BadRequest, "Bad request") : req.CreateResponse(HttpStatusCode.OK, "Term inserted");
        }
    }
}
