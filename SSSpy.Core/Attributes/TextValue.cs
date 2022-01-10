using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSpy.Core.Attributes
{
    public class TextValueAttribute : Attribute
    {
        public string Value { get; set; }

        public TextValueAttribute(string v)
        {
            Value = v;
        }
    }
}
