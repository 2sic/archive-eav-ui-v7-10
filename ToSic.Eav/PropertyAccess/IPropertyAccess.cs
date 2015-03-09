namespace ToSic.Eav.PropertyAccess
{
	public interface IPropertyAccess
	{
		/// <summary>
		/// Gets the Name of the Property Accessor, e.g. QueryString or PipelineSettings
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Gets a Property by Name
		/// </summary>
		string GetProperty(string propertyName, string format, ref bool propertyNotFound);
	}
}