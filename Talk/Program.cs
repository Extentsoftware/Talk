using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Talk
{
    class Program
    {
        static IServiceProvider _serviceProvider;

        static void Main(string[] args)
        {

            Console.WriteLine("Lets Talk!");
            var myAppSettings = BuildConfiguration();

            var services = new ServiceCollection()
               .AddTransient<IEntityTokeniser, DateTokeniser>()
               .AddTransient<IEntityTokeniser, PosTacticalTokeniser>()
               .AddTransient<IEntityTokeniser, NegIntentTokeniser>()
               .AddTransient<IEntityTokeniser, NegTacticalTokeniser>()
               .AddTransient<IEntityTokeniser, QuestionTokeniser>()
               .AddTransient<IEntityTokeniser, KeywordTokeniser>()
               .AddTransient<IEntityTokeniser, AmountTokeniser>()
               .AddSingleton<IAppSettings>(myAppSettings);
               

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddApplicationInsights();
                loggingBuilder.AddConsole();
            });

            _serviceProvider.BuildServiceProvider();

            CollectDataContext context = new CollectDataContext()
            {
                InitialPrompt = "Hi [Person:FirstName], a minimum payment of £[MinimumPayment] is due on your credit card.\nWe can accept payments up to £[MaximumPayment].\nPlease reply with you DoB, amount and when you can pay",
                DataToCollect = new List<CollectProperty>
                {
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Fail,
                          MessageTemplate="neg_escalation",
                          PropertyName = "",
                          Expression=new PropertyMatchExpression{ Token="NegTacticalToken" }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Fail,
                          MessageTemplate="neg_escalation",
                          PropertyName = "",
                          Expression=new PropertyMatchExpression{ Token="NegIntentToken" }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Fail,
                          MessageTemplate="pos_escalation",
                          PropertyName = "",
                          Expression=new PropertyMatchExpression{ Token="PosTacticalToken" }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Fail,
                          MessageTemplate="future_escalation",
                          PropertyName = "",
                          Expression=new PropertyMatchExpression{ Token="DateToken", AnySubtypes=new string[] { "Future" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Collect,
                          MessageTemplate="got_dob",
                          PropertyName = "Birthday",
                          Prompt="missing_dob",
                          Expression=new PropertyMatchExpression{ Token="DateToken", AnySubtypes = new string[] { "Birthday" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Warning,
                          MessageTemplate="got_payment_toolow",
                          PropertyName = "PaymentDay",
                          Expression=new PropertyMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { "<MinimumPayment" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Warning,
                          MessageTemplate="got_payment_toohi",
                          PropertyName = "PaymentDay",
                          Expression=new PropertyMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { ">MaximumPayment" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Collect,
                          MessageTemplate="got_payment_day",
                          PropertyName = "PaymentDay",
                          Prompt="missing_when",
                          Expression=new PropertyMatchExpression{ Token="DateToken", AnySubtypes = new string[] { "Today" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Collect,
                          MessageTemplate="got_payment_amount",
                          PropertyName = "PaymentAmount",
                          Prompt="missing_payment",
                          Expression=new PropertyMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { "=MinimumPayment", ">MinimumPayment" } }
                     }
                },
                MessageTemplates = new Dictionary<string, string>
                {
                    { "neg_escalation", "Escalated!!" },
                    { "pos_escalation", "Escalated!!" },
                    { "future_escalation", "Escalated!!" },
                    { "got_dob", "got_dob of [Birthday]" },
                    { "got_payment_day", "got_payment_day [PaymentDay]" },
                    { "got_payment_amount", "got_payment_amount [PaymentAmount]" },
                    { "got_payment_toolow", "Your payment amount is too low, it should be between [MinimumPayment] and [MaximumPayment], let me know how much you can pay" },
                    { "got_payment_toohi", "Your payment amount is too high, it should be between [MinimumPayment] and [MaximumPayment], let me know how much you can pay" },
                    { "missing_dob", "..Your DoB, this is for security" },
                    { "missing_payment", "..A payment amount between [MinimumPayment] and [MaximumPayment]" },
                    { "missing_when", "..When can you make this payment" }
                },
                Properties = new Dictionary<string, object>
                {
                    { "Birthday", new DateTime(1999, 3, 3) },
                    { "Person:FirstName", "Marcus" },
                    { "Person:GivenName", "Poulton" },
                    { "MinimumPayment", 12.0 },
                    { "MaximumPayment", 24.0 },
                    { "Last4Card", 1234.0 },
                    { "DueDate", DateTime.Today.AddDays(1) },
                }
            };

            PerformStep(context);

        }

        public static string Substitute(string message, Dictionary<string, object> properties)
        {
            foreach(var v in properties)
                message = message.Replace($"[{v.Key}]", v.Value.ToString());
            return message;
        }

        /// <summary>
        /// make a message from a message template key and data in the context
        /// </summary>
        /// <param name="message_key"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string MakeMessageFromKey(string message_key, CollectDataContext context)
        {
            var msg_template = context.MessageTemplates[message_key];
            msg_template = Substitute(msg_template, context.CollectedData);
            msg_template = Substitute(msg_template, context.Properties);
            return msg_template;
        }

        public static void PerformStep(CollectDataContext context)
        {
            bool quit = false;
            var nextPrompt = context.InitialPrompt;
            do
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Substitute(nextPrompt, context.Properties));

                Console.ForegroundColor = ConsoleColor.White;
                var quest = Console.ReadLine();
                if (string.IsNullOrEmpty(quest))
                    break;

                Console.ForegroundColor = ConsoleColor.Gray;

                (quit, nextPrompt) = ProcessResponse(context, quest, _serviceProvider);

            } while (!quit);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(nextPrompt);
        }

        public class PropertyMatchExpression
        {
            public string Token;
            public string[] AnySubtypes;
        }

        public class CollectProperty
        {
            public enum CollectionResult
            {
                Ignore,
                Collect,
                Warning,
                Fail
            };

            public CollectionResult Result;
            public string MessageTemplate;
            public PropertyMatchExpression Expression;
            public string PropertyName;
            public bool Optional;
            public string Prompt;
        }

        public class CollectDataContext
        {
            public Dictionary<string, string> MessageTemplates;
            public string InitialPrompt;
            public Dictionary<string, object> Properties = new Dictionary<string, object>();
            public Dictionary<string, object> CollectedData = new Dictionary<string, object>();
            public List<CollectProperty> DataToCollect = new List<CollectProperty>();
        }

        static (bool, string) ProcessResponse(CollectDataContext context, string customerMessage, IServiceProvider serviceProvider)
        {
            List<string> botResponse = new List<string>();

            // decode their response
            var customerResponse = GetNextResponse(context, customerMessage, serviceProvider);

            // collect all data from tokens
            var matches = Contains(customerResponse, context.DataToCollect);

            // report any failures
            var fail_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Fail).ToList();
            if (fail_matches != null && fail_matches.Count > 0)
                return (true, $"Escalated because of {fail_matches.Count} rejection tokens");

            // find warnings
            var warn_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Warning).ToList();
            if (warn_matches != null && fail_matches.Count > 0)
                foreach (var warning in warn_matches)
                    botResponse.Add(MakeMessageFromKey(warning.Property.MessageTemplate, context));

            var collect_matches = matches.Where(x => x.Property.Result == CollectProperty.CollectionResult.Collect).ToList();

            bool got_everything = (collect_matches.Count == context.CollectedData.Count);

            if (collect_matches != null && collect_matches.Count > 0)
            {
                foreach (var collect in collect_matches)
                {
                    context.CollectedData.Add(collect.Property.PropertyName, collect.MatchingTokens.First().Token.Text);
                    botResponse.Add(MakeMessageFromKey(collect.Property.MessageTemplate, context));
                }
            }

            // got it all??
            if (got_everything)
            {
                botResponse.Add($"Ok, I think thats everything. (on to confirmation)");
                return (true, string.Join("\n", botResponse) );
            }

            // find missing data

            var missing = new List<CollectProperty>();
            foreach (var required in context.DataToCollect.Where(x => x.Optional == false && x.Result== CollectProperty.CollectionResult.Collect))
                if (!context.CollectedData.ContainsKey(required.PropertyName))
                    missing.Add(required);

            if (missing.Count>0)
            {
                botResponse.Add("So I still need from you:");
                foreach (var required in missing)
                    botResponse.Add( MakeMessageFromKey(required.Prompt, context) );
            }

            return (false, string.Join("\n", botResponse));
        }

        class CollectPropertyMatch
        {
            public CollectProperty Property;
            public List<TokenNode> MatchingTokens;
        }

        /// <summary>
        /// matches a list of expressions in a token list
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="expressions"></param>
        /// <returns></returns>
        static List<CollectPropertyMatch> Contains(List<TokenNode> tokens, List<CollectProperty> expressions)
        {
            List<CollectPropertyMatch> MatchingTokens = new List<CollectPropertyMatch>();
            foreach(var expression in expressions)
            {
                var matches = Contains(tokens, expression.Expression);
                if (matches!=null && matches.Count>0)
                    MatchingTokens.Add( new CollectPropertyMatch { MatchingTokens = matches, Property = expression });
            }
            return MatchingTokens;
        }

        /// <summary>
        /// find matching tokens in token list
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        static List<TokenNode> Contains(List<TokenNode> tokens, PropertyMatchExpression expression)
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

        static TokenNode Contains(List<TokenNode> tokens, Type tokenType, string subtype = null)
        {
            foreach (var t in tokens)
                if (t.Token.GetType() == tokenType)
                    if (subtype == null || (t.Token.Subtypes.Contains(subtype)))
                        return t;
            return null;
        }

        static List<TokenNode> GetNextResponse(CollectDataContext context, string customerMessage, IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var tokenisers = scope.ServiceProvider.GetServices<IEntityTokeniser>();

                TokenTree tree = new TokenTree()
                    .Build(tokenisers, customerMessage, context.Properties);

                var flattenedTokens = tree.Flatten();

                // occam's razor, pick the least complicated solution
                var intent = flattenedTokens.OrderBy(x => x.Count).First();

                ListTokens(intent);

                return intent;
            }
        }

        private static void ListFlattenedTokens(List<List<TokenNode>> flattenedIntents)
        {
            Console.WriteLine($"{flattenedIntents.Count} intents");
            foreach (var intent in flattenedIntents)
                ListTokens(intent);
        }

        private static void ListTokens(List<TokenNode> intent)
        {
            foreach (var tokenNode in intent)
                Console.Write($"{tokenNode.Token} ");
            Console.WriteLine("");
        }

        public static AppSettings BuildConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            // get settings from config manager into 
            var myAppSettings = config.Get<AppSettings>();

            if (myAppSettings == null)
                throw new ApplicationException("Configuration error: Could not instantiate app settings");

            return myAppSettings;
        }
    }
}
