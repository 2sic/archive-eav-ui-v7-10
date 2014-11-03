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
			var source = DataSource.GetInitialDataSource(appId: appId);
			var metaDataSource = DataSource.GetMetaDataSource(source.ZoneId, source.AppId);	// ToDo: Validate change/extension with zoneId and appId Parameter

			var dataPipeline = source[DataSource.DefaultStreamName].List[pipelineEntityId];
			var dataPipelineParts = metaDataSource.GetAssignedEntities(DataSource.AssignmentObjectTypeIdDataPipeline, dataPipeline.EntityGuid);

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
	}
}