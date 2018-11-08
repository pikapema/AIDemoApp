using System.Collections.Generic;

namespace ToDoListDataAPI.Models
{
    public class Output1
    {
        public string Sentiment { get; set; }
        public string Score { get; set; }
    }

    public class Results
    {
        public List<Output1> output1 { get; set; }
    }

    public class ModelResponse
    {
        public Results Results { get; set; }
    }
}
