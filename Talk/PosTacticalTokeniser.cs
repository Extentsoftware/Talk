using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Talk
{
    public class PosTacticalTokeniser : EntityTokeniser
    {
        IAppSettings _settings;

        public PosTacticalTokeniser(IAppSettings settings)
        {
            _settings = settings;
        }

        public override List<Token> GetTokens(string textfragment, Dictionary<string, object> Properties)
        {
            List<Token> tokens = new List<Token>();

            foreach (var exp in _settings.PositiveEscalationWords)
            {
                Regex regex = new Regex(exp, RegexOptions.IgnoreCase);
                Match x = regex.Match(textfragment);
                if (x.Success)
                {
                    tokens.Add(new PosTacticalToken { Text = x.Value, Length = x.Length, Pos = x.Index });
                }
            }

            return tokens;
        }
    }

}
