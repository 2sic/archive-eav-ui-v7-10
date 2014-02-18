using System;
using System.Collections.Generic;

namespace ToSic.Eav
{
	/// <summary>
	/// Represents an Entity
	/// </summary>
	public interface IEntity
	{
		/// <summary>
		/// Gets the EntityId
		/// </summary>
		int EntityId { get; }
		/// <summary>
		/// Gets the EntityGuid
		/// </summary>
		Guid EntityGuid { get; }
		/// <summary>
		/// Gets a Dictionary having all Attributes having a value
		/// </summary>
		Dictionary<string, IAttribute> Attributes { get; }
		/// <summary>
		/// Gets the ContentType of this Entity
		/// </summary>
		IContentType Type { get; }
		/// <summary>
		/// Gets the Title-Attribute
		/// </summary>
		IAttribute Title { get; }
		/// <summary>
		/// Gets an Attribute by its StaticName
		/// </summary>
		/// <param name="attributeName">StaticName of the Attribute</param>
		IAttribute this[string attributeName] { get; }
		/// <summary>
		/// Get Related entities
		/// </summary>
		RelationshipManager Relationships { get; }
	}
}