using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vanquis.Digital.Ivan.Dialog.EntityExtractor;
using Vanquis.Digital.Ivan.Dialog.Model;
using Vanquis.Digital.Ivan.Dialog.Tokenisers;

namespace Vanquis.Digital.Ivan.Dialog.Talk
{
    public static partial class DialogEngine
    {
       
        public static TalkAction ProcessResponse(
            string humanText,
            IDialogConfig config,
            TalkContext context,
            IEnumerable<IEntityTokeniser> tokenisers)
        {
            var currentStep = config.Intents.FirstOrDefault(x => x.Name == context.CurrentIntent);

            // no customer response just yet - assume initial prompt
            if (humanText == null)
            {
                var response = currentStep.InitialPrompt.Substitute(context);
                return new SayAction
                {
                    Prompt = response,
                    Category = "InitialPrompt"
                };
            }

            List<string> botResponse = new List<string>();

            // decode their response
            var responseTokenList = TokeniseText(config, context, humanText, tokenisers);

            // collect all data from tokens
            var matches = Contains(responseTokenList, currentStep.DataToCollect);

            // report any failures
            var fail_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Fail).ToList();
            if (fail_matches != null && fail_matches.Count > 0)
            {
                return new FailAction
                {
                    Reason = $"Escalated because of {fail_matches.Count} rejection tokens",
                    Rejections = fail_matches.Select(x=>x.Property).ToList()
                };
            }

            HandleWarnings(matches, botResponse, currentStep, config, context);

            HandleCollectedData(matches, botResponse, currentStep, config, context);

            // got it all??
            if (GotValuesForStep(currentStep, config, context))
            {
                botResponse.Add(MakeMessageFromKey(currentStep.CompletePrompt, config, context));
                return new NextStepAction
                {
                    Prompt = string.Join("\n", botResponse)
                };
            }

            // handle repeated failures
            var failaction = HandleTooManyWarnings(matches, currentStep, config, context);
            if (failaction != null)
                return failaction;

            failaction = HandleTooManyCollects(botResponse, currentStep, config, context);
            if (failaction != null)
                return failaction;

            HandleMissingItems(botResponse, currentStep, config, context);

            return new SayAction
            {
                Category = "MoreData",
                Prompt = string.Join("\n", botResponse)
            };
        }

        /// <summary>
        /// check that we have captured all required items
        /// </summary>
        /// <param name="config"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool GotValuesForStep(Intent currentStep, IDialogConfig config, TalkContext context)
        {
            // get list of items required on this step.
            var reqrd_properties = currentStep.DataToCollect.Where(x => x.Result == CollectProperty.CollectionResult.Collect).ToList();
            // check the collected data to see if we have everything
            foreach(var property in reqrd_properties)
            {
                if (!context.CollectedData.Any(x => x.Key == property.PropertyName))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// default method for handling FailAction
        /// Moves to next fail step
        /// </summary>
        /// <param name="config"></param>
        /// <param name="context"></param>
        public static void FailDefaultAction(IDialogConfig config, TalkContext context)
        {
            // get next intent from the route
            var group = config.IntentGroups[context.IntentGroup];
            var route = group.IntentRoutes.FirstOrDefault(x => x.FromName == context.CurrentIntent);
            if (route != null)
                context.CurrentIntent = route.FailName;
            else
                context.CurrentIntent = null;
        }

        /// <summary>
        /// default method for handling NextStep
        /// </summary>
        /// <param name="config"></param>
        /// <param name="context"></param>
        public static void NextStepDefaultAction(IDialogConfig config, TalkContext context)
        {
            var group = config.IntentGroups[context.IntentGroup];
            var route = group.IntentRoutes.FirstOrDefault(x => x.FromName == context.CurrentIntent);
            if (route != null)
                context.CurrentIntent = route.ToName;
            else
                context.CurrentIntent = null;
        }

        private static void HandleWarnings(List<CollectPropertyMatch> matches, List<string> botResponse, Intent currentStep, IDialogConfig config, TalkContext context)
        {
            // find warnings
            var warn_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Warning).ToList();
            if (warn_matches != null && warn_matches.Count > 0)
            {
                foreach (var warning in warn_matches)
                {
                    // remove any collected data that matches this property name
                    if (!string.IsNullOrEmpty(warning.Property.PropertyName))
                    {
                        matches.RemoveAll(x => x.Property.Result == CollectProperty.CollectionResult.Collect && x.Property.PropertyName == warning.Property.PropertyName);
                    }
                    botResponse.Add(MakeMessageFromKey(warning.Property.CapturedTemplate, config, context));
                }
            }
        }

        private static void HandleCollectedData(List<CollectPropertyMatch> matches, List<string> botResponse, Intent currentStep, IDialogConfig config, TalkContext context)
        {
            var collect_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Collect).ToList();

            // got some new data
            if (collect_matches != null && collect_matches.Count > 0)
            {
                // store collected data into the context
                foreach (var collect in collect_matches)
                {
                    var key = collect.Property.PropertyName;

                    // replace existing entity if it already exists
                    if (context.CollectedData.ContainsKey(key))
                        context.CollectedData.Remove(key);
                    context.CollectedData.Add(key, collect.MatchingTokens.First());
                }

                // get list of collect matches that need to be repeated back to human
                var collect_matches_to_verbalise = collect_matches.Where(x => !string.IsNullOrEmpty(x.Property.CapturedTemplate));
                if (collect_matches_to_verbalise != null
                    && collect_matches_to_verbalise.Count() > 0
                    && !string.IsNullOrEmpty(currentStep.CapturedPrompt))
                {
                    // output header message
                    var msg = MakeMessageFromKey(currentStep.CapturedPrompt, config, context);
                    botResponse.Add(msg);
                    // output each captured property to verbalise
                    foreach (var collect in collect_matches_to_verbalise)
                    {
                        // show message detailing what I got tis time around
                        var itemmsg = MakeMessageFromKey(collect.Property.CapturedTemplate, config, context);
                        botResponse.Add(itemmsg);
                    }
                }
            }
        }

        private static FailAction HandleTooManyWarnings(List<CollectPropertyMatch> matches, Intent currentStep, IDialogConfig config, TalkContext context)
        {
            // build a list of missing non-optional "Collect" items
            var items = matches
                .Select(x => x.Property)
                .Where(x => x.MaxTries > 0 && x.Result == CollectProperty.CollectionResult.Warning)
                .ToList();

            return HandleTooManyRepeats(items, currentStep, config, context);
        }

        private static FailAction HandleTooManyCollects(List<string> botResponse, Intent currentStep, IDialogConfig config, TalkContext context)
        {
            // build a list of missing non-optional "Collect" items
            var items = currentStep.DataToCollect
                .Where(x => x.MaxTries > 0 && x.Optional == false && x.Result == CollectProperty.CollectionResult.Collect)
                .ToList();

            return HandleTooManyRepeats(items, currentStep, config, context);
        }


        private static FailAction HandleTooManyRepeats(List<CollectProperty> items, Intent currentStep, IDialogConfig config, TalkContext context)
        {

            // check the number times we have tried to collect this data
            foreach (var item in items)
            {
                // add if new
                var countkey = $"count_{item.PropertyName}";
                if (!context.AskCount.ContainsKey(countkey))
                    context.AskCount.Add(countkey, 0);

                // increment
                var tries = context.AskCount[countkey] + 1;
                context.AskCount[countkey] = tries;

                // check limit (MaxTries can be 0 for infinite retries)
                if (item.MaxTries > 0 && tries == item.MaxTries)
                    return new FailAction
                    {
                        Reason = $"Hi maximum. Got {item.PropertyName} {item.MaxTries} times",
                        Rejections = new List<CollectProperty> { item }
                    };
            }
            return null;
        }

        private static void HandleMissingItems(List<string> botResponse, Intent currentStep, IDialogConfig config, TalkContext context)
        {
            var missing_list = new List<CollectProperty>();

            // build a list of missing non-optional "Collect" items
            foreach (var required in currentStep.DataToCollect.Where(x => x.Optional == false && x.Result == CollectProperty.CollectionResult.Collect))
                if (!context.CollectedData.ContainsKey(required.PropertyName))
                    missing_list.Add(required);

            if (missing_list.Count > 1)
            {
                botResponse.Add(MakeMessageFromKey(currentStep.InCompleteManyPrompt, config, context));
                for (int i = 1; i <= missing_list.Count; i++)
                    botResponse.Add($"{i}) " + MakeMessageFromKey(missing_list[i - 1].PromptTemplate, config, context));
            }

            if (missing_list.Count == 1)
            {
                var s1 = MakeMessageFromKey(currentStep.InCompleteSinglePrompt, config, context);
                var s2 = MakeMessageFromKey(missing_list[0].PromptTemplate, config, context);
                botResponse.Add(s1 + s2);
            }
        }

        private static string Substitute(this string message, TalkContext context)
        {
            return message
                .Substitute(context.Properties)
                .Substitute(context.CollectedData);
        }

        /// <summary>
        /// substitute values into a string from a dictionary
        /// </summary>
        /// <param name="message"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private static string Substitute(this string message, Dictionary<string, object> properties)
        {
            Debug.Assert(message!=null);
            if (properties != null)
                foreach (var v in properties)
                    message = message.Replace($"[{v.Key}]", v.Value.ToString());
            return message;
        }

        /// <summary>
        /// substitute properties into a string template
        /// </summary>
        /// <param name="message"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private static string Substitute(this string message, Dictionary<string, Token> properties)
        {
            if (properties != null)
                foreach (var v in properties)
                    message = message.Replace($"[{v.Key}]", v.Value.Text);
            return message;
        }

        /// <summary>
        /// make a message from a message template key and data in the context
        /// </summary>
        /// <param name="message_key"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string MakeMessageFromKey(string message_key, IDialogConfig config, TalkContext context)
        {
            var currentStep = config.Intents.FirstOrDefault(x => x.Name == context.CurrentIntent);
            var msg_template = currentStep.MessageTemplates[message_key];
            var msg = msg_template.Substitute(context);
            return msg;
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
                var matches = tokens.Contains(expression.Expression);
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
        private static List<Token> Contains(this List<Token> tokens, TokenMatchExpression expression)
        {
            // get list of match token types for this expression
            List<Token> matchingTokens = tokens.Where(x=>x.GetType().Name == expression.Token).ToList();
            List<Token> filteredTokens = new List<Token>();

            foreach (var t in matchingTokens)
            {
                bool any = false;
                bool mustnot = false;

                if (expression.AnySubtypes != null)
                {
                    foreach (var subtype in expression.AnySubtypes)
                    {
                        if (subtype == null || (t.Subtypes.Contains(subtype)))
                            any = true;
                    }
                }

                if (expression.MustNotHave != null)
                {
                    foreach (var subtype in expression.MustNotHave)
                    {
                        if (t.Subtypes.Contains(subtype))
                            mustnot = true;
                    }
                }

                if (any && !mustnot)
                    filteredTokens.Add(t);
            }

            return filteredTokens;
        }

        /// <summary>
        /// tokenise human text into a token list
        /// </summary>
        /// <param name="config"></param>
        /// <param name="context"></param>
        /// <param name="customerMessage"></param>
        /// <param name="tokenisers"></param>
        /// <returns></returns>
        private static List<Token> TokeniseText( IDialogConfig config, TalkContext context, string customerMessage, IEnumerable<IEntityTokeniser> tokenisers)
        {
            var currentStep = config.Intents.FirstOrDefault(x => x.Name == context.CurrentIntent);
            var flattenedTokens = Parser.ParseText(context.Properties, customerMessage, tokenisers);
            return flattenedTokens.MostLikely(currentStep.DataToCollect);
        }

        /// <summary>
        /// calculate the importance of the token list given a list of properties to collect
        /// </summary>
        /// <param name="tokenlist"></param>
        /// <returns></returns>
        private static double Weight(this List<Token> tokenlist, List<CollectProperty> collectProperties)
        {
            // collect all data from tokens
            var matches = Contains(tokenlist, collectProperties);
            var weight = matches.Sum(x => x.Property.Weight);
            return weight;
        }

        /// <summary>
        /// returns the most likely token stream from a list of potentials
        /// </summary>
        /// <param name="flattenedTree"></param>
        /// <param name="collectProperties"></param>
        /// <returns></returns>
        private static List<Token> MostLikely(this List<List<Token>> flattenedTree, List<CollectProperty> collectProperties)
        {
            // calculated as the average weight of the response
            return flattenedTree.OrderByDescending(x => x.Weight(collectProperties)/x.Count).FirstOrDefault();
        }
    }
}
