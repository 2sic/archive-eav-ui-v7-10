using System.Collections.Generic;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Represents a DataStream
	/// </summary>
	public interface IDataStreamLight
	{
		/// <summary>
		/// Dictionary of Entites in this Stream
		/// </summary>
		IEnumerable<IEntity> List { get; }
	}
}