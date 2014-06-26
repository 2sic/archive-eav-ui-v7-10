using System.Collections.Generic;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Represents a DataStream
	/// </summary>
	public interface IDataStream
	{
		/// <summary>
		/// Dictionary of Entites in this Stream
		/// </summary>
		IDictionary<int, IEntity> List { get; }
		/// <summary>
		/// DataSource providing the Entities
		/// </summary>
		IDataSource Source { get; }
		/// <summary>
		/// Name of this Stream
		/// </summary>
		string Name { get; }
	}
}