using System.Collections.Generic;

namespace Talk
{
    public abstract class Token
    {
        public int Pos;

        public int Length;

        public string Text;

        public List<string> Subtypes = new List<string>();

        public override string ToString()
        {
            return $"[{Text}]({GetType().Name} {string.Join(" & ", Subtypes)})";
        }
    }

}
