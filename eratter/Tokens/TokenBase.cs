using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eratter.Tokens
{
    class TokenBase
    {
        public string Name { get; protected set; }
        public string Data { get; protected set; }
        public int Priority { get; private set; }

        public TokenBase Init(string name, string data, int priority)
        {
            Name = name;
            Data = data;
            Priority = priority;
            return this;
        }
    }
}
