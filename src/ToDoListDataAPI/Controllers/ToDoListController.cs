using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web.Http;
using ToDoListDataAPI.Models;
using System.Threading.Tasks;
using ToDoListDataAPI.AIHelpers.CognitiveServices;
using ToDoListDataAPI.AIHelpers.MLNET;
using System.Net.Http;
using System.Globalization;

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
            mockData.Add(0, new ToDoItem { ID = 0, Owner = "*", Description = "feed the dog", CognitiveSentimentScore = 0.5, MlNetSentimentScore = 0.5 });
            mockData.Add(1, new ToDoItem { ID = 1, Owner = "*", Description = "take the dog on a walk", CognitiveSentimentScore = 0.5, MlNetSentimentScore = 0.5 });
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

            //Check Sentiment
            await CognitiveServicesText.SentimentAnalysis(todo);

            //Call .NET Core project for ML.NET prediction
            var values = new Dictionary<string, string>
            {
               { "Text", todo.Description }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("http://localhost:60146/api/todo", content);
            string responseString = await response.Content.ReadAsStringAsync();

            //double mlSentimentValue = MLNetTextSentiment.PredictSentiment(todo.Description);
            double result;
            if (Double.TryParse(responseString, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out result))
            {
                todo.MlNetSentimentScore = result;
                System.Diagnostics.Debug.WriteLine(responseString + " --> " + result);
            }
            else
                System.Diagnostics.Debug.WriteLine("Unable to parse " + responseString);

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

