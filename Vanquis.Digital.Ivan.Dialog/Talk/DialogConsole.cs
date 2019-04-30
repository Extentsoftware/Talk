using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Vanquis.Digital.Ivan.Dialog.Model;
using Vanquis.Digital.Ivan.Dialog.Tokenisers;
using static Vanquis.Digital.Ivan.Dialog.Talk.DialogEngine;

namespace Vanquis.Digital.Ivan.Dialog.Talk
{
    public static class DialogConsole
    {
        private static readonly JsonSerializerSettings jsonsettings = new JsonSerializerSettings()
        {
            MaxDepth = 1000,
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Include,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };


        public class Response
        {
            public string Human;
            public TalkAction Bot;
        }

        public class DialogTest
        {
            public string IntentGroup;
            public string CurrentIntent;
            public List<Response> Responses;
            public Dictionary<string, object> Properties;
        }

        public static bool ExecuteBulkTest(IDialogConfig config, IEnumerable<IEntityTokeniser> tokenisers)
        {
            List<DialogTest> tests = new List<DialogTest>
            {
                new DialogTest
                {
                    IntentGroup = "PreDelinquent",
                    CurrentIntent = "PreDelinquentInitial",
                    Responses = new List<Response> {
                        new Response{ Human = null, Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "3/3/99 12 today", Bot = new NextStepAction()},
                        new Response{ Human = "yes", Bot = new NextStepAction()}
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "Birthday", new DateTime(1999, 3, 3).Date },
                        { "Person:FirstName", "Marcus" },
                        { "Person:GivenName", "Poulton" },
                        { "MinimumPayment", 12.0 },
                        { "MaximumPayment", 24.0 },
                        { "Last4Card", 1234.0 },
                        { "DueDate", DateTime.Today.AddDays(1) },
                    }
                },
                new DialogTest
                {
                     IntentGroup = "PreDelinquent",
                     CurrentIntent = "PreDelinquentInitial",
                     Responses = new List<Response> {
                        new Response{ Human = null, Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "3/3/99", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "12", Bot = new SayAction { Category="MoreData" }},
                        new Response{ Human = "today", Bot = new NextStepAction()},
                        new Response{ Human = "yes", Bot = new NextStepAction()}
                     },
                     Properties = new Dictionary<string, object>
                     {
                        { "Birthday", new DateTime(1999, 3, 3).Date },
                        { "Person:FirstName", "Marcus" },
                        { "Person:GivenName", "Poulton" },
                        { "MinimumPayment", 12.0 },
                        { "MaximumPayment", 24.0 },
                        { "Last4Card", 1234.0 },
                        { "DueDate", DateTime.Today.AddDays(1) },
                     }
                }
            };

            foreach( var test in tests)
            {
                TalkContext context = new TalkContext
                {
                    IntentGroup = test.IntentGroup,
                    CurrentIntent = test.CurrentIntent,
                    Properties = test.Properties
                };

                var passed = ExecuteTest(test.Responses, config, context, tokenisers);
                if (!passed)
                    return false;
            }

            // all passed
            return true;
        }
    
        public static bool ExecuteTest(List<Response> responses, IDialogConfig config, TalkContext context, IEnumerable<IEntityTokeniser> tokenisers)
        {
            var contextJson = JsonConvert.SerializeObject(context, jsonsettings);

            foreach (var response in responses)
            {
                context = JsonConvert.DeserializeObject<TalkContext>(contextJson, jsonsettings);

                var action = ProcessResponse(response.Human, config, context, tokenisers);

                contextJson = JsonConvert.SerializeObject(context, jsonsettings);

                if (action is SayAction sayAction)
                {
                    // check we have the right cetagory of response
                    if (sayAction.Category != ((SayAction)response.Bot).Category)
                        return false;
                }

                if (action is FailAction failAction)
                {
                    if (response.Bot.GetType() != typeof(FailAction))
                        return false;
                    FailDefaultAction(config, context);
                }

                if (action is NextStepAction nextAction)
                {
                    if (response.Bot.GetType() != typeof(NextStepAction))
                        return false;
                    NextStepDefaultAction(config, context);
                }
            }

            return true;
        }

        public static void ExecuteAsConsole(IDialogConfig config, TalkContext context, IEnumerable<IEntityTokeniser> tokenisers)
        {            
            PerformStep(
                config,
                context,
                tokenisers,
                (x) => {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(x);
                },
                () => {
                    // receive
                    Console.ForegroundColor = ConsoleColor.White;
                    return Console.ReadLine();
                }
            );
        }
    }
}
