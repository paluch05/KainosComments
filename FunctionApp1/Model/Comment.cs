using System;

namespace FunctionApp1.Model
{
    public class Comment
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
    }
}