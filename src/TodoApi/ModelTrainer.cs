using System;
using System.IO;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML;
using TodoApi.Models;

namespace TodoApi
{
    public static class ModelTrainer
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string TrainDataPath => "C:/Users/kapeltol/Source/Repos/AIDemoApp/src/TodoApi/Data/Sentiment_tweets.tsv";
        //private static string TrainDataPath => "C:/Users/kapeltol/Source/Repos/AIDemoApp/src/TodoApi/Data/wikipedia-detox-250-line-data.tsv";
        private static string TestDataPath => "C:/Users/kapeltol/Source/Repos/AIDemoApp/src/TodoApi/Data/wikipedia-detox-250-line-test.tsv";
        private static string ModelPath => "C:/Users/kapeltol/Source/Repos/AIDemoApp/src/TodoApi/Data/SentimentModel.zip";

        public static void CreateModel()
        {
            //1. Create ML.NET context/environment
            using (var env = new LocalEnvironment())
            {
                //2. Create DataReader with data schema mapped to file's columns
                var reader = new TextLoader(env,
                                            new TextLoader.Arguments()
                                            {
                                                Separator = "tab",
                                                HasHeader = true,
                                                Column = new[]
                                                {
                                                    new TextLoader.Column("Label", DataKind.Bool, 0),
                                                    new TextLoader.Column("Text", DataKind.Text, 1)
                                                }
                                            });

                //Load training data
                IDataView trainingDataView = reader.Read(new MultiFileSource(TrainDataPath));


                //3.Create a flexible pipeline (composed by a chain of estimators) for creating/traing the model.

                var pipeline = new TextTransform(env, "Text", "Features")  //Convert the text column to numeric vectors (Features column)   
                                           .Append(new LinearClassificationTrainer(env, new LinearClassificationTrainer.Arguments(),
                                                                                   "Features",
                                                                                   "Label"));
                //.Append(new LinearClassificationTrainer(env, "Features", "Label")); //(Simpler in ML.NET v0.7)



                //4. Create and train the model    
                var model = pipeline.Fit(trainingDataView);


                //5. Evaluate the model and show accuracy stats

                //Load evaluation/test data
                IDataView testDataView = reader.Read(new MultiFileSource(TestDataPath));
                var predictions = model.Transform(testDataView);

                var binClassificationCtx = new BinaryClassificationContext(env);
                var metrics = binClassificationCtx.Evaluate(predictions, "Label");


                System.Diagnostics.Debug.WriteLine("Model quality metrics evaluation");
                System.Diagnostics.Debug.WriteLine("------------------------------------------");
                System.Diagnostics.Debug.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
                System.Diagnostics.Debug.WriteLine($"Auc: {metrics.Auc:P2}");
                System.Diagnostics.Debug.WriteLine($"F1Score: {metrics.F1Score:P2}");
                System.Diagnostics.Debug.WriteLine("=============== End of Model's evaluation ===============");
                                

                // Save model to .ZIP file
                SaveModelAsFile(env, model);
            }
        }

        private static void SaveModelAsFile(LocalEnvironment env, TransformerChain<BinaryPredictionTransformer<Microsoft.ML.Runtime.Internal.Internallearn.IPredictorWithFeatureWeights<float>>> model)
        {
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                model.SaveTo(env, fs);

            System.Diagnostics.Debug.WriteLine("The model is saved to {0}", ModelPath);
        }
    }
}