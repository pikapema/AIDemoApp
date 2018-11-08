using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web.Http;
using SentimentDataAPI.Models;
using System.Threading.Tasks;
using SentimentDataAPI.AIHelpers.CognitiveServices;
using System.Net.Http;
using System.Globalization;
using System.Net.Http.Headers;

namespace SentimentDataAPI.Controllers
{
    public class SentimentController : ApiController
    {
        // Uncomment following lines for service principal authentication
        //private static string trustedCallerClientId = ConfigurationManager.AppSettings["todo:TrustedCallerClientId"];
        //private static string trustedCallerServicePrincipalId = ConfigurationManager.AppSettings["todo:TrustedCallerServicePrincipalId"];

        private static Dictionary<int, SentimentItem> mockData = new Dictionary<int, SentimentItem>();
        private static readonly HttpClient client = new HttpClient();

        static SentimentController()
        {
            mockData.Add(0, new SentimentItem { ID = 0, Text = "I'm so happy", CognitiveSentimentScore = 1, MlNetSentimentScore = 1, MlCustomSentimentScore = 1 });
            mockData.Add(1, new SentimentItem { ID = 1, Text = "I hate mondays!", CognitiveSentimentScore = 0, MlNetSentimentScore = 0, MlCustomSentimentScore = 0});
        }

        private static void CheckCallerId()
        {
            // Uncomment following lines for service principal authentication
            //string currentCallerClientId = ClaimsPrincipal.Current.FindFirst("appid").Value;
            //string currentCallerServicePrincipalId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            //if (currentCallerClientId != trustedCallerClientId || currentCallerServicePrincipalId != trustedCallerServicePrincipalId)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The appID or service principal ID is not the expected value." });
            //}
        }

        // GET: api/ToDoItemList
        public IEnumerable<SentimentItem> Get(string owner)
        {
            CheckCallerId();

            return mockData.Values.ToList();
        }

       
        // POST: api/ToDoItemList
        public async Task Post(SentimentItem todo)
        {
            CheckCallerId();

            //Check Sentiment with Cognitive Services
            await CognitiveServicesText.SentimentAnalysis(todo);

            //Call .NET Core project for ML.NET prediction
            var values = new Dictionary<string, string>
            {
               { "Text", todo.Text }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("http://localhost:60146/api/todo", content);
            string responseString = await response.Content.ReadAsStringAsync();

            
            double result;
            if (Double.TryParse(responseString, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out result))
            {
                todo.MlNetSentimentScore = result;
                System.Diagnostics.Debug.WriteLine(responseString + " --> " + result);
            }
            else
                System.Diagnostics.Debug.WriteLine("Unable to parse " + responseString);
            

            //Call Kristinas Model
            var scoreRequest = new
            {
                Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "tweet_text", todo.Text
                                            },
                                }
                            }
                        },
                    },
                GlobalParameters = new Dictionary<string, string>()
                {
                }
            };

            const string apiKey = "/OQbQr+MALLJTi8qcydUTlW49mSYUP/MSxo/4tLgeI6QuRvFeonb1QWMj8JibGsXocRERDK6SnNQi7vKwuQzuw=="; // Replace this with the API key for the web service 
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);           
            var response2 = await client.PostAsJsonAsync("https://ussouthcentral.services.azureml.net/workspaces/86a7b45bf031449c887345c7a24a75a4/services/ac7138a3b38647c4bfeb403fd14d9e24/execute?api-version=2.0&format=swagger", scoreRequest);
            ModelResponse responseModel = await response2.Content.ReadAsAsync<ModelResponse>();
            var score = responseModel.Results.output1[0].Score;
            var sentiment = responseModel.Results.output1[0].Sentiment;

            System.Diagnostics.Debug.WriteLine("Custom model sentiment " + sentiment + ", score: " + score);

            if ( sentiment.Equals("positive") )
                todo.MlCustomSentimentScore = 1;
            //{"Results":{"output1":[{"Sentiment":"positive","Score":"0.709294199943542"}]}}            

            todo.ID = mockData.Count > 0 ? mockData.Keys.Max() + 1 : 1;
            mockData.Add(todo.ID, todo);
        }
    }
}

