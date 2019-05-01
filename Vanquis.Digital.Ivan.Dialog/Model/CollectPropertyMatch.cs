using System.Collections.Generic;
using Vanquis.Digital.Ivan.Dialog.EntityExtractor;

namespace Vanquis.Digital.Ivan.Dialog.Model
{
    public class CollectPropertyMatch
    {
        public CollectProperty Property;
        public List<Token> MatchingTokens;

        public override string ToString()
        {
            return $"{Property.PropertyName} {string.Join(", ", MatchingTokens)}";
        }
    }
}
