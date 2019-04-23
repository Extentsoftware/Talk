namespace Talk
{
    public class CollectProperty
    {
        public enum CollectionResult
        {
            Ignore,
            Collect,
            Warning,
            Fail
        };

        public CollectionResult Result;
        public string MessageTemplate;
        public PropertyMatchExpression Expression;
        public string PropertyName;
        public bool Optional;
        public string Prompt;
    }
}
