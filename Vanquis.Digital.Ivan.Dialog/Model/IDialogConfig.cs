using System.Collections.Generic;

namespace Vanquis.Digital.Ivan.Dialog.Model
{
    public interface IDialogConfig
    {
        string[] DateCleanupFormats { get; set; }
        string[] DateFormats { get; set; }
        string[] TodayDateWords { get; set; }
        KeywordList[] Keywords { get; set; }
        string TestData { get; set; }
        List<Intent> Intents { get; set; }
        Dictionary<string, IntentGroup> IntentGroups { get; set; }
    }
}
