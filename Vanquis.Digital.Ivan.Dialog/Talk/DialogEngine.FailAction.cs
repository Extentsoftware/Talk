namespace Vanquis.Digital.Ivan.Dialog.Talk
{
    public static partial class DialogEngine
    {
        /// <summary>
        /// failure
        /// </summary>
        public class FailAction : TalkAction
        {
            public string Reason { get; set; }
        }
    }

}
