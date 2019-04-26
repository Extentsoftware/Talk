using System.Collections.Generic;
using Talk.EntityExtractor;

namespace Talk.Tokenisers
{
    internal interface IEntityTokeniser
    {
        List<Token> GetTokens(string textfragment, Dictionary<string, object> Properties);
        void BeginParse(string text, Dictionary<string, object> Properties);

    }
}