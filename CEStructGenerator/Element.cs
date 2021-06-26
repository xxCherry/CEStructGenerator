using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEStructGenerator
{
    public class Element
    {
        public string Type;
        public string Name;

        public Element(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
