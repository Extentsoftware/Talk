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
}
