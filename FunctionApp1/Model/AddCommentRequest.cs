using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace FunctionApp1.Model
{
    internal class AddCommentRequest
    {
        public string Author { get; set; }
        public string Text { get; set; }
    }
}