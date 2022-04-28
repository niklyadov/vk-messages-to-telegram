using System;

namespace VkToTg.Models
{
    public class MessageModel
    {
        public string Text { get; set; }
        public Uri PhotoLink { get; set; }
        public Uri DocumentLink { get; set; }
        public Uri VideoLink { get; set; }
        public Uri AudioMessageLink { get; set; }
    }
}