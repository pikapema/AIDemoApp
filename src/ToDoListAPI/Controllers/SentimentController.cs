using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using SentimentAPI.Filters;
using System.Configuration;
using SentimentAPI.Models;

namespace SentimentAPI.Filters
{
    using System.Web.Http.Filters;

    public class HttpOperationExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is Microsoft.Rest.HttpOperationException ex)
            {
                context.Response = ex.Response;
            }
        }
    }
}

namespace SentimentAPI.Controllers
{
    [HttpOperationExceptionFilterAttribute]
    public class SentimentController : ApiController
    {

        private static SentimentDataAPI NewDataAPIClient()
        {
            var client = new SentimentDataAPI(new Uri(ConfigurationManager.AppSettings["sentimentDataAPIURL"]));
            // Uncomment following line and entire ServicePrincipal.cs file for service principal authentication of calls to SentimentDataAPI
            //client.HttpClient.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", ServicePrincipal.GetS2SAccessTokenForProdMSA().AccessToken);
            return client;
        }

        // GET: api/SentimentItemList
        public async Task<IEnumerable<SentimentItem>> Get()
        {
            // Uncomment following line in each action method for user authentication
            //owner = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;
            using (var client = NewDataAPIClient())
            {
                var results = await client.Sentiment.GetByIdByOwnerAndId;
                return results.Select(m => new SentimentItem
                {
                    Text = m.Text,
                    ID = (int)m.ID,
                    CognitiveSentiment = m.CognitiveSentiment,
                    MlNetSentiment = m.MlNetSentiment,
                    MlCustomSentiment = m.MlCustomSentiment
                });
            }
        }
        // POST: api/ToDoItemList
        public async Task Post(SentimentItem todo)
        {
            using (var client = NewDataAPIClient())
            {
                await client.Sentiment.PostByTodoAsync(new SentimentItem
                {
                    Text = todo.Text,
                    ID = todo.ID,
                    CognitiveSentiment = todo.CognitiveSentiment,
                    MlNetSentiment = todo.MlNetSentiment,
                    MlCustomSentiment = todo.MlCustomSentiment
                });
            }
        }
    }
}

