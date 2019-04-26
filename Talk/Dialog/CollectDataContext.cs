using System.Collections.Generic;

namespace Talk.Dialog
{
    public class TalkContext
    {
        public DialogConfig Config;
        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        public Dictionary<string, object> CollectedData = new Dictionary<string, object>();

        public StepConfig[] Steps;

        public StepConfig CurrentStep;
    }

    public class StepRoute
    {
        public string FromName;
        public string ToName;
        public string FailName;
    }

    public class StepConfig
    {
        public string Name;
        public List<CollectProperty> DataToCollect = new List<CollectProperty>();
        public Dictionary<string, string> MessageTemplates;
        public string InitialPrompt;
        internal string CompletePrompt;
        internal string InCompleteSinglePrompt;
        internal string InCompleteManyPrompt;
    }
}
