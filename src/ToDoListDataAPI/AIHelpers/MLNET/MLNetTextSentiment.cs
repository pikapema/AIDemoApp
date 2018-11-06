using System;
using System.IO;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;
using Microsoft.ML;
using ToDoListDataAPI.AIHelpers.MLNET.Models;

namespace ToDoListDataAPI.AIHelpers.MLNET
{
    public static class MLNetTextSentiment
    {
        //private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string ModelPath => "C:/Users/kapeltol/Source/Repos/AIDemoApp/src/ToDoListDataAPI/AIHelpers/MLNET/Data/SentimentModel.zip";

        public static double PredictSentiment(string text)
        {
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
            using (var env2 = new LocalEnvironment())
            {
                ITransformer loadedModel;
                var tmppath = "C:/Users/kapeltol/Source/Repos/AIDemoApp/src/ToDoListDataAPI/AIHelpers/MLNET/Data/SentimentModel.zip";
                using (var stream = new FileStream(tmppath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    loadedModel = TransformerChain.LoadFrom(env2, stream);
                }

                // Create prediction engine and make prediction.

                var engine = loadedModel.MakePredictionFunction<SentimentIssue, SentimentPrediction>(env2);

                var predictionFromLoaded = engine.Predict(sampleStatement);

                Console.WriteLine();
                Console.WriteLine("=============== Test of model with a sample ===============");

                Console.WriteLine($"Text: {sampleStatement.Text} | Prediction: {(Convert.ToBoolean(predictionFromLoaded.Prediction) ? "Toxic" : "Nice")} sentiment | Probability: {predictionFromLoaded.Probability} ");
                return predictionFromLoaded.Score;
            }            
        }
    }
}