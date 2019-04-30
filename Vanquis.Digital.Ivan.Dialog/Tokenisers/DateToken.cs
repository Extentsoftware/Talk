using System;
using Vanquis.Digital.Ivan.Dialog.EntityExtractor;

namespace Vanquis.Digital.Ivan.Dialog.Tokenisers
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
