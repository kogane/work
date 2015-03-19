using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eratter.Tokens
{
    class Unparse : TokenBase
    {
        public string Name { get; private set; }
        public string Data { get; private set; }
        public int Priority { get; private set; }

        public Unparse(string name, string data, int priority)
        {
            Name = name;
            Data = data;
            Priority = priority;
        }
    }
}
