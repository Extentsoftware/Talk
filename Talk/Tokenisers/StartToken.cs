using Talk.EntityExtractor;

namespace Talk.Tokenisers
{
    internal class StartToken : Token
    {
        public override string ToString()
        {
            return $">>";
        }
    }
}
