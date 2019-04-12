using System.Collections.Generic;

namespace Talk
{
    public class KeywordTokeniser : RegExTokeniser
    {
        public override void BeginParse(string text, Dictionary<string, object> Properties)
        {
            foreach (var di in Properties)
            {
                if (di.Value is string txt)
                {
                    var handler = new Handler
                    {
                        Expression = $"{txt}",
                        Parser = (match, properties) =>
                        {
                            var token = new KeywordToken { Text = match.Value, Length = match.Length, Pos = match.Index };
                            token.Subtypes.Add(di.Key);
                            return token;
                        }
                    };

                    Expressions.Add(handler);
                }
            }
        }
        protected override List<Handler> Expressions { get; set; } = new List<Handler>();

    };
}

