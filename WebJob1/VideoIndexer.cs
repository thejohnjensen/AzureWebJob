using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;

namespace WebJob1
{
    class VideoIndexer
    {
        public static void ProcessVideoIndexing(string videoUrl)
        {
            var apiUrl = "https://api.videoindexer.ai";
            var accountId = "21f29e8d-4b12-4a3d-b202-c6255d6051e8";
            var location = "trial";
            var apiKey = "78a4abff5d8b4f898a72af6e31637b07";

            System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;

            // create the http client
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            // obtain account access token
            var accountAccessTokenRequestResult = client.GetAsync($"{apiUrl}/auth/{location}/Accounts/{accountId}/AccessToken?allowEdit=true").Result;
            var accountAccessToken = accountAccessTokenRequestResult.Content.ReadAsStringAsync().Result.Replace("\"", "");

            client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");

            // upload a video
            var content = new MultipartFormDataContent();
            Console.WriteLine("Uploading...");
            //var videoUrl = "https://topcs.blob.core.windows.net/public/Overview-of-the-Microsoft-AI-School_high.mp4"; // replace with the video url 
            var testString1 = string.Format($"{apiUrl}/{location}/Accounts/{accountId}/Videos?name=some_name&description=some_description&privacy=private&partition=some_partition&videoUrl={videoUrl}");
            var testString = string.Format($"{apiUrl}/{location}/Accounts/{accountId}/Videos?accessToken={accountAccessToken}&name=some_name&description=some_description&privacy=private&partition=some_partition&videoUrl={videoUrl}");
            var uploadRequestResult = client.PostAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos?accessToken={accountAccessToken}&name=some_name&description=some_description&privacy=private&partition=some_partition&videoUrl={videoUrl}", content).Result;
            var uploadResult = uploadRequestResult.Content.ReadAsStringAsync().Result;

            // get the video id from the upload result
            string videoId = JsonConvert.DeserializeObject<dynamic>(uploadResult)["id"];
            Console.WriteLine("Uploaded");
            Console.WriteLine("Video ID: " + videoId);

            // obtain video access token            
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            var videoTokenRequestResult = client.GetAsync($"{apiUrl}/auth/{location}/Accounts/{accountId}/Videos/{videoId}/AccessToken?allowEdit=true").Result;
            var videoAccessToken = videoTokenRequestResult.Content.ReadAsStringAsync().Result.Replace("\"", "");

            client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key"); //workaround

            // wait for the video index to finish
            while (true)
            {
                Thread.Sleep(10000);

                var videoGetIndexRequestResult = client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{videoId}/Index?accessToken={videoAccessToken}&language=English").Result;
                var videoGetIndexResult = videoGetIndexRequestResult.Content.ReadAsStringAsync().Result;

                string processingState = JsonConvert.DeserializeObject<dynamic>(videoGetIndexResult)["state"];

                //Console.WriteLine();
                Console.WriteLine("State:");
                Console.WriteLine(processingState);

                // job is finished
                if (processingState != "Uploaded" && processingState != "Processing")
                {
                    //Console.WriteLine();
                    Console.WriteLine("Full JSON:");
                    Console.WriteLine(videoGetIndexResult);
                    Console.WriteLine("\n");

                    // get transcript
                    var transcript = JsonConvert.DeserializeObject<dynamic>(videoGetIndexResult)["videos"][0]["insights"]["transcript"];
                    var text = "";
                    for (int i = 0; i < transcript.Count; i++)
                    {
                        text += transcript[i]["text"];
                    }
                    Console.WriteLine(text);
                    Functions.PutInBlob(text);
                    break;
                }

            }
            /*
            // search for the video
            var searchRequestResult = client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/Search?accessToken={accountAccessToken}&id={videoId}").Result;
            var searchResult = searchRequestResult.Content.ReadAsStringAsync().Result;
            //Console.WriteLine();
            Console.WriteLine("Search:");
            Console.WriteLine(searchResult);

            // get insights widget url
            var insightsWidgetRequestResult = client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{videoId}/InsightsWidget?accessToken={videoAccessToken}&widgetType=Keywords&allowEdit=true").Result;
            var insightsWidgetLink = insightsWidgetRequestResult.Headers.Location;
            Console.WriteLine("Insights Widget url:");
            Console.WriteLine(insightsWidgetLink);

            // get player widget url
            var playerWidgetRequestResult = client.GetAsync($"{apiUrl}/{location}/Accounts/{accountId}/Videos/{videoId}/PlayerWidget?accessToken={videoAccessToken}").Result;
            var playerWidgetLink = playerWidgetRequestResult.Headers.Location;

            //Console.WriteLine();
            Console.WriteLine("Player Widget url:");
            Console.WriteLine(playerWidgetLink);

            // get transcript
            // var transcript = JsonConvert.DeserializeObject<dynamic>(videoGe)
            */
        }
    }
}