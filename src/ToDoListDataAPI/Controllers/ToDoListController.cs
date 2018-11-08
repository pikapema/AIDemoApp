using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web.Http;
using ToDoListDataAPI.Models;
using System.Threading.Tasks;
using ToDoListDataAPI.AIHelpers.CognitiveServices;
using System.Net.Http;
using System.Globalization;
using System.Net.Http.Headers;

namespace ToDoListDataAPI.Controllers
{
    public class ToDoListController : ApiController
    {
        // Uncomment following lines for service principal authentication
        //private static string trustedCallerClientId = ConfigurationManager.AppSettings["todo:TrustedCallerClientId"];
        //private static string trustedCallerServicePrincipalId = ConfigurationManager.AppSettings["todo:TrustedCallerServicePrincipalId"];

        private static Dictionary<int, ToDoItem> mockData = new Dictionary<int, ToDoItem>();
        private static readonly HttpClient client = new HttpClient();

        static ToDoListController()
        {
            mockData.Add(0, new ToDoItem { ID = 0, Owner = "*", Description = "feed the dog", CognitiveSentimentScore = 0.5, MlNetSentimentScore = 0.5, MlCustomSentimentScore = 0.5 });
            mockData.Add(1, new ToDoItem { ID = 1, Owner = "*", Description = "take the dog on a walk", CognitiveSentimentScore = 0.5, MlNetSentimentScore = 0.5, MlCustomSentimentScore = 0.5});
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
        public IEnumerable<ToDoItem> Get(string owner)
        {
            CheckCallerId();

            return mockData.Values.Where(m => m.Owner == owner || owner == "*");
        }

        // GET: api/ToDoItemList/5
        public ToDoItem GetById(string owner, int id)
        {
            CheckCallerId();

            return mockData.Values.Where(m => (m.Owner == owner || owner == "*" ) && m.ID == id).First();
        }
        // POST: api/ToDoItemList
        public async Task Post(ToDoItem todo)
        {
            CheckCallerId();

            //Check Sentiment with Cognitive Services
            await CognitiveServicesText.SentimentAnalysis(todo);

            //Call .NET Core project for ML.NET prediction
            var values = new Dictionary<string, string>
            {
               { "Text", todo.Description }
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
                                                "tweet_text", todo.Description
                                            },
                                }
                            }
                        },
                    },
                GlobalParameters = new Dictionary<string, string>()
                {
                }
            };

            const string apiKey = "Key"; // Replace this with the API key for the web service 
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

        public void Put(ToDoItem todo)
        {
            CheckCallerId();

            ToDoItem xtodo = mockData.Values.First(a => (a.Owner == todo.Owner || todo.Owner == "*") && a.ID == todo.ID);
            if (todo != null && xtodo != null)
            {
                xtodo.Description = todo.Description;
            }
        }

        // DELETE: api/ToDoItemList/5
        public void Delete(string owner, int id)
        {
            CheckCallerId();

            ToDoItem todo = mockData.Values.First(a => (a.Owner == owner || owner == "*") && a.ID == id);
            if (todo != null)
            {
                mockData.Remove(todo.ID);
            }
        }
    }
}

