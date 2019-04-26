using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Talk.EntityExtractor;

namespace Talk.Tokenisers
{
    internal abstract class RegExTokeniser : EntityTokeniser
    {
        protected abstract List<Handler> Expressions { get; set; }

        public class Handler
        {
            public string Expression;
            public Func<Match, Dictionary<string, object>, Token> Parser;
        }

        public override List<Token> GetTokens(string textfragment, Dictionary<string, object> properties)
        {
            List<Token> tokens = new List<Token>();

            foreach (var exp in Expressions)
            {
                Regex regex = new Regex(exp.Expression, RegexOptions.IgnoreCase);
                Match match = regex.Match(textfragment);
                if (match.Success)
                {                    
                    var token = exp.Parser(match, properties);
                    if (token!=null)
                        tokens.Add(token);
                }
            }

            return tokens;
        }
    }

}
