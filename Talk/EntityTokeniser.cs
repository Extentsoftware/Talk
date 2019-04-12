using System.Collections.Generic;

namespace Talk
{
    public abstract class EntityTokeniser: IEntityTokeniser
    {
        public abstract List<Token> GetTokens(string textfragment, Dictionary<string, object> properties);

        public virtual void BeginParse(string text, Dictionary<string, object> Properties)
        { }
    }

}
