using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoApi.Controllers
{
    [Route("api/MLNETmodel")]
    public class MLNETmodelController : Controller
    {
        static MLNETmodelController()
        {
        }
  
        [HttpPost]
        public int Post(string text)
        {
            return MLNetTextSentiment.PredictSentiment(text);
        }

    }
}
