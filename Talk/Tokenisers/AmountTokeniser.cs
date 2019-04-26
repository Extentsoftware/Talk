using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Talk.Tokenisers
{
    internal class AmountTokeniser : RegExTokeniser
    {
        protected override List<Handler> Expressions { get; set; } = new List<Handler> {
                new Handler{
                    Expression = @"\b[£]*\d+([.]\d{0,2})*\b",
                    Parser = (match,properties)=> {
                        var valueText = match.Value.Replace("£","");
                        var token = new AmountToken { Text = valueText, Length = match.Length, Pos = match.Index };
                        if (double.TryParse(valueText, out double token_amount))
                        {
                            foreach(var d in properties)
                            {
                                if (d.Value is double?)
                                {
                                    var dbl = d.Value as double?;
                                    if (dbl!=null)
                                    {
                                        if (dbl==token_amount)
                                             token.Subtypes.Add("="+d.Key);
                                        if (dbl>token_amount)
                                             token.Subtypes.Add("<"+d.Key);
                                        if (dbl<token_amount)
                                             token.Subtypes.Add(">"+d.Key);
                                    }
                                }
                            }
                            return token;
                        }
                            else
                                return null;
                    }
                },
        };
    }
}
