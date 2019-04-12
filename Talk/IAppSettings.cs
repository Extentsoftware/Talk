namespace Talk
{
    public interface IAppSettings
    {
        string[] DateCleanupFormats { get; set; }
        string[] DateFormats { get; set; }
        string[] TodayDateWords { get; set; }
        string[] NegativeIntentWords { get; set; }
        string[] PositiveEscalationWords { get; set; }
        string[] NegativeTacticalEscalationWords { get; set; }
        string TestData { get; set; }
    }
}
