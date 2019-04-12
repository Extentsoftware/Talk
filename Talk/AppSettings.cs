using System;
using System.Collections.Generic;
using System.Text;

namespace Talk
{
    public class AppSettings : IAppSettings
    {
        public string[] DateCleanupFormats { get; set; }
        public string[] DateFormats { get; set; }
        public string[] TodayDateWords { get; set; }
        public string[] NegativeIntentWords { get; set; }
        public string[] PositiveEscalationWords { get; set; }
        public string[] NegativeTacticalEscalationWords { get; set; }
        public string TestData { get; set; }
    }
}
