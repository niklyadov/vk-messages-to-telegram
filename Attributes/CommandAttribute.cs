using System;

namespace VkToTg.Attributes
{
    public class CommandAttribute : Attribute
    {
        public CommandAttribute()
        {

        }

        public CommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
    }
}