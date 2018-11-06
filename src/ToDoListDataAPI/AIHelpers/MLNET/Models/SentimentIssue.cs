using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoListDataAPI.AIHelpers.MLNET.Models
{

    public class SentimentIssue
    {
        public bool Label { get; set; }
        public string Text { get; set; }
    }
}