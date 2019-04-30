using System.Collections.Generic;

namespace Vanquis.Digital.Ivan.Dialog.Model
{
    public class DialogConfig : IDialogConfig
    {
        public string[] DateCleanupFormats { get; set; }
        public string[] DateFormats { get; set; }
        public string[] TodayDateWords { get; set; }
        public KeywordList[] Keywords { get; set; }
        public string TestData { get; set; }
        public List<Intent> Intents { get; set; }
        public Dictionary<string, IntentGroup> IntentGroups { get; set; }
        
    }
}
