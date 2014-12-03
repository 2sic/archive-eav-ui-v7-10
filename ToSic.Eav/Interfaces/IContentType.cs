namespace ToSic.Eav
{
	/// <summary>
	/// Represents a Content Type
	/// </summary>
	public interface IContentType
	{
		/// <summary>
		/// Gets the Display Name of the Content Type
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Gets the Static Name of the Content Type
		/// </summary>
		string StaticName { get; }
        /// <summary>
        /// Get the id of the Content Type (AttributeSet)
        /// </summary>
        int AttributeSetId { get; }
        /// <summary>
        /// Get the scope of the Content Type
        /// </summary>
        string Scope { get; }
	}
}