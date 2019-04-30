using System.Collections.Generic;
using Vanquis.Digital.Ivan.Dialog.EntityExtractor;

namespace Vanquis.Digital.Ivan.Dialog.Model
{
    public class TalkContext
    {
        /// <summary>
        /// list of entry properties/entities for this conversation
        /// </summary>
        public Dictionary<string, object> Properties = new Dictionary<string, object>();

        /// <summary>
        /// tokens collected during processing of human response
        /// </summary>
        public Dictionary<string, Token> CollectedData = new Dictionary<string, Token>();

        /// <summary>
        /// define the current conversation
        /// </summary>
        public string IntentGroup;

        /// <summary>
        /// the current intent within the current conversation
        /// </summary>
        public string CurrentIntent;

    }

    public class IntentGroup
    {
        public List<IntentRoute> IntentRoutes;
    }

    /// <summary>
    /// defines routing from one intent to another
    /// </summary>
    public class IntentRoute
    {
        /// <summary>
        /// source intent
        /// </summary>
        public string FromName;

        /// <summary>
        /// destination intent if following success path
        /// </summary>
        /// 
        public string ToName;
        /// <summary>
        /// destination intent if following fail path or NULL for end of conversation
        /// </summary>
        public string FailName;
    }

    /// <summary>
    /// encapsulates all logic for a single intent which captures properties
    /// </summary>
    public class Intent
    {
        /// <summary>
        /// name of this intent.
        /// </summary>
        public string Name;

        /// <summary>
        /// list of properties to extract
        /// </summary>
        public List<CollectProperty> DataToCollect = new List<CollectProperty>();

        /// <summary>
        /// list of message templates containing text to emit
        /// </summary>
        public Dictionary<string, string> MessageTemplates;

        /// <summary>
        /// Initial prompt when started by the bot
        /// </summary>
        public string InitialPrompt;

        /// <summary>
        /// completion text when successful
        /// </summary>
        public string CompletePrompt;

        /// <summary>
        /// message for when a single "Collect" property is missing
        /// </summary>
        public string InCompleteSinglePrompt;

        /// <summary>
        /// message for when multiple "Collect" properties are missing
        /// </summary>
        public string InCompleteManyPrompt;

        /// <summary>
        /// message to emit to report captured items
        /// </summary>
        public string CapturedPrompt;
    }
}
