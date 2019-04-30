using System.Collections.Generic;
using System.Text.RegularExpressions;
using Vanquis.Digital.Ivan.Dialog.EntityExtractor;
using Vanquis.Digital.Ivan.Dialog.Model;

namespace Vanquis.Digital.Ivan.Dialog.Tokenisers
{
    public class KeywordTokeniser : EntityTokeniser
    {
        IDialogConfig _settings;

        public KeywordTokeniser(IDialogConfig settings)
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
