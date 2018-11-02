using System;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System.Collections.Generic;
using Microsoft.Rest;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ToDoListDataAPI.AIHelpers
{

    public static class CognitiveServicesText
    {       

        /// </summary>
        class ApiKeyServiceClientCredentials : ServiceClientCredentials
        {
            string subscriptionKey = ""; //Insert your Text Anaytics subscription key
            public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                return base.ProcessHttpRequestAsync(request, cancellationToken);
            }
        }

        // Create a client.
        static ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials())
        {
            Endpoint = "https://westus.api.cognitive.microsoft.com"
        }; //Replace 'westus' with the correct region for your Text Analytics subscription

       
        public static double SentimentAnalysis(string text)
        {
            //Detect language
            var result = client.DetectLanguageAsync(new BatchInput(
                    new List<Input>()
                        {
                          new Input("1", text)
                    })).Result;

            string language = result.Documents[1].DetectedLanguages[1].Name;


            SentimentBatchResult result2 = client.SentimentAsync(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput(language, "1", text)
                        })).Result;

            return (double)result2.Documents[1].Score;

        }
    }
}