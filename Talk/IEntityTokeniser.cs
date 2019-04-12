using System.Collections.Generic;

namespace Talk
{
    public interface IEntityTokeniser
    {
        List<Token> GetTokens(string textfragment, Dictionary<string, object> Properties);
        void BeginParse(string text, Dictionary<string, object> Properties);

    }
}