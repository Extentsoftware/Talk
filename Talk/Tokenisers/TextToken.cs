using Talk.EntityExtractor;

namespace Talk.Tokenisers
{
    internal class TextToken : Token
    {
        public override string ToString()
        {
            return $"{Text}";
        }
    }
}
