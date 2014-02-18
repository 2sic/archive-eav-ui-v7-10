using System.Collections.Generic;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Represents a data source that can retrieve Data
	/// </summary>
	public interface IDataTarget
	{
		/// <summary>
		/// In Connections
		/// </summary>
		IDictionary<string, IDataStream> In { get; }
		
		/// <summary>
		/// Attach specified DataSource to In
		/// </summary>
		/// <param name="dataSource">DataSource to attach</param>
		void Attach(IDataSource dataSource);
	}
}
