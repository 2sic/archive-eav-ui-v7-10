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
		private const string PassThroughtOnEmptyEntityIdsKey = "PassThroughtOnEmptyEntityIds";

		/// <summary>
		/// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
		/// </summary>
		public string EntityIds
		{
			get { return Configuration[EntityIdKey]; }
			set { Configuration[EntityIdKey] = value; }
		}

		/// <summary>
		/// Pass throught all Entities if EntityIds is empty
		/// </summary>
		public bool PassThroughtOnEmptyEntityIds
		{
			get { return bool.Parse(Configuration[PassThroughtOnEmptyEntityIdsKey]); }
			set { Configuration[PassThroughtOnEmptyEntityIdsKey] = value.ToString(); }
		}

		#endregion

		/// <summary>
		/// Constructs a new EntityIdFilter
		/// </summary>
		public EntityIdFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add(EntityIdKey, "[Settings:EntityIds]");
			Configuration.Add(PassThroughtOnEmptyEntityIdsKey, "[Settings:PassThroughtOnEmptyEntityIds||false]");
		}

		private IDictionary<int, IEntity> GetEntities()
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
					foreach (var strEntityId in Configuration["EntityIds"].Split(',').Where(strEntityId => !string.IsNullOrWhiteSpace(strEntityId)))
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

			var originals = In[DataSource.DefaultStreamName].List;

			if (entityIds.Length == 0 && PassThroughtOnEmptyEntityIds)
				return originals;

			var result = entityIds.Distinct().Where(originals.ContainsKey).ToDictionary(id => id, id => originals[id]);

			return result;
		}
	}
}