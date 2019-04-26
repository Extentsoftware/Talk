using System.Collections.Generic;

namespace Talk.Dialog
{
    /// <summary>
    /// List of keywords in a specific category
    /// </summary>
    public class KeywordList
    {
        public string Category { get; set; }
        public List<string> Items { get; set; } = new List<string>();
    }
}
