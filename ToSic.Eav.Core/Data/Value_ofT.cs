namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a Value
    /// </summary>
    /// <typeparam name="T">Type of the actual Value</typeparam>
    public class Value<T> : Value, IValue<T>, IValueManagement
    {
        public T TypedContents { get; internal set; }

        public Value(T typedContents)
        {
            TypedContents = typedContents;
        }
    }
}