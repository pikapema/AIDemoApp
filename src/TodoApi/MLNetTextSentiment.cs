using System;
using System.IO;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;
using Microsoft.ML;
using TodoApi.Models;

namespace TodoApi
{
    public static class MLNetTextSentiment
    {
        //private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string ModelPath => "C:/Users/kapeltol/Source/Repos/AIDemoApp/src/TodoApi/Data/SentimentModel.zip";

        public static int PredictSentiment(string text)
        {
            if (text == null || text == "")
                return 0;

            //check to see if model does exist
            if( !File.Exists(ModelPath))
            {
                ModelTrainer.CreateModel();
            }

            SentimentIssue sampleStatement = new SentimentIssue
            {
                Text = text
            };

            // Test with Loaded Model from .zip file
            using (var env = new LocalEnvironment())
            {
                ITransformer loadedModel;
                using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    loadedModel = TransformerChain.LoadFrom(env, stream);
                }

                // Create prediction engine and make prediction.
                var engine = loadedModel.MakePredictionFunction<SentimentIssue, SentimentPrediction>(env);

                SentimentPrediction predictionFromLoaded = engine.Predict(sampleStatement);
                System.Diagnostics.Debug.WriteLine("prediction: " + predictionFromLoaded.Prediction + ", Probability: " + predictionFromLoaded.Probability + ", score: " + predictionFromLoaded.Score);
                int predicition = 0;
                if (predictionFromLoaded.Prediction == true)
                    predicition = 1;

                return predicition;
            }            
        }
    }
}