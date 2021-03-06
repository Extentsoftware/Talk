﻿namespace Talk.Dialog
{
    public interface IDialogConfig
    {
        string[] DateCleanupFormats { get; set; }
        string[] DateFormats { get; set; }
        string[] TodayDateWords { get; set; }
        KeywordList[] Keywords { get; set; }
        string TestData { get; set; }
    }
}
