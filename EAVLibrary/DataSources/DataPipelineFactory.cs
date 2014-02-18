using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ToSic.Eav.DataSources.Tokens;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Factory to create a Data Pipeline
	/// </summary>
	public class DataPipelineFactory
	{
		private static readonly Regex WireRegex = new Regex("(?<SourceId>.*):(?<SourceOutName>.*)>(?<TargetId>.*):(?<TargetInName>.*)");

		/// <summary>
		/// Creates a DataSource from a PipelineEntity for specified Zone and App
		/// </summary>
		/// <param name="zoneId">ZoneId to use</param>
		/// <param name="appId">AppId to use</param>
		/// <param name="pipelineEntityId">EntityId of the Entity describing the Pipeline</param>
		/// <param name="configurationPropertyAccesses">Property Providers for configurable DataSources</param>
		/// <returns>A single DataSource Out with wirings and configurations loaded, ready to use</returns>
		public static IDataSource GetDataSource(int zoneId, int appId, int pipelineEntityId, IPropertyAccess[] configurationPropertyAccesses)
		{
			var source = DataSource.GetInitialDataSource(DataSource.DefaultZoneId, DataSource.MetaDataAppId);
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
				var cettingsPropertySource = new AssignedEntityAttributePropertyAccess("settings", dataPipelinePart.EntityGuid, metaDataSource);
				configurationProvider.Sources.Add(cettingsPropertySource.Name, cettingsPropertySource);
				configurationProvider.Sources.Add(pipelineSettingsProvider.Name, pipelineSettingsProvider);

				// attach all propertyProviders
				if (configurationPropertyAccesses != null)
					foreach (var propertyProvider in configurationPropertyAccesses)
						configurationProvider.Sources.Add(propertyProvider.Name, propertyProvider);
				#endregion

				var dataSource = DataSource.GetDataSource(dataPipelinePart["PartAssemblyAndType"][0].ToString(), zoneId, appId, configurationProvider: configurationProvider);
				configurationProvider.ConfigList = dataSource.Configuration;

				pipeline.Add(dataPipelinePart.EntityGuid.ToString(), dataSource);
			}
			var outSource = new PassThrough();
			pipeline.Add("Out", outSource);
			#endregion

			#region Loop and create all Stream Wirings
			var streamWiring = dataPipeline["StreamWiring"][0].ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);

			foreach (var wire in streamWiring)
			{
				var wireMatch = WireRegex.Match(wire);
				var sourceId = wireMatch.Groups["SourceId"].Value;
				var sourceOutName = wireMatch.Groups["SourceOutName"].Value;
				var targetId = wireMatch.Groups["TargetId"].Value;
				var targetInName = wireMatch.Groups["TargetInName"].Value;

				var sourceDsrc = pipeline[sourceId];
				((IDataTarget)pipeline[targetId]).In[targetInName] = sourceDsrc.Out[sourceOutName];
			}
			#endregion

			return outSource;
		}
	}
}