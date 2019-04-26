using System;
using System.Collections.Generic;
using Talk.EntityExtractor;

namespace Talk.Tokenisers
{
    internal class DateToken : Token
    {
        public DateTime Value;

        public override string ToString()
        {
            return $"{Text}({Value} {string.Join(",",Subtypes)})";
        }
    }

}
