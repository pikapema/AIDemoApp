namespace SentimentDataAPI.Models
{
    public class SentimentItem
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public double CognitiveSentimentScore { get; set; } = 0;
        public double MlNetSentimentScore { get; set; } = 0;
        public double MlCustomSentimentScore { get; set; } = 0;
    }
}
