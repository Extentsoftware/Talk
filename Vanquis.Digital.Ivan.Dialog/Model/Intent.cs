using System.Collections.Generic;

namespace Vanquis.Digital.Ivan.Dialog.Model
{

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
