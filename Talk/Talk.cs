using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Talk
{

    internal class Talk
    {
        internal static (bool, string) ProcessResponse(TalkContext context, string customerMessage, IServiceProvider serviceProvider)
        {
            List<string> botResponse = new List<string>();

            // decode their response
            var customerResponse = GetNextResponse(context, customerMessage, serviceProvider);

            // collect all data from tokens
            var matches = Contains(customerResponse, context.CurrentStep.DataToCollect);

            // report any failures
            var fail_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Fail).ToList();
            if (fail_matches != null && fail_matches.Count > 0)
                return (true, $"Escalated because of {fail_matches.Count} rejection tokens");

            // find warnings
            var warn_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Warning).ToList();
            if (warn_matches != null && warn_matches.Count > 0)
                foreach (var warning in warn_matches)
                    botResponse.Add(MakeMessageFromKey(warning.Property.MessageTemplate, context));

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

                    context.CollectedData.Add(collect.Property.PropertyName, collect.MatchingTokens.First().Token.Text);
                    botResponse.Add(MakeMessageFromKey(collect.Property.MessageTemplate, context));
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
                    botResponse.Add($"{i}) " + MakeMessageFromKey(missing[i - 1].Prompt, context));
            }

            if (missing.Count == 1)
            {
                var s1 = MakeMessageFromKey(context.CurrentStep.InCompleteSinglePrompt, context);
                var s2 = MakeMessageFromKey(missing[0].Prompt, context);
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
        private static List<CollectPropertyMatch> Contains(List<TokenNode> tokens, List<CollectProperty> expressions)
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
        private static List<TokenNode> Contains(List<TokenNode> tokens, PropertyMatchExpression expression)
        {
            List<TokenNode> MatchingTokens = new List<TokenNode>();
            foreach (var t in tokens)
            {
                if (t.Token.GetType().Name == expression.Token)
                {
                    if (expression.AnySubtypes == null)
                        MatchingTokens.Add(t);
                    else
                    {
                        foreach (var subtype in expression.AnySubtypes)
                        {
                            if (subtype == null || (t.Token.Subtypes.Contains(subtype)))
                                MatchingTokens.Add(t);
                        }
                    }
                }
            }
            return MatchingTokens;
        }

        private static TokenNode Contains(List<TokenNode> tokens, Type tokenType, string subtype = null)
        {
            foreach (var t in tokens)
                if (t.Token.GetType() == tokenType)
                    if (subtype == null || (t.Token.Subtypes.Contains(subtype)))
                        return t;
            return null;
        }

        public static List<List<TokenNode>> ParseText(Dictionary<string, object> properties, string customerMessage, IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var tokenisers = scope.ServiceProvider.GetServices<IEntityTokeniser>();

                TokenTree tree = new TokenTree()
                    .Build(tokenisers, customerMessage, properties);

                var flattenedTokens = tree.Flatten();

                return flattenedTokens;
            }
        }


        private static List<TokenNode> GetNextResponse(TalkContext context, string customerMessage, IServiceProvider serviceProvider)
        {
            var flattenedTokens = ParseText(context.Properties, customerMessage, serviceProvider);

            // occam's razor, pick the least complicated solution
            var intent = flattenedTokens.OrderBy(x => x.Count).First();

            // ListTokens(intent);

            return intent;
        }

        public static void ListFlattenedTokens(List<List<TokenNode>> flattenedIntents)
        {
            Console.WriteLine($"{flattenedIntents.Count} intents");
            foreach (var intent in flattenedIntents)
                ListTokens(intent);
        }

        public static void ListTokens(List<TokenNode> intent)
        {
            foreach (var tokenNode in intent)
                Console.Write($"{tokenNode.Token} ");
            Console.WriteLine("");
        }
    }

}
