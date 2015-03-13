namespace ToSic.Eav.Data
{
	/// <summary>
	/// Represents a Relation between two entities
	/// </summary>
	public class EntityRelationshipItem
	{
		/// <summary>
		/// Initializes a new instance of the EntityRelationshipItem class.
		/// </summary>
		/// <param name="parent">Parent Entity</param>
		/// <param name="child">Child Entity</param>
		public EntityRelationshipItem(IEntity parent, IEntity child)
		{
			Parent = parent;
			Child = child;
		}

		internal IEntity Parent { get; set; }
		internal IEntity Child { get; set; }
	}
}
