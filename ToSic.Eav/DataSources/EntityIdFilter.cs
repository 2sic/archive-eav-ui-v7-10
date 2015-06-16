using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
	[PipelineDesigner]
	public class EntityIdFilter : BaseDataSource
	{
		#region Configuration-properties
		private const string EntityIdKey = "EntityIds";
		//private const string PassThroughOnEmptyEntityIdsKey = "PassThroughOnEmptyEntityIds";

		/// <summary>
		/// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
		/// </summary>
		public string EntityIds
		{
			get { return Configuration[EntityIdKey]; }
			set { Configuration[EntityIdKey] = value; }
		}

		#endregion

		/// <summary>
		/// Constructs a new EntityIdFilter
		/// </summary>
		public EntityIdFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(EntityIdKey, "[Settings:EntityIds]");
			//Configuration.Add(PassThroughOnEmptyEntityIdsKey, "[Settings:PassThroughOnEmptyEntityIds||false]");
		}

		private IDictionary<int, IEntity> GetEntities()
		{
			var entityIds = GetDistinctArrayOfEntityIds();

		    var originals = In[DataSource.DefaultStreamName].List;

			return entityIds.Where(originals.ContainsKey).ToDictionary(id => id, id => originals[id]);
		}

        // Todo: Dangerous code. Different code which should do the same thing, not 100% reliable...?
        // won't implement this - as I assume a multi-lookup is more efficient with a full dictionary
        //private IEnumerable<IEntity> GetList()
        //{
        //    var entityIds = GetDistinctArrayOfEntityIds();

        //    var originals = In[DataSource.DefaultStreamName].LightList;

        //    var result = new List<IEntity>();

        //    foreach (var id in entityIds)
        //    {
        //        var r = originals.Where(o => o.EntityId == id).First();
        //        if(r != null) 
        //            result.Add(r);
        //    }
        //    return result;
        //}
        //    return originals.Where(o => entityIds.Contains(o.EntityId));// entityIds.Where(originals.Select(o => o));//.ToDictionary(id => id, id => originals[id]);
        //} 

	    private IEnumerable<int> GetDistinctArrayOfEntityIds()
	    {

	        EnsureConfigurationIsLoaded();

	        #region Init EntityIds from Configuration (a String)

	        int[] entityIds;
	        try
	        {
	            var configEntityIds = Configuration["EntityIds"];
	            if (string.IsNullOrWhiteSpace(configEntityIds))
	                entityIds = new int[0];
	            else
	            {
	                var lstEntityIds = new List<int>();
	                foreach (
	                    var strEntityId in
	                        Configuration["EntityIds"].Split(',').Where(strEntityId => !string.IsNullOrWhiteSpace(strEntityId)))
	                {
	                    int entityIdToAdd;
	                    if (int.TryParse(strEntityId, out entityIdToAdd))
	                        lstEntityIds.Add(entityIdToAdd);
	                }

	                entityIds = lstEntityIds.ToArray();
	            }
	        }
	        catch (Exception ex)
	        {
	            throw new Exception("Unable to load EntityIds from Configuration.", ex);
	        }

	        #endregion

            return entityIds.Distinct();
	    }
	}
}