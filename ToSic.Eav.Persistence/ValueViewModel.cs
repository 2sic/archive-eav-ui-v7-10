namespace ToSic.Eav.Data
{
	///// <summary>
	///// Extended Attribute Model for internal Cache
	///// </summary>
	//internal class AttributeDefinition : AttributeBase
	//{
	//    public int AttributeId { get; set; }

	//    public AttributeDefinition(string name, string type, bool isTitle, int attributeId)
	//        : base(name, type, isTitle)
	//    {
	//        AttributeId = attributeId;
	//    }
	//}


	/// <summary>
	/// Represents a Value
	/// </summary>
	public class ValueViewModel
	{
		/// <summary>
		/// Gets or sets the Value
		/// </summary>
		public object Value { get; set; }
		/// <summary>
		/// Gets or sets the internal ValueId
		/// </summary>
		public int? ValueId { get; set; }
		/// <summary>
		/// Gets or sets whether the Value is read only (means shared from another Language)
		/// </summary>
		public bool ReadOnly { get; set; }
	}

}