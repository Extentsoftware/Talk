using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Talk
{
    public class QuestionTokeniser : EntityTokeniser
    {
        public override List<Token> GetTokens(string textfragment, Dictionary<string, object> Properties)
        {
            List<Token> tokens = new List<Token>();
            string[] expressions = new string[] {
                @"\?|what|how many|where"
            };

            foreach (var exp in expressions)
            {
                Regex regex = new Regex(exp, RegexOptions.IgnoreCase);
                Match x = regex.Match(textfragment);
                if (x.Success)
                {
                    tokens.Add(new QuestionToken { Text = x.Value, Length = x.Length, Pos = x.Index });
                }
            }

            return tokens;
        }
    }

}
