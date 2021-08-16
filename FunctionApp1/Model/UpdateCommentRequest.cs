using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FunctionApp1.Model
{
    internal class UpdateCommentRequest
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Author { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
