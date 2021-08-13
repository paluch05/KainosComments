using System;
using Newtonsoft.Json;

namespace FunctionApp1.Model
{
    public class Comment
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
    }
}