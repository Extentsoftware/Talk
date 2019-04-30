using System.Collections.Generic;
using Vanquis.Digital.Ivan.Dialog.EntityExtractor;

namespace Vanquis.Digital.Ivan.Dialog.Tokenisers
{
    public abstract class EntityTokeniser: IEntityTokeniser
    {
        public abstract List<Token> GetTokens(string textfragment, Dictionary<string, object> properties);

        public virtual void BeginParse(string text, Dictionary<string, object> Properties)
        { }
    }

}
