using System;
using System.Collections.Generic;

namespace VkToTg.Models
{
    public class MessageModel
    {
        public string Text { get; set; }
        public List<Uri> PhotosLinks { get; set; } = new List<Uri>();
        public List<Uri> DocumentsLinks { get; set; } = new List<Uri>();
        public Uri AudioMessageLink { get; set; }
    }
}