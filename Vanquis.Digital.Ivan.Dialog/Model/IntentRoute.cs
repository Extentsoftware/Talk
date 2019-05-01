namespace Vanquis.Digital.Ivan.Dialog.Model
{
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
}
