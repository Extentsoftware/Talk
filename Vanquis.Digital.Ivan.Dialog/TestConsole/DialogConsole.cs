using System;
using System.Collections.Generic;
using Vanquis.Digital.Ivan.Dialog.Model;
using Vanquis.Digital.Ivan.Dialog.Tokenisers;
using static Vanquis.Digital.Ivan.Dialog.Talk.DialogEngine;

namespace Vanquis.Digital.Ivan.Dialog.Talk.TestConsole
{

    public static partial class DialogConsole
    {
        public static void ExecuteAsConsole(IDialogConfig config, TalkContext context, IEnumerable<IEntityTokeniser> tokenisers)
        {
            PerformStep(
                config,
                context,
                tokenisers,
                (x) =>
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(x);
                },
                () =>
                {
                    // receive
                    Console.ForegroundColor = ConsoleColor.White;
                    return Console.ReadLine();
                }
            );
        }

        private static void PerformStep(
           IDialogConfig config,
           TalkContext context,
           IEnumerable<IEntityTokeniser> tokenisers,
           Action<string> Say,
           Func<string> GetResponse
           )
        {
            bool quit = false;
            string humanText = null;

            do
            {
                TalkAction action = ProcessResponse(humanText, config, context, tokenisers);

                if (action is FailAction failAction)
                    FailDefaultAction(config, context);

                if (action is SayAction sayAction)
                {
                    // send
                    Say(sayAction.Prompt);

                    humanText = GetResponse();

                    if (string.IsNullOrEmpty(humanText))
                        break;

                    continue;
                }

                if (action is NextStepAction nextAction)
                {
                    // get next intent from the route
                    humanText = null;
                    NextStepDefaultAction(config, context);
                }

                if (context.CurrentIntent == null)
                    quit = true;

            } while (!quit);

        }

    }
}
