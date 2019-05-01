using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Vanquis.Digital.Ivan.Dialog.Model;
using Vanquis.Digital.Ivan.Dialog.Tokenisers;
using static Vanquis.Digital.Ivan.Dialog.Talk.DialogEngine;

namespace Vanquis.Digital.Ivan.Dialog.Talk.TestConsole
{
    public class DialogTest
    {
        public string Description;
        public string IntentGroup;
        public string CurrentIntent;
        public List<Response> Responses;
        public Dictionary<string, object> Properties;
    }

    public class Response
    {
        public string Human;
        public TalkAction Bot;
        public string IntentGroup;
        public string CurrentIntent;
    }

    public static partial class DialogTestEngine
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

        public static bool ExecuteBulkTest(IDialogConfig config, IEnumerable<IEntityTokeniser> tokenisers, List<DialogTest> tests)
        {

            foreach (var test in tests)
            {
                TalkContext context = new TalkContext
                {
                    IntentGroup = test.IntentGroup,
                    CurrentIntent = test.CurrentIntent,
                    Properties = test.Properties
                };

                var passed = ExecuteTest(test, config, context, tokenisers);
                if (!passed)
                    return false;
            }

            // all passed
            return true;
        }

        public static bool ExecuteTest(DialogTest test, IDialogConfig config, TalkContext context, IEnumerable<IEntityTokeniser> tokenisers)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine($"Executing: {test.Description}");
            var botres = new string(' ', 1);
            var humres = new string(' ', 30);
            var botinfo = new string(' ', 60);

            var contextJson = JsonConvert.SerializeObject(context, jsonsettings);

            foreach (var response in test.Responses)
            {
                // display current intent
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine($"{botinfo} {context.IntentGroup}:{context.CurrentIntent} data:{context.CollectedData.Count}");

                if (context.IntentGroup != response.IntentGroup || context.CurrentIntent != response.CurrentIntent)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine($"Test Failed: Incorrect intent: Expected {response.IntentGroup}:{response.CurrentIntent} but got {context.IntentGroup}:{context.CurrentIntent}");
                    return false;
                }

                context = JsonConvert.DeserializeObject<TalkContext>(contextJson, jsonsettings);

                if (!string.IsNullOrEmpty(response.Human))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine($"Human:                     {response.Human}");
                }

                // get bot response
                var action = ProcessResponse(response.Human, config, context, tokenisers);

                if (action is SayAction sayAction)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine($"Bot: {botres} {sayAction.Prompt}");

                    if (typeof(SayAction) != response.Bot.GetType())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.WriteLine($"Test Failed: Expected Bot response of type {response.Bot.GetType().Name} but got {action.GetType().Name}");
                        return false;
                    }

                    // check we have the right cetagory of response
                    var expected_category = ((SayAction)response.Bot).Category;
                    if (sayAction.Category != expected_category)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.WriteLine($"Test Failed: Expected Bot category of {expected_category} but got {sayAction.Category}");
                        return false;
                    }
                }

                if (action is FailAction failAction)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine($"Bot Fail: {botres} {failAction.Reason} {string.Join(",", failAction.Rejections.Select(x=>x.PropertyName))}");

                    if (typeof(FailAction) != response.Bot.GetType())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.WriteLine($"Test Failed: Expected Bot response of type {response.Bot.GetType().Name} but got {action.GetType().Name}");
                        return false;
                    }

                    FailDefaultAction(config, context);
                }

                if (action is NextStepAction nextAction)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine($"Bot Step: {botres} {nextAction.Prompt}");

                    if (typeof(NextStepAction) != response.Bot.GetType())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.WriteLine($"Test Failed: Expected Bot response of type {response.Bot.GetType().Name} but got {action.GetType().Name}");
                        return false;
                    }

                    NextStepDefaultAction(config, context);
                }

                // save context in json
                contextJson = JsonConvert.SerializeObject(context, jsonsettings);

            }


            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"Test Passed\n\n");

            return true;
        }
    }
}
