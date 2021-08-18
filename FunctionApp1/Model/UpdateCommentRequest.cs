using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace FunctionApp1.Model
{
    internal class UpdateCommentRequest
    {
        public string Text { get; set; }
    }
}
