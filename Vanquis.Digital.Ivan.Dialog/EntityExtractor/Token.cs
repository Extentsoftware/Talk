using System.Collections.Generic;

namespace Vanquis.Digital.Ivan.Dialog.EntityExtractor
{
    public abstract class Token
    {
        /// <summary>
        /// position in the original text stream of the capture text
        /// </summary>
        public int Pos;

        /// <summary>
        /// length of the original text 
        /// </summary>
        public int Length;

        /// <summary>
        /// capture text causing this token to be emitted
        /// </summary>
        public string Text;

        /// <summary>
        /// subtype of this token
        /// </summary>
        public List<string> Subtypes = new List<string>();

        public override string ToString()
        {
            return $"[{Text}]({GetType().Name} {string.Join(" & ", Subtypes)})";
        }
    }

}
