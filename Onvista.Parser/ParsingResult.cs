namespace Onvista.Parser
{
    public class ParsingResult<T>
    {
        public ParsingResult(T entity, ParsingResultType resultType)
        {
            Entity = entity;
            ResultType = resultType;
        }

        public T Entity { get; set; }

        public ParsingResultType ResultType { get; set; }
    }

    public enum ParsingResultType
    {
        None = 0,
        PendingForSave = 1,
        AlreadyExists = 2,
        Saved = 3
    }
}
