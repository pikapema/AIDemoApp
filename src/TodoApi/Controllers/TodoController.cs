using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoApi.Controllers
{
    [Route("api/todo")]
    public class TodoController : Controller
    {

        private static Dictionary<int, TodoItem> mockData = new Dictionary<int, TodoItem>();

        static TodoController()
        {
            mockData.Add(0, new TodoItem { Id = 0, Description = "feed the dog", CognitiveSentimentScore = 0.5, MlNetSentimentScore = false });
            mockData.Add(1, new TodoItem { Id = 1, Description = "take the dog on a walk", CognitiveSentimentScore = 0.5, MlNetSentimentScore = false });
        }
        
        // GET: api/ToDoItemList
        [HttpGet]
        public IEnumerable<TodoItem> Get()
        {
            return mockData.Values;
        }

        // GET: api/ToDoItemList
        [HttpPost]
        public void Post(TodoItem todo)
        {
            bool mlSentimentValue = MLNetTextSentiment.PredictSentiment(todo.Description);
            todo.MlNetSentimentScore = mlSentimentValue;

            todo.Id = mockData.Count > 0 ? mockData.Keys.Max() + 1 : 1;
            mockData.Add(todo.Id, todo);
        }

    }
}
