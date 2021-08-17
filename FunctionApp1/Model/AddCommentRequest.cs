using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace FunctionApp1.Model
{
    internal class AddCommentRequest
    {
        public string Author { get; set; }
        [JsonProperty()]
        public string Text { get; set; }

        //[OnDeserialized]
        //internal void OnDeserializedMethod(StreamingContext context)
        //{
        //    if (Text.Length >= 200)
        //    {
        //        throw new InvalidOperationException("Your text is too long");
        //    }
        //}
    }
}