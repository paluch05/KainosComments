using System;
using Newtonsoft.Json;

namespace FunctionApp1.Model
{
    internal class Comment
    {
        private readonly int LIMIT = 200;
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
    }
}