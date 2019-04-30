namespace Vanquis.Digital.Ivan.Dialog.Model
{
    /// <summary>
    /// configuration item that specifies a prroperty to capture
    /// </summary>
    public class CollectProperty
    {
        public enum CollectionResult
        {
            /// <summary>
            /// throw away this item, its not important
            /// </summary>
            Ignore,

            /// <summary>
            /// this item must be collected from the token stream
            /// </summary>
            Collect,

            /// <summary>
            /// Raise a warning if this item is captured from the token stream
            /// </summary>
            Warning,

            /// <summary>
            /// fail the token stream if this item is captured
            /// </summary>
            Fail
        };

        /// <summary>
        /// the importance of capturing this token
        /// </summary>
        public CollectionResult Result;

        /// <summary>
        /// message template to emit if this item has been captured. 
        /// leave blank to silently capture his item else
        /// this will be repeated back to human
        /// </summary>
        public string CapturedTemplate;

        /// <summary>
        /// Rule that determines how to capture this item from the token list
        /// </summary>
        public TokenMatchExpression Expression;

        /// <summary>
        /// the name of the property to capture (Capture) or delete (Warning)
        /// </summary>
        public string PropertyName;

        /// <summary>
        /// flag to indicate that this property is optional
        /// </summary>
        public bool Optional;

        /// <summary>
        /// template to emit to ask for this property
        /// </summary>
        public string PromptTemplate;

        /// <summary>
        /// value indicates how significant the detection of this item is.
        /// The value is used to calculate the relative probability of a token stream
        /// from a list of candidate token stream
        /// </summary>
        public double Weight;
    }
}
