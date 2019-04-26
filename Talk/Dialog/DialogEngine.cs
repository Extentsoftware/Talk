using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Talk.EntityExtractor;

namespace Talk.Dialog
{

    internal static class DialogEngine
    {
        /// <summary>
        /// calculate the importance of the token list given a list of properties to collect
        /// </summary>
        /// <param name="tokenlist"></param>
        /// <returns></returns>
        internal static double Weight(this List<Token> tokenlist, List<CollectProperty> collectProperties)
        {
            // collect all data from tokens
            var matches = Contains(tokenlist, collectProperties);
            return matches.Sum(x => x.Property.Weight);
        }

        /// <summary>
        /// returns the most likely token stream from a list of potentials
        /// </summary>
        /// <param name="flattenedTree"></param>
        /// <param name="collectProperties"></param>
        /// <returns></returns>
        internal static List<Token> MostLikely(this List<List<Token>> flattenedTree, List<CollectProperty> collectProperties)
        {
            return flattenedTree.OrderBy(x => x.Weight(collectProperties)).FirstOrDefault();
        }

        internal static (bool, string) ProcessResponse(TalkContext context, string customerMessage, IServiceProvider serviceProvider)
        {
            List<string> botResponse = new List<string>();

            // decode their response
            var responseTokenList = GetNextResponse(context, customerMessage, serviceProvider);

            // collect all data from tokens
            var matches = Contains(responseTokenList, context.CurrentStep.DataToCollect);

            // report any failures
            var fail_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Fail).ToList();
            if (fail_matches != null && fail_matches.Count > 0)
                return (true, $"Escalated because of {fail_matches.Count} rejection tokens");

            // find warnings
            var warn_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Warning).ToList();
            if (warn_matches != null && warn_matches.Count > 0)
                foreach (var warning in warn_matches)
                    botResponse.Add(MakeMessageFromKey(warning.Property.CapturedTemplate, context));

            var collect_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Collect).ToList();
            var reqrd_matches = context.CurrentStep.DataToCollect.Where(x => x.Result == CollectProperty.CollectionResult.Collect).ToList();

            bool got_everything = (reqrd_matches.Count == context.CollectedData.Count);

            if (collect_matches != null && collect_matches.Count > 0)
            {
                foreach (var collect in collect_matches)
                {
                    var key = collect.Property.PropertyName;
                    if (context.CollectedData.ContainsKey(key))
                        context.CollectedData.Remove(key);

                    context.CollectedData.Add(collect.Property.PropertyName, collect.MatchingTokens.First().Text);
                    botResponse.Add(MakeMessageFromKey(collect.Property.CapturedTemplate, context));
                }
            }

            // got it all??
            if (got_everything)
            {
                botResponse.Add(MakeMessageFromKey(context.CurrentStep.CompletePrompt, context));
                return (true, string.Join("\n", botResponse));
            }

            // find missing data

            var missing = new List<CollectProperty>();
            foreach (var required in context.CurrentStep.DataToCollect.Where(x => x.Optional == false && x.Result == CollectProperty.CollectionResult.Collect))
                if (!context.CollectedData.ContainsKey(required.PropertyName))
                    missing.Add(required);

            if (missing.Count > 1)
            {
                botResponse.Add(MakeMessageFromKey(context.CurrentStep.InCompleteManyPrompt, context));
                for (int i = 1; i <= missing.Count; i++)
                    botResponse.Add($"{i}) " + MakeMessageFromKey(missing[i - 1].PromptTemplate, context));
            }

            if (missing.Count == 1)
            {
                var s1 = MakeMessageFromKey(context.CurrentStep.InCompleteSinglePrompt, context);
                var s2 = MakeMessageFromKey(missing[0].PromptTemplate, context);
                botResponse.Add(s1 + s2);
            }

            return (false, string.Join("\n", botResponse));
        }

        internal static string Substitute(string message, Dictionary<string, object> properties)
        {
            foreach (var v in properties)
                message = message.Replace($"[{v.Key}]", v.Value.ToString());
            return message;
        }

        /// <summary>
        /// make a message from a message template key and data in the context
        /// </summary>
        /// <param name="message_key"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string MakeMessageFromKey(string message_key, TalkContext context)
        {
            var msg_template = context.CurrentStep.MessageTemplates[message_key];
            msg_template = Substitute(msg_template, context.CollectedData);
            msg_template = Substitute(msg_template, context.Properties);
            return msg_template;
        }

        /// <summary>
        /// matches a list of expressions in a token list
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="expressions"></param>
        /// <returns></returns>
        private static List<CollectPropertyMatch> Contains(List<Token> tokens, List<CollectProperty> expressions)
        {
            List<CollectPropertyMatch> MatchingTokens = new List<CollectPropertyMatch>();
            foreach (var expression in expressions)
            {
                var matches = Contains(tokens, expression.Expression);
                if (matches != null && matches.Count > 0)
                    MatchingTokens.Add(new CollectPropertyMatch { MatchingTokens = matches, Property = expression });
            }
            return MatchingTokens;
        }

        /// <summary>
        /// find matching tokens in token list
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static List<Token> Contains(List<Token> tokens, TokenMatchExpression expression)
        {
            List<Token> MatchingTokens = new List<Token>();
            foreach (var t in tokens)
            {
                if (t.GetType().Name == expression.Token)
                {
                    if (expression.AnySubtypes == null)
                        MatchingTokens.Add(t);
                    else
                    {
                        foreach (var subtype in expression.AnySubtypes)
                        {
                            if (subtype == null || (t.Subtypes.Contains(subtype)))
                                MatchingTokens.Add(t);
                        }
                    }
                }
            }
            return MatchingTokens;
        }

        private static List<Token> GetNextResponse(TalkContext context, string customerMessage, IServiceProvider serviceProvider)
        {
            var flattenedTokens = Parser.ParseText(context.Properties, customerMessage, serviceProvider);

            return flattenedTokens.MostLikely(context.CurrentStep.DataToCollect);
        }
    }

}
