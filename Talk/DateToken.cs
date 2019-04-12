using System;
using System.Collections.Generic;

namespace Talk
{
    public class DateToken : Token
    {
        public DateTime Value;

        public override string ToString()
        {
            return $"{Text}({Value} {string.Join(",",Subtypes)})";
        }
    }

}
