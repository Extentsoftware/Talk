namespace Vanquis.Digital.Ivan.Dialog.Talk
{
    public static partial class DialogEngine
    {
        /// <summary>
        /// wait for a response from the human
        /// </summary>
        public class SayAction : TalkAction
        {
            public string Prompt { get; set; }
            public string Category { get; set; }
        }
    }

}
