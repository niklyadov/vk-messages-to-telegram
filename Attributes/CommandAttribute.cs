using System;

namespace VkToTg.Attributes
{
    public class CommandAttribute : Attribute
    {
        public CommandAttribute()
        {

        }

        public CommandAttribute(string v)
        {
            V = v;
        }

        public string V { get; }
    }
}