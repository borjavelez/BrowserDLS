using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace Browser.Utilities
{
    class Crawler
    {

        private static readonly HttpClient _httpClient = new HttpClient();

        List<String> words = new List<string>();

        private static int? CURRENT_NUM_LOAD_BALANCERS;

        //private static readonly string DEFAULT_URI_LOAD_BALANCERS = "https://Xloadbalancerbrowserapi.azurewebsites.net/api/postDataToApi";
        private static readonly string DEFAULT_URI_LOAD_BALANCERS = "http://localhost:7071/api/LoadBalancer";

        private static readonly int DEFAULT_NUM_LOAD_BALANCERS = 1;

        //private static List<string> LOAD_BALANCERS_LIST = new List<string>();

        //private static readonly string URL_GET_NUMBER_BALANCERS_COMPONENT = "https://browserapinumloadbal.azurewebsites.net/api/GetNumLoadBalancers";

        private static readonly string URL_MONITOR = "http://localhost:5128/api/LogMonitors";

        List<String> thesaurus;

        public Crawler()
        {
            //updateLoadBalancersListAsync();
            //Post(DEFAULT_URI_LOAD_BALANCERS, "DekstopApp", "Test");
        }


        public void indexFilesAndDirectories()
        {
            readThesaurus();
            String path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Docs";
            //With this method all the .txt files in a directory are found recursively.
            string[] files = Directory.GetFiles(path, "*.txt*", SearchOption.AllDirectories);
            char[] delimiterChars = { ' ', ',', '.', ':', '\t', '?', '!', ';', '-' };
            foreach (string fileName in files)
            {
               
                string[] readText = File.ReadAllLines(fileName);
                foreach (string line in readText)
                {
                    string[] wordsPerLine = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string word in wordsPerLine)
                    {
                        //Here is where we check if the word is contained in the thesaurus or not, and if yes we update 
                        //the database 

                        //TODO
                        foreach (string value in thesaurus)
                        {
                            if (value.Equals(word))
                            {
                                //Insert term into list
                                words.Add(word);
                            }
                        }

                    }


                    //Send terms to balancer via POST
                    postWordsToLoadBalancers(path);
                }
            }

        }

        public void sendLogTest()
        {
            SendLog("Overlapping test: Started");
            Thread.Sleep(10000);
            SendLog("Overlapping test: Finished");
        }

        public List<string> readThesaurus()
        {
            String thesaurusPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Internal\\Thesaurus.txt";
            string[] readText = File.ReadAllLines(thesaurusPath);
            List<string> list = new List<string>(readText);
            thesaurus = list;
            return list; 
        }

        //private void updateLoadBalancersListAsync()
        //{
        //    try
        //    {
        //        CURRENT_NUM_LOAD_BALANCERS = GetNumLoadBalancersAsync().Result;
        //    }
        //    catch
        //    {
        //        CURRENT_NUM_LOAD_BALANCERS = DEFAULT_NUM_LOAD_BALANCERS;
        //    }

        //    for (int i = 1; i <= CURRENT_NUM_LOAD_BALANCERS; i++)
        //    {
        //        string url = DEFAULT_URI_LOAD_BALANCERS.Replace("X", i.ToString());
        //        LOAD_BALANCERS_LIST.Add(url);
        //    }
        //}

        //private async Task<int> GetNumLoadBalancersAsync()
        //{
        //    var _httpClient = new HttpClient();

        //    using (var result = await _httpClient.GetAsync(URL_GET_NUMBER_BALANCERS_COMPONENT).ConfigureAwait(false))
        //    {
        //        string content = await result.Content.ReadAsStringAsync();
        //        return int.Parse(content);
        //    }
        //}

        private void postWordsToLoadBalancers(string path)
        {
            for (int i = 0; i < words.Count; i++)
            {
                Post(DEFAULT_URI_LOAD_BALANCERS, words.ElementAt(i), path);
                //Post(LOAD_BALANCERS_LIST.ElementAt(i % CURRENT_NUM_LOAD_BALANCERS.Value), words.ElementAt(i), path).ConfigureAwait(false);
            }

        }

        public static async Task Post(string urlBalancer, string value, string path)
        {
            string finalPath = path.Replace("\\", "/");

            string str = "{\"Value\":\"" + value + "\",\"Path\":\"" + finalPath + "\"}";

            _httpClient.DefaultRequestHeaders
             .Accept
             .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Put method with error handling
            using (var content = new StringContent(str, Encoding.UTF8, "application/json"))
            {
                var result = await _httpClient.PostAsync($"{urlBalancer}", content).ConfigureAwait(false);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
                else
                {
                    // Something wrong happened
                    string resultContent = result.ReasonPhrase;
                    //resultContent = URLEncoder.encode(resultCo, "UTF-8");
                    // Send log to monitor

                    //await SendLog(resultContent);
                }
            }
        }


        public static async Task SendLog(string postData)
        {
            string str = "{\"Origin\":\"Desktop application\",\"Time\":\"" + DateTime.Now.TimeOfDay.ToString() + "\",\"Message\":\"" + postData + "\"} ";

            _httpClient.DefaultRequestHeaders
             .Accept
             .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Put method with error handling
            using (var content = new StringContent(str, Encoding.UTF8, "application/json"))
            {
                var result = await _httpClient.PostAsync($"{URL_MONITOR}", content).ConfigureAwait(false);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }

            }
        }

        public class RootObject
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public string Path { get; set; }
            public string Time { get; set; }
        }

        public List<String> selectValue(String text)
        {
            String json = new WebClient().DownloadString("http://localhost:7303/api/Terms?Value=" + text);
            var model = JsonConvert.DeserializeObject<List<RootObject>>(json);

            List<String> result = new List<String>();
            //var json = JsonParse.FromJson(new WebClient().DownloadString("http://localhost:7303/api/Terms/1"));
            //var json = JsonParse.FromJson("{\"Id\":16,\"Value\":\"rain\",\"Path\":\"C:/ Users / Borja / Desktop / Browser / Browser / Docs\",\"Time\":\"11:56:26.5672634\"}]");
            if (model.Count > 0)
            {
                for (int i = 0; i < model.Count; i++)
                {
                    result.Add(model[i].Path);
                }
                return result;
            } else
            {
                result.Add("No results found");
                return result;
            }
        }
    }
}