using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eratter.Tokens
{
    class Token
    {
        public string Type = null;
        public string Data = null;

        public Token(string type, string data)
        {
            Type = type;
            Data = data;
        }
    }
}
