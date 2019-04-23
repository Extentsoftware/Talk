using System;
using System.Collections.Generic;
using System.Text;

namespace Talk
{
    public class TalkConfig : ITalkConfig
    {
        public string[] DateCleanupFormats { get; set; }
        public string[] DateFormats { get; set; }
        public string[] TodayDateWords { get; set; }
        public KeywordList[] Keywords { get; set; }
        public string TestData { get; set; }
    }
}
