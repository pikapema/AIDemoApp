namespace ToDoListDataAPI.Models
{
    public class ToDoItem
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public double CognitiveSentimentScore { get; set; } = 0;
        public double MlNetSentimentScore { get; set; } = 0;
    }
}
