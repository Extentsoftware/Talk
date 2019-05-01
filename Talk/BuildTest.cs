using System;
using System.Collections.Generic;
using Vanquis.Digital.Ivan.Dialog.Model;
using Vanquis.Digital.Ivan.Dialog.Talk.TestConsole;
using static Vanquis.Digital.Ivan.Dialog.Talk.DialogEngine;

namespace Talk
{
    internal static class BuildTest
    {
        internal static List<DialogTest> Build()
        {
            List<DialogTest> tests = new List<DialogTest>
            {
                // 
                new DialogTest
                {
                    Description = "Happy path - incorrect dob on first pass",
                    IntentGroup = "PreDelinquent",
                    CurrentIntent = "PreDelinquentInitial",
                    Responses = new List<Response> {
                        new Response{ Human = null,                 IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="InitialPrompt" } },
                        new Response{ Human = "3/3/88",             IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "3/3/99",             IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "10",                 IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "12",                 IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "today",              IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new NextStepAction()},
                        new Response{ Human = null,                 IntentGroup = "PreDelinquent", CurrentIntent = "ConfirmPayment", Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "yes",                IntentGroup = "PreDelinquent", CurrentIntent = "ConfirmPayment", Bot = new NextStepAction()}
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
                // 
                new DialogTest
                {
                    Description = "Escalate - incorrect dob > 2",
                    IntentGroup = "PreDelinquent",
                    CurrentIntent = "PreDelinquentInitial",
                    Responses = new List<Response> {
                        new Response{ Human = null,                 IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="InitialPrompt" } },
                        new Response{ Human = "3/3/88",             IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "4/4/99",             IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "3/3/10",             IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new FailAction() },
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
                // 
                new DialogTest
                {
                    Description = "Happy path - missing dob on first pass",
                    IntentGroup = "PreDelinquent",
                    CurrentIntent = "PreDelinquentInitial",
                    Responses = new List<Response> {
                        new Response{ Human = null,                 IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="InitialPrompt" } },
                        new Response{ Human = "12 today",           IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "3/3/99",             IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new NextStepAction()},
                        new Response{ Human = null,                 IntentGroup = "PreDelinquent", CurrentIntent = "ConfirmPayment", Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "yes",                IntentGroup = "PreDelinquent", CurrentIntent = "ConfirmPayment", Bot = new NextStepAction()}
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
                //
                new DialogTest
                {
                     Description = "Escalate path - single step - negative",
                     IntentGroup = "PreDelinquent",
                     CurrentIntent = "PreDelinquentInitial",
                     Responses = new List<Response> {
                        new Response{ Human = null,                     IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "3/3/99 12 brain tumour", IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new FailAction() },
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
                //
                new DialogTest
                {
                     Description = "Escalate path - single step future date",
                     IntentGroup = "PreDelinquent",
                     CurrentIntent = "PreDelinquentInitial",
                     Responses = new List<Response> {
                        new Response{ Human = null,                     IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "3/3/99 12 12/12/2099",   IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new FailAction() },
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
                },                // 
                new DialogTest
                {
                     Description = "escalate path - seperate steps",
                     IntentGroup = "PreDelinquent",
                     CurrentIntent = "PreDelinquentInitial",
                     Responses = new List<Response> {
                        new Response{ Human = null,             IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "3/3/99",         IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "12",             IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" }},
                        new Response{ Human = "tomorrow",       IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new FailAction()},
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

                // 
                new DialogTest
                {
                    Description = "Happy path - nice and simple",
                    IntentGroup = "PreDelinquent",
                    CurrentIntent = "PreDelinquentInitial",
                    Responses = new List<Response> {
                        new Response{ Human = null,                 IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "3/3/99 12 today",    IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new NextStepAction()},
                        new Response{ Human = null,                 IntentGroup = "PreDelinquent", CurrentIntent = "ConfirmPayment", Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "yes",                IntentGroup = "PreDelinquent", CurrentIntent = "ConfirmPayment", Bot = new NextStepAction()}
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
                // 
                new DialogTest
                {
                    Description = "Happy path - seperate steps",
                     IntentGroup = "PreDelinquent",
                     CurrentIntent = "PreDelinquentInitial",
                     Responses = new List<Response> {
                        new Response{ Human = null,         IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "3/3/99",     IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" } },
                        new Response{ Human = "12",         IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new SayAction { Category="MoreData" }},
                        new Response{ Human = "today",      IntentGroup = "PreDelinquent", CurrentIntent = "PreDelinquentInitial", Bot = new NextStepAction()},
                        new Response{ Human = null,         IntentGroup = "PreDelinquent", CurrentIntent = "ConfirmPayment", Bot = new SayAction { Category="InitialPrompt" }  },
                        new Response{ Human = "yes",        IntentGroup = "PreDelinquent", CurrentIntent = "ConfirmPayment", Bot = new NextStepAction()}
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
                // 
            };

            return tests;
        }
    }
}
