using System.Collections.Generic;
using Talk.EntityExtractor;

namespace Talk.Tokenisers
{
    internal abstract class EntityTokeniser: IEntityTokeniser
    {
        public abstract List<Token> GetTokens(string textfragment, Dictionary<string, object> properties);

        public virtual void BeginParse(string text, Dictionary<string, object> Properties)
        { }
    }

}
