using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Vanquis.Digital.Ivan.Dialog.EntityExtractor;
using Vanquis.Digital.Ivan.Dialog.Model;

namespace Vanquis.Digital.Ivan.Dialog.Tokenisers
{
    public class DateTokeniser : EntityTokeniser
    {
        private const string ReplacePattern = "${day}/${month}/${year}";

        IDialogConfig _settings;

        public DateTokeniser(IDialogConfig settings)
        {
            _settings = settings;
        }

        public override List<Token> GetTokens(string textfragment, Dictionary<string, object> properties)
        {
            List<Token> tokens = new List<Token>();
            foreach (var pattern in _settings.TodayDateWords)
            {
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                MatchCollection matches = regex.Matches(textfragment);
                foreach (Match match in matches)
                {
                    var token = new DateToken {
                        Value = DateTime.Now,
                        Length = match.Length,
                        Pos = match.Index,
                        Text = "today"
                    };

                    token.Subtypes.Add("Today");
                    tokens.Add(token);
                }
            }

            foreach (var pattern in _settings.DateCleanupFormats)
            {
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                MatchCollection matches = regex.Matches(textfragment);
                foreach (Match match in matches)
                {
                    var cleandate = Regex.Replace(match.Value, pattern, ReplacePattern, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

                    if (DateTime.TryParseExact(cleandate, _settings.DateFormats, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    {
                        var token = new DateToken {
                            Value = result,
                            Length = match.Length,
                            Pos = match.Index,
                            Text = "on " + result.ToShortDateString()
                        };

                        foreach (var d in properties)
                            if (d.Value is DateTime)
                                if ((DateTime)d.Value == token.Value)
                                    token.Subtypes.Add(d.Key);

                        // and useful dates
                        if (token.Value == DateTime.Today)
                        {
                            token.Text = "today";
                            token.Subtypes.Add("Today");
                        }

                        if (token.Value == DateTime.Today.AddDays(1))
                        {
                            token.Text = "tomorrow";
                            token.Subtypes.Add("Tomorrow");
                        }

                        if (token.Value == DateTime.Today.AddDays(-1))
                        {
                            token.Text = "yesterday";
                            token.Subtypes.Add("Yesterday");
                            token.Subtypes.Add("Past");
                        }

                        if (token.Value.Date.Subtract(DateTime.Today).TotalDays > 0)
                            token.Subtypes.Add("Future");

                        if (token.Value.Date.Subtract(DateTime.Today).TotalDays < 0)
                            token.Subtypes.Add("Past");

                        tokens.Add(token);
                    }
                }
            }

            return tokens;
        }

    }
}