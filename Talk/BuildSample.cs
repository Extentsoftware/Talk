using System.Collections.Generic;
using Vanquis.Digital.Ivan.Dialog.Model;


namespace Talk
{
    internal static class BuildSample
    {

        internal static IDialogConfig Build(IDialogConfig talkConfig)
        {
            Intent step1 = new Intent()
            {
                Name = "PreDelinquentInitial",
                CompletePrompt = "complete",
                CapturedPrompt = "captured",
                InCompleteSinglePrompt = "complete_single",
                InCompleteManyPrompt = "complete_many",
                InitialPrompt = "Hi [Person:FirstName], a minimum payment of £[MinimumPayment] is due on your credit card.\nWe can accept payments up to £[MaximumPayment].\nPlease reply with you DoB, amount and when you can pay",
                MessageTemplates = new Dictionary<string, string>
                {
                    { "captured", "Ok I understand " },
                    { "complete","Ok, I think thats everything. (on to confirmation)" },
                    { "complete_single","I just need to know " },
                    { "complete_many","I still need the following information:" },
                    { "negpos_escalation", "Escalated! - neg/pos escalation" },
                    { "future_escalation", "Escalated! - future_escalation" },
                    { "past_warning", "I dont recognise that date" },                    
                    { "got_dob", "Your birthday is [CapturedBirthday]" },
                    { "got_payment_day", "You intend to pay on [PaymentDay]" },
                    { "got_payment_amount", "You intend to pay £[PaymentAmount]" },
                    { "got_payment_toolow", "Your payment amount is too low" },
                    { "got_payment_toohi", "Your payment amount is too high" },
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
                          CapturedTemplate="negpos_escalation",
                          Expression=new TokenMatchExpression{ Token="KeywordToken", AnySubtypes=new string[] { "NegTacticalToken" , "NegIntentToken", "PosTacticalToken" } }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="future_escalation",
                          Expression=new TokenMatchExpression{ Token="DateToken", AnySubtypes=new string[] { "Future" } }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Warning,
                          CapturedTemplate="past_warning",
                          Expression=new TokenMatchExpression{ Token="DateToken", AnySubtypes = new string[] { "Past" }, MustNotHave = new string[] { "Birthday" } }
                     },
                     new CollectProperty
                     {
                          Weight = 5,
                          Result = CollectProperty.CollectionResult.Collect,
                          CapturedTemplate="got_dob",
                          PropertyName = "CapturedBirthday",
                          PromptTemplate="missing_dob",
                          Expression=new TokenMatchExpression{ Token="DateToken", AnySubtypes = new string[] { "Birthday" } }
                     },
                     
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Warning,
                          CapturedTemplate="got_payment_toolow",
                          PropertyName = "PaymentAmount",
                          Expression=new TokenMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { "<MinimumPayment" } }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Warning,
                          CapturedTemplate="got_payment_toohi",
                          PropertyName = "PaymentAmount",
                          Expression=new TokenMatchExpression{ Token="AmountToken", AnySubtypes = new string[] { ">MaximumPayment" } }
                     },
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Collect,
                          CapturedTemplate="got_payment_day",
                          PropertyName = "PaymentDate",
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

            Intent step2 = new Intent()
            {
                Name = "ConfirmPayment",
                CompletePrompt = "complete",
                CapturedPrompt = "",
                InCompleteSinglePrompt = "complete_single",
                InCompleteManyPrompt = "complete_many",
                InitialPrompt = "So please confirm (yes/no) that you can make a payment of £[PaymentAmount] [PaymentDate] using card ending in [Last4Card]",
                MessageTemplates = new Dictionary<string, string>
                {
                    { "complete","Ok, we will arrange payment now. Thank you." },
                    { "complete_single", "I just need to know " },
                    { "negpos_escalation", "Escalated!!" },
                    { "future_escalation", "Escalated!!" },
                    { "pay_confirm", "got_payment_day [PaymentDay]" }
                },
                DataToCollect = new List<CollectProperty>
                {
                     new CollectProperty
                     {
                          Weight = 1,
                          Result = CollectProperty.CollectionResult.Fail,
                          CapturedTemplate="negpos_escalation",
                          PropertyName = "",
                          Expression=new TokenMatchExpression{ Token="KeywordToken", AnySubtypes=new string[] { "NegTacticalToken" , "NegIntentToken", "PosTacticalToken", "RejectToken" } }
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
                          CapturedTemplate="pay_confirm",
                          PropertyName = "pay_confirm",
                          Expression=new TokenMatchExpression{ Token="KeywordToken", AnySubtypes=new string[] { "ConfirmToken" } }
                     }
                }
            };

            talkConfig.Intents = new List<Intent> { step1, step2 };

            talkConfig.IntentGroups = new Dictionary<string, IntentGroup>
            {
                {
                    "PreDelinquent",

                    new IntentGroup
                    {
                        IntentRoutes = new List<IntentRoute> {
                         new IntentRoute
                         {
                             FromName="PreDelinquentInitial",
                             ToName="ConfirmPayment",
                             FailName="Escalate",
                         },
                         new IntentRoute
                         {
                             FromName="ConfirmPayment",
                             ToName=null,
                             FailName="Escalate",
                         }
                      }
                    }
                }
            };

            return talkConfig;
        }
    }
}
