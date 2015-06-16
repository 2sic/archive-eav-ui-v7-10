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
	public class DataStream : IDataStream
	{
		private readonly GetListDelegate _dictionaryDelegate;
	    private readonly GetEntitiesDelegate _lightListDelegate;

		/// <summary>
		/// Constructs a new DataStream
		/// </summary>
		/// <param name="source">The DataSource providing Entities when needed</param>
		/// <param name="name">Name of this Stream</param>
		/// <param name="dictionaryDelegate">Function which gets Entities</param>
		public DataStream(IDataSource source, string name, GetListDelegate dictionaryDelegate, GetEntitiesDelegate lightListDelegate = null)
		{
			Source = source;
			Name = name;
			_dictionaryDelegate = dictionaryDelegate;
		    _lightListDelegate = lightListDelegate;
		}

	    private IDictionary<int, IEntity> _dicList; 
		public IDictionary<int, IEntity> List
		{
			get
			{
                // already retrieved? then return last result to be faster
                if (_dicList != null)
                    return _dicList;

                // new version to build upon the simple list, if a simple list was provided instead Tag:PureEntitiesList
			    if (_dictionaryDelegate == null && _lightListDelegate != null)
			        return _dicList = LightList.ToDictionary(e => e.EntityId, e => e);

			    try
			    {
			        var getList = new GetListDelegate(_dictionaryDelegate);
			        return _dicList = getList();
			    }
			    catch (InvalidOperationException ex)
			    {
			        // this is a special exeption - for example when using SQL. Pass it on to enable proper testing
			        throw ex;
			    }
				catch (Exception ex)
				{
					throw new Exception(string.Format("Error getting List of Stream.\nStream Name: {0}\nDataSource Name: {1}", Name, Source.Name), ex);
				}
			}
		}

	    private IEnumerable<IEntity> _lightList; 
        public IEnumerable<IEntity> LightList
	    {
            get
            {
                // already retrieved? then return last result to be faster
                if (_lightList != null)
                    return _lightList;

                // try to use the built-in Entities-Delegate, but if not defined, use other delegate; just make sure we test both, to prevent infinite loops
                if (_lightListDelegate == null && _dictionaryDelegate != null)
                    return _lightList = List.Select(x => x.Value);

                try
                {
                    var getEntitiesDelegate = new GetEntitiesDelegate(_lightListDelegate);
                    return _lightList = getEntitiesDelegate();
                }
                catch (InvalidOperationException ex)
                {
                    // this is a special exeption - for example when using SQL. Pass it on to enable proper testing
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error getting List of Stream.\nStream Name: {0}\nDataSource Name: {1}", Name, Source.Name), ex);
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
