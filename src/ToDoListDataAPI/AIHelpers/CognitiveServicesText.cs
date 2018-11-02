﻿using System;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System.Collections.Generic;
using Microsoft.Rest;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ToDoListDataAPI.Models;

namespace ToDoListDataAPI.AIHelpers
{

    public static class CognitiveServicesText
    {       

        public static async Task SentimentAnalysis(ToDoItem item)
        {
            ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials("Key goes here"))
            {
                Endpoint = "https://westeurope.api.cognitive.microsoft.com/text/analytics/v2.0"
            };

            //Detect language
            List<Input> list = new List<Input>()
                        {
                          new Input("1", item.Description)
                    };
            BatchInput input = new BatchInput();
            input.Documents = list;
            try
            {
                LanguageBatchResult result = await client.DetectLanguageAsync(input);
                var docs = result.Documents;

                string language = result.Documents[0].DetectedLanguages[0].Name;


                SentimentBatchResult result3 = client.SentimentAsync(
                        new MultiLanguageBatchInput(
                            new List<MultiLanguageInput>()
                            {
                          new MultiLanguageInput(language, "1", item.Description)
                            })).Result;

                item.Sentiment = (double)result3.Documents[0].Score;
            }
            catch (Exception e) { }

        }
    }
}