using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Vanquis.Digital.Ivan.Dialog.Model;
using Vanquis.Digital.Ivan.Dialog.Talk;
using Vanquis.Digital.Ivan.Dialog.Tokenisers;

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
                loggingBuilder.AddConsole();
            });

            _serviceProvider = services.BuildServiceProvider();

            var config = BuildSample.Build(talkConfig);

            //var list = Talk.ParseText(context.Properties, "please take money 20 3rd march 1999 hate this stuff paid marcus poulton", _serviceProvider);
            using (var scope = _serviceProvider.CreateScope())
            {
                TalkContext context = new TalkContext
                {
                    IntentGroup = "PreDelinquent",
                    CurrentIntent = "PreDelinquentInitial",
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
                };

                var configtext = JsonConvert.SerializeObject(config);
                var contexttext = JsonConvert.SerializeObject(context);

                var tokenisers = scope.ServiceProvider.GetServices<IEntityTokeniser>();

                var passed = DialogConsole.ExecuteBulkTest(config, tokenisers);


                DialogConsole.ExecuteAsConsole(config, context, tokenisers);
            }
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
}
