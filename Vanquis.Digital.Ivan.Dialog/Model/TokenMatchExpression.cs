namespace Vanquis.Digital.Ivan.Dialog.Model
{
    /// <summary>
    /// a match expression that matches a specific token and optionally zero or more sub types
    /// </summary>
    public class TokenMatchExpression
    {
        /// <summary>
        /// token type to match
        /// </summary>
        public string Token;

        /// <summary>
        /// optional list of subtypes that must also match
        /// </summary>
        public string[] AnySubtypes;

        /// <summary>
        /// optional list of subtypes that must not match
        /// </summary>
        public string[] MustNotHave;
    }
}
