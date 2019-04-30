using System.Collections.Generic;
using Vanquis.Digital.Ivan.Dialog.EntityExtractor;

namespace Vanquis.Digital.Ivan.Dialog.Tokenisers
{
    public interface IEntityTokeniser
    {
        List<Token> GetTokens(string textfragment, Dictionary<string, object> Properties);
        void BeginParse(string text, Dictionary<string, object> Properties);

    }
}