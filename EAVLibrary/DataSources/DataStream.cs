using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Delegate to get Entities when needed
	/// </summary>
	public delegate IDictionary<int, IEntity> GetListDelegate();

	/// <summary>
	/// A DataStream to get Entities when needed
	/// </summary>
	public class DataStream : IDataStream
	{
		private readonly GetListDelegate _listDelegate;

		/// <summary>
		/// Constructs a new DataStream
		/// </summary>
		/// <param name="source">The DataSource providing Entities when needed</param>
		/// <param name="name">Name of this Stream</param>
		/// <param name="listDelegate">Function which gets Entities</param>
		public DataStream(IDataSource source, string name, GetListDelegate listDelegate)
		{
			Source = source;
			Name = name;
			_listDelegate = listDelegate;
		}

		public IDictionary<int, IEntity> List
		{
			get
			{
				var getList = new GetListDelegate(_listDelegate);
				return getList();
			}
		}

		/// <summary>
		/// Get Entities based on a list of Ids
		/// </summary>
		/// <param name="entityIds">Array of EntityIds</param>
		public IDictionary<int, IEntity> GetEntities(int[] entityIds)
		{
			if (!Source.Ready)
				throw new Exception("Data Source Not Ready");

			var originals = List;
			return entityIds.Distinct().Where(originals.ContainsKey).ToDictionary(id => id, id => originals[id]);
		}

		public IDataSource Source { get; set; }

		public string Name { get; private set; }
	}
}
