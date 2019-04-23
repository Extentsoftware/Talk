using System.Collections.Generic;

namespace Talk
{
    /// <summary>
    /// Tokeniser that looks for string properties such as proper nouns and finds them in the text
    /// The tokeniser does not perform stemming or any other manipulation of the input text other
    /// than it is case-insensitive.
    /// </summary>
    public class PropertyKeywordTokeniser : RegExTokeniser
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
                            var token = new PropertyKeywordToken { Text = match.Value, Length = match.Length, Pos = match.Index };
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

