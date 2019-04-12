using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Talk
{
    public class NegTacticalTokeniser : EntityTokeniser
    {
        IAppSettings _settings;

        public NegTacticalTokeniser(IAppSettings settings)
        {
            _settings = settings;
        }

        public override List<Token> GetTokens(string textfragment, Dictionary<string, object> Properties)
        {
            List<Token> tokens = new List<Token>();
            
            foreach (var exp in _settings.NegativeTacticalEscalationWords)
            {
                Regex regex = new Regex(exp, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                Match x = regex.Match(textfragment);
                if (x.Success)
                {
                    tokens.Add(new NegIntentToken { Text = x.Value, Length = x.Length, Pos = x.Index });
                }
            }

            return tokens;
        }
    }

}
