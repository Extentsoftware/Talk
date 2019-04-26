using System;
using System.Collections.Generic;
using System.Text;

namespace Talk.Dialog
{
    public class DialogConfig : IDialogConfig
    {
        public string[] DateCleanupFormats { get; set; }
        public string[] DateFormats { get; set; }
        public string[] TodayDateWords { get; set; }
        public KeywordList[] Keywords { get; set; }
        public string TestData { get; set; }
    }
}
