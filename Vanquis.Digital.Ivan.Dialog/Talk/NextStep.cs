namespace Vanquis.Digital.Ivan.Dialog.Talk
{
    public static partial class DialogEngine
    {
        /// <summary>
        /// move to next step
        /// </summary>
        public class NextStepAction : TalkAction
        {
            public string Reason { get; set; }
        }
    }

}
