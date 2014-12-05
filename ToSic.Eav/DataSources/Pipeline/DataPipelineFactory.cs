using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Tokens;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Factory to create a Data Pipeline
	/// </summary>
	public class DataPipelineFactory
	{
		/// <summary>
		/// Creates a DataSource from a PipelineEntity for specified Zone and App
		/// </summary>
		/// <param name="appId">AppId to use</param>
		/// <param name="pipelineEntityId">EntityId of the Entity describing the Pipeline</param>
		/// <param name="configuration">ConfigurationProvider Provider for configurable DataSources</param>
		/// <param name="outSource">DataSource to attach the Out-Streams</param>
		/// <returns>A single DataSource Out with wirings and configurations loaded, ready to use</returns>
		public static IDataSource GetDataSource(int appId, int pipelineEntityId, ConfigurationProvider configuration, IDataSource outSource)
		{
			return GetDataSource(appId, pipelineEntityId, configuration.Sources.Select(s => s.Value), outSource);
		}

		/// <summary>
		/// Creates a DataSource from a PipelineEntity for specified Zone and App
		/// </summary>
		/// <param name="appId">AppId to use</param>
		/// <param name="pipelineEntityId">EntityId of the Entity describing the Pipeline</param>
		/// <param name="configurationPropertyAccesses">Property Providers for configurable DataSources</param>
		/// <param name="outSource">DataSource to attach the Out-Streams</param>
		/// <returns>A single DataSource Out with wirings and configurations loaded, ready to use</returns>
		public static IDataSource GetDataSource(int appId, int pipelineEntityId, IEnumerable<IPropertyAccess> configurationPropertyAccesses, IDataSource outSource)
		{
			#region Load Pipeline Entity and Pipeline Parts
			var source = DataSource.GetInitialDataSource(appId: appId);
			var metaDataSource = DataSource.GetMetaDataSource(source.ZoneId, source.AppId);	// ToDo: Validate change/extension with zoneId and appId Parameter

			var appEntities = source[DataSource.DefaultStreamName].List;
			IEntity dataPipeline;
			try
			{
				dataPipeline = appEntities[pipelineEntityId];
			}
			catch (KeyNotFoundException)
			{
				throw new Exception("PipelineEntity not found with ID " + pipelineEntityId + " on AppId " + appId);
			}
			var dataPipelineParts = metaDataSource.GetAssignedEntities(DataSource.AssignmentObjectTypeIdDataPipeline, dataPipeline.EntityGuid);
			#endregion

			var pipelineSettingsProvider = new AssignedEntityAttributePropertyAccess("pipelinesettings", dataPipeline.EntityGuid, metaDataSource);
			#region init all DataPipelineParts
			var pipeline = new Dictionary<string, IDataSource>();
			foreach (var dataPipelinePart in dataPipelineParts)
			{
				#region Init Configuration Provider
				var configurationProvider = new ConfigurationProvider();
				var settingsPropertySource = new AssignedEntityAttributePropertyAccess("settings", dataPipelinePart.EntityGuid, metaDataSource);
				configurationProvider.Sources.Add(settingsPropertySource.Name, settingsPropertySource);
				configurationProvider.Sources.Add(pipelineSettingsProvider.Name, pipelineSettingsProvider);

				// attach all propertyProviders
				if (configurationPropertyAccesses != null)
					foreach (var propertyProvider in configurationPropertyAccesses)
					{
						if (propertyProvider.Name == null)
							throw new NullReferenceException("PropertyProvider must have a Name");
						configurationProvider.Sources.Add(propertyProvider.Name, propertyProvider);
					}

				#endregion

				var dataSource = DataSource.GetDataSource(dataPipelinePart["PartAssemblyAndType"][0].ToString(), source.ZoneId, source.AppId, configurationProvider: configurationProvider);
				//configurationProvider.configList = dataSource.Configuration;

				pipeline.Add(dataPipelinePart.EntityGuid.ToString(), dataSource);
			}
			pipeline.Add("Out", outSource);
			#endregion

			#region Loop and create all Stream Wirings
			var wirings = DataPipelineWiring.Deserialize((string)dataPipeline[DataPipeline.StreamWiringAttributeName][0]);
			foreach (var wire in wirings)
			{
				var sourceDsrc = pipeline[wire.From];
				((IDataTarget)pipeline[wire.To]).In[wire.In] = sourceDsrc.Out[wire.Out];
			}
			#endregion

			return outSource;
		}

		/// <summary>
		/// Find a DataSource of a specific Type in a DataPipeline
		/// </summary>
		/// <typeparam name="T">Type of the DataSource to find</typeparam>
		/// <param name="rootDataSource">DataSource to look for In-Connections</param>
		/// <returns>DataSource of specified Type or null</returns>
		public static T FindDataSource<T>(IDataTarget rootDataSource) where T : IDataSource
		{
			foreach (var stream in rootDataSource.In)
			{
				// If type matches, return this DataSource
				if (stream.Value.Source.GetType() == typeof(T))
					return (T)stream.Value.Source;

				// Find recursive in In-Streams of this DataSource (if any)
				var dataTarget = stream.Value.Source as IDataTarget;
				if (dataTarget != null)
					return FindDataSource<T>(dataTarget);
			}

			return default(T);
		}
	}
}