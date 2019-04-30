using Vanquis.Digital.Ivan.Dialog.EntityExtractor;

namespace Vanquis.Digital.Ivan.Dialog.Tokenisers
{
    internal class TextToken : Token
    {
        public override string ToString()
        {
            return $"{Text}";
        }
    }
}
