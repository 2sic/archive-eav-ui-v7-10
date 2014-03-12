using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav
{
	/// <summary>
	/// System to prepare data sources according to our needs.
	/// </summary>
	public class DataSource
	{
		/// <summary>
		/// Default ZoneId. Used if none is specified on the Context.
		/// </summary>
		public readonly static int DefaultZoneId = 1;
		/// <summary>
		/// AppId where MetaData (Entities) are stored.
		/// </summary>
		public readonly static int MetaDataAppId = 1;
		/// <summary>
		/// AssignmentObjectTypeId for FieldProperties (Field MetaData)
		/// </summary>
		public readonly static int AssignmentObjectTypeIdFieldProperties = 2;
		/// <summary>
		/// AssignmentObjectTypeId for DataPipelines
		/// </summary>
		public readonly static int AssignmentObjectTypeIdDataPipeline = 4;
		/// <summary>
		/// StaticName of the DataPipeline AttributeSet
		/// </summary>
		public readonly static string DataPipelinePartStaticName = "DataPipelinePart";
		/// <summary>
		/// Default In-/Out-Stream Name
		/// </summary>
		public const string DefaultStreamName = "Default";

		/// <summary>
		/// Assemble a DataSource with specified Type/Interface-Chain in reversed order.
		/// </summary>
		/// <param name="chain">Array of Full Qualified Names of DataSources</param>
		/// <param name="zoneId">ZoneId for this DataSource</param>
		/// <param name="appId">AppId for this DataSource</param>
		/// <returns>A single DataSource that has attached </returns>
		private static IDataSource AssembleDataSourceReverse(IList<string> chain, int zoneId, int appId)
		{
			var newSource = GetDataSource(chain[0], zoneId, appId);
			if (chain.Count > 1)
			{
				var source = AssembleDataSourceReverse(chain.Skip(1).ToArray(), zoneId, appId);
				((IDataTarget)newSource).Attach(source);
			}
			return newSource;
		}

		/// <summary>
		/// Get DataSource for specified sourceName/Type using Unity.
		/// </summary>
		/// <param name="sourceName">Full Qualified Type/Interface Name</param>
		/// <param name="zoneId">ZoneId for this DataSource</param>
		/// <param name="appId">AppId for this DataSource</param>
		/// <param name="upstream">In-Connection</param>
		/// <param name="configurationProvider">Provides configuration values if needed</param>
		/// <returns>A single DataSource</returns>
		public static IDataSource GetDataSource(string sourceName, int? zoneId = null, int? appId = null, IDataSource upstream = null, IConfigurationProvider configurationProvider = null)
		{
			var newDs = (BaseDataSource)Factory.Container.Resolve(Type.GetType(sourceName));
			var zoneAppId = GetZoneAppId(zoneId, appId);
			newDs.ZoneId = zoneAppId.Item1;
			newDs.AppId = zoneAppId.Item2;
			if (upstream != null)
				((IDataTarget)newDs).Attach(upstream);
			if (configurationProvider != null)
				newDs.ConfigurationProvider = configurationProvider;

			return newDs;
		}

		private static readonly string[] InitialDataSourcePipeline = { "ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav", "ToSic.Eav.DataSources.RootSources.IRootSource, ToSic.Eav" };
		/// <summary>
		/// Retunrs an ICache with IRootSource as In-Source.
		/// </summary>
		/// <param name="zoneId">ZoneId for this DataSource</param>
		/// <param name="appId">AppId for this DataSource</param>
		/// <returns>A single DataSource</returns>
		public static IDataSource GetInitialDataSource(int? zoneId = null, int? appId = null)
		{
			var zoneAppId = GetZoneAppId(zoneId, appId);

			var dataSource = AssembleDataSourceReverse(InitialDataSourcePipeline, zoneAppId.Item1, zoneAppId.Item2);

			return dataSource;
		}

		/// <summary>
		/// Resolve and validate ZoneId and AppId for specified ZoneId and/or AppId (if any)
		/// </summary>
		/// <returns>Item1 = ZoneId, Item2 = AppId</returns>
		private static Tuple<int, int> GetZoneAppId(int? zoneId, int? appId)
		{
			if (zoneId == null || appId == null)
			{
				var cache = GetCache(DefaultZoneId, MetaDataAppId);
				return cache.GetZoneAppId(zoneId, appId);
			}
			return Tuple.Create(zoneId.Value, appId.Value);
		}

		/// <summary>
		/// Get a new ICache DataSource
		/// </summary>
		/// <param name="zoneId">ZoneId for this DataSource</param>
		/// <param name="appId">AppId for this DataSource</param>
		/// <returns>A new ICache</returns>
		public static ICache GetCache(int zoneId, int? appId = null)
		{
			return (ICache)GetDataSource("ToSic.Eav.DataSources.Caches.ICache, ToSic.Eav", zoneId, appId);
		}

		/// <summary>
		/// Get DataSource having common MetaData, like Field MetaData
		/// </summary>
		/// <returns>IMetaDataSource (from ICache)</returns>
		public static IMetaDataSource GetMetaDataSource(int zoneId, int appId)
		{
			return (IMetaDataSource)GetCache(zoneId, appId);
		}
	}
}