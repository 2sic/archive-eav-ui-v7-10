using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Exceptions;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Delegate to get Entities when needed
	/// </summary>
	public delegate IDictionary<int, IEntity> GetListDelegate();

    // 2015-06-14 2dm experimenting with Tag:PureEntitiesList
    public delegate IEnumerable<IEntity> GetEntitiesDelegate(); 

	/// <summary>
	/// A DataStream to get Entities when needed
	/// </summary>
	public class DataStream : IDataStream, IDataStreamLight
	{
		private readonly GetListDelegate _listDelegate;
	    private readonly GetEntitiesDelegate _entitiesDelegate;

		/// <summary>
		/// Constructs a new DataStream
		/// </summary>
		/// <param name="source">The DataSource providing Entities when needed</param>
		/// <param name="name">Name of this Stream</param>
		/// <param name="listDelegate">Function which gets Entities</param>
		public DataStream(IDataSource source, string name, GetListDelegate listDelegate, GetEntitiesDelegate entitiesDelegate = null)
		{
			Source = source;
			Name = name;
			_listDelegate = listDelegate;
		    _entitiesDelegate = entitiesDelegate;
		}


        ///// <summary>
        ///// Newer syntaxt to construct a new DataStream
        ///// Still experimental
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="name"></param>
        ///// <param name="entitiesDelegate"></param>
        //public DataStream(IDataSource source, string name, GetEntitiesDelegate entitiesDelegate)
        //{
        //    Source = source;
        //    Name = name;
        //    _entitiesDelegate = entitiesDelegate;
        //}

		public IDictionary<int, IEntity> List
		{
			get
			{
                // new version to build upon the simple list, if a simple list was provided instead Tag:PureEntitiesList
			    if (_entitiesDelegate != null)
			        return (this as IDataStreamLight).List.ToDictionary(e => e.EntityId, e => e);

			    try
			    {
			        var getList = new GetListDelegate(_listDelegate);
			        return getList();
			    }
			    catch (InvalidOperationException ex)
			    {
			        // this is a special exeption - for example when using SQL. Pass it on to enable proper testing
			        throw ex;
			    }
                //catch (DataSourceException ex)
                //{
                //    throw ex;
                //}
				catch (Exception ex)
				{
					throw new Exception(string.Format("Error getting List of Stream.\nStream Name: {0}\nDataSource Name: {1}", Name, Source.Name), ex);
				}
			}
		}


        // 2015-06-14 test 2dm to get only entities without the dictionary-setup Tag:PureEntitiesList
	    IEnumerable<IEntity> IDataStreamLight.List
	    {
	        get
	        {
                // try to use the built-in Entities-Delegate, but if not defined...
	            if (_entitiesDelegate != null)
	            {
	                try
	                {
	                    var getEntitiesDelegate = new GetEntitiesDelegate(_entitiesDelegate);
	                    return getEntitiesDelegate();
	                }
	                catch (InvalidOperationException ex)
	                {
	                    // this is a special exeption - for example when using SQL. Pass it on to enable proper testing
	                    throw ex;
	                }
	                catch (Exception ex)
	                {
	                    throw new Exception(
	                        string.Format("Error getting List of Stream.\nStream Name: {0}\nDataSource Name: {1}", Name,
	                            Source.Name), ex);
	                }
	            }

                // if no specific entities-delegate defined, use the main delegate
	            else
	            {
	                return List.Select(x => x.Value);
	            }
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
