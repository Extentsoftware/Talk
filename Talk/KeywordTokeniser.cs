using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Talk
{
    public class KeywordTokeniser : EntityTokeniser
    {
        ITalkConfig _settings;

        public KeywordTokeniser(ITalkConfig settings)
        {
            _settings = settings;
        }

        public override List<Token> GetTokens(string textfragment, Dictionary<string, object> Properties)
        {
            List<Token> tokens = new List<Token>();

            foreach (var category in _settings.Keywords)
            {
                foreach (var exp in category.Items)
                {
                    Regex regex = new Regex(exp, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                    Match x = regex.Match(textfragment);
                    if (x.Success)
                    {
                        tokens.Add(new KeywordToken { Text = x.Value, Length = x.Length, Pos = x.Index, Subtypes = new List<string> { category.Category } });
                    }
                }
            }
            return tokens;
        }
    }

}
