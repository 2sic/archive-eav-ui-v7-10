using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// A DataSource that filters Entities by Ids
	/// </summary>
	public class EntityIdFilter : BaseDataSource
	{
		public override string Name { get { return "EntityIdFilter"; } }

		/// <summary>
		/// Constructs a new EntityIdFilter
		/// </summary>
		public EntityIdFilter()
		{
			Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, GetEntities));
			Configuration.Add("EntityIds", "[Settings:EntityIds]");
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

			var result = entityIds.Distinct().Where(originals.ContainsKey).ToDictionary(id => id, id => originals[id]);

			return result;
		}
	}
}