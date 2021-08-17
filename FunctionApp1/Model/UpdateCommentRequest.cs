using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace FunctionApp1.Model
{
    internal class UpdateCommentRequest
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (Text.Length >= 200)
            {
                throw new ArgumentException("Too long");
            }
        }

    }
}
