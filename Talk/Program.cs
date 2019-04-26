using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Talk.Dialog;
using Talk.EntityExtractor;
using Talk.Tokenisers;

namespace Talk
{

    internal class Program
    {
        private static IServiceProvider _serviceProvider;

        static void Main(string[] args)
        {

            Console.WriteLine("Lets Talk!");
            var talkConfig = BuildConfiguration();

            var services = new ServiceCollection()
               .AddTransient<IEntityTokeniser, DateTokeniser>()
               .AddTransient<IEntityTokeniser, KeywordTokeniser>()
               .AddTransient<IEntityTokeniser, PropertyKeywordTokeniser>()
               .AddTransient<IEntityTokeniser, AmountTokeniser>()
               .AddSingleton<IDialogConfig>(talkConfig);
               

            services.AddLogging(loggingBuilder =>
            {
                //loggingBuilder.AddApplicationInsights();
                loggingBuilder.AddConsole();
            });

            _serviceProvider = services.BuildServiceProvider();

            var logger = _serviceProvider.GetService<ILogger>();

            StepConfig step1 = new StepConfig()
            {
                Name = "GetDetails",
                CompletePrompt = "complete",
                InCompleteSinglePrompt = "complete_single",
                InCompleteManyPrompt = "complete_many",
                InitialPrompt = "Hi [Person:FirstName], a minimum payment of £[MinimumPayment] is due on your credit card.\nWe can accept payments up to £[MaximumPayment].\nPlease reply with you DoB, amount and when you can pay",
                MessageTemplates = new Dictionary<string, string>
                {
                    { "complete","Ok, I think thats everything. (on to confirmation)" },
                    { "complete_single","I just need to know " },
                    { "complete_many","I still need the follow information:" },
                    { "neg_escalation", "Escalated!!" },
                    { "pos_escalation", "Escalated!!" },
                    { "future_escalation", "Escalated!!" },
                    { "got_dob", "Your birthday is [Birthday]" },
                    { "got_payment_day", "You intend to pay on [PaymentDay]" },
                    { "got_payment_amount", "You intend to pay [PaymentAmount]" },
                    { "got_payment_toolow", "Your payment amount is too low, it should be between [MinimumPayment] and [MaximumPayment], let me know how much you can pay" },
                    { "got_payment_toohi", "Your payment amount is too high, it should be between [MinimumPayment] and [MaximumPayment], let me know how much you can pay" },
                    { "missing_dob", "your DoB, this is for security" },
                    { "missing_payment", "a payment amount between [MinimumPayment] and [MaximumPayment]" },
                    { "missing_when", "when can you make this payment" }
                },
                DataToCollect = new List<CollectProperty>
                {
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="neg_escalation",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="NegTacticalToken" }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="neg_escalation",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="NegIntentToken" }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="pos_escalation",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="PosTacticalToken" }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="future_escalation",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="DateToken", AnySubtypes=new string[] { "Future" } }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Collect,
                          CapturedTemplate="got_dob",
                          PropertyName = "Birthday",
                          PromptTemplate="missing_dob",
                          Expression=new TokenMatchExpression{ Token="DateToken", AnySubtypes = new string[] { "Birthday" } }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Warning,
                          CapturedTemplate="got_payment_toolow",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { "<MinimumPayment" } }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Warning,
                          CapturedTemplate="got_payment_toohi",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { ">MaximumPayment" } }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Collect,
                          CapturedTemplate="got_payment_day",
                          PropertyName = "PaymentDay",
                          PromptTemplate="missing_when",
                          Expression=new TokenMatchExpression{ Token="DateToken", AnySubtypes = new string[] { "Today" } }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Collect,
                          CapturedTemplate="got_payment_amount",
                          PropertyName = "PaymentAmount",
                          PromptTemplate="missing_payment",
                          Expression=new TokenMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { "=MinimumPayment", ">MinimumPayment" } }
                     }
                },
            };

            StepConfig step2 = new StepConfig()
            {
                Name = "ConfirmPayment",
                CompletePrompt = "complete",
                InCompleteSinglePrompt = "complete_single",
                InCompleteManyPrompt = "complete_many",
                InitialPrompt = "So please confirm you can make a payment of [PaymentAmount] on [PaymentDate] using card ending in [Last4Card]",
                MessageTemplates = new Dictionary<string, string>
                {
                    { "complete","Ok, we will arrange payment now. Thank you." },
                    { "complete_single","I just need to know " },
                    { "neg_escalation", "Escalated!!" },
                    { "pos_escalation", "Escalated!!" },
                    { "future_escalation", "Escalated!!" },
                    { "got_payment_confirm", "got_payment_day [PaymentDay]" },
                    { "missing_dob", "your DoB, this is for security" },
                    { "missing_payment", "a payment amount between [MinimumPayment] and [MaximumPayment] " },
                    { "missing_when", "when can you make this payment" }
                },
                DataToCollect = new List<CollectProperty>
                {
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="neg_escalation",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="NegTacticalToken" }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="neg_escalation",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="NegIntentToken" }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="pos_escalation",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="PosTacticalToken" }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="future_escalation",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="DateToken", AnySubtypes=new string[] { "Future" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Collect,
                          CapturedTemplate="got_dob",
                          PropertyName = "Birthday",
                          PromptTemplate="missing_dob",
                          Expression=new TokenMatchExpression{ Token="DateToken", AnySubtypes = new string[] { "Birthday" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Warning,
                          CapturedTemplate="got_payment_toolow",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { "<MinimumPayment" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Warning,
                          CapturedTemplate="got_payment_toohi",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { ">MaximumPayment" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Collect,
                          CapturedTemplate="got_payment_day",
                          PropertyName = "PaymentDay",
                          PromptTemplate="missing_when",
                          Expression=new TokenMatchExpression{ Token="DateToken", AnySubtypes = new string[] { "Today" } }
                     },
                     new CollectProperty
                     {
                          Result = CollectProperty.CollectionResult.Collect,
                          CapturedTemplate="got_payment_amount",
                          PropertyName = "PaymentAmount",
                          PromptTemplate="missing_payment",
                          Expression=new TokenMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { "=MinimumPayment", ">MinimumPayment" } }
                     }
                },
            };

            TalkContext context = new TalkContext
            {
                Properties = new Dictionary<string, object>
                {
                    { "Birthday", new DateTime(1999, 3, 3) },
                    { "Person:FirstName", "Marcus" },
                    { "Person:GivenName", "Poulton" },
                    { "MinimumPayment", 12.0 },
                    { "MaximumPayment", 24.0 },
                    { "Last4Card", 1234.0 },
                    { "DueDate", DateTime.Today.AddDays(1) },
                },
                Steps = new StepConfig[] { step1, step2 },
                CurrentStep = step1
            };

            Microsoft.ApplicationInsights.TelemetryClient client = new Microsoft.ApplicationInsights.TelemetryClient();
            client.TrackEvent("Hello", new Dictionary<string, string> { { "Talk", "Startup" } });

            var properties = new Dictionary<string, string> {
                { "ConversationId", "12" }
            };

            client.TrackTrace("Hello", properties );
            client.Flush();

            //logger.AppLogInfo($"Processing fake news message", properties);

            //PerformStep(context);

            //var list = Talk.ParseText(context.Properties, "please take money 20 3rd march 1999 hate this stuff paid marcus poulton", _serviceProvider);
            var list = Parser.ParseText(context.Properties, "20 3 3 1999 100", _serviceProvider);
            Parser.ListFlattenedTokens(list);

            var best = DialogEngine.MostLikely(list, step1.DataToCollect);

        }

        private static void PerformStep(TalkContext context)
        {
            bool quit = false;
            var nextPrompt = context.CurrentStep.InitialPrompt;
            do
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(DialogEngine.Substitute(nextPrompt, context.Properties));

                Console.ForegroundColor = ConsoleColor.White;
                var quest = Console.ReadLine();
                if (string.IsNullOrEmpty(quest))
                    break;

                Console.ForegroundColor = ConsoleColor.Gray;

                (quit, nextPrompt) = DialogEngine.ProcessResponse(context, quest, _serviceProvider);

            } while (!quit);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(nextPrompt);
        }


        public static DialogConfig BuildConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            // get settings from config manager into 
            var talkConfig = config.Get<DialogConfig>();

            if (talkConfig == null)
                throw new ApplicationException("Configuration error: Could not instantiate app settings");

            return talkConfig;
        }
    }

    public static class LogUtil
    {
        public static void AppLogInfo(this ILogger logger, string message, Dictionary<string, string> properties = null)
        {
            Microsoft.ApplicationInsights.TelemetryClient client = new Microsoft.ApplicationInsights.TelemetryClient();
            client.TrackTrace(message, SeverityLevel.Information, properties);
            logger.LogInformation(message);
        }
    }


}
