﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApi.Models
{
    public class TodoItem
{
    public int Id { get; set; }
    public string Description { get; set; }
    public double CognitiveSentimentScore { get; set; }
    public bool MlNetSentimentScore { get; set; }
}
}