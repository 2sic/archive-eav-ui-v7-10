using System.Collections.Generic;

namespace ToSic.Eav
{
	/// <summary>
	/// Helpers to Upgrade EAV from earlier Versions
	/// </summary>
	public class VersionUpgrade
	{
		private readonly EavContext _metaDataCtx = EavContext.Instance(DataSource.DefaultZoneId, DataSource.MetaDataAppId);
		private readonly string _userName;

		public VersionUpgrade(string userName)
		{
			_userName = userName;
		}

		/// <summary>
		/// Create Pipeline Designer Entities if they don't exist yet. Uses the EAV Import API.
		/// </summary>
		public void EnsurePipelineDesignerAttributeSets()
		{
			#region Define AttributeSets for DataPipeline and DataPipelinePart
			var pipelinesAttributeSet = Import.AttributeSet.SystemAttributeSet(DataSource.DataPipelineStaticName, "Describes a set of data sources and how they are interconnected.",
				new List<Import.Attribute>
				{
					new Import.Attribute("Name", "Pipeline name", AttributeTypeEnum.String, "Descriptive Name", true),
					new Import.Attribute("Description", "Description", AttributeTypeEnum.String, "Short info about this pipeline, what it's for", true),
					new Import.Attribute("AllowEdit", "Allow Edit", AttributeTypeEnum.Boolean, "If set to false, the pipeline-system will only show this pipeline but not allow changes.", true),
					new Import.Attribute("StreamsOut", "Streams Out", AttributeTypeEnum.String, "Comma separated list of streams this pipeline offers to the target. Like 'Content, Presentation, ListContent, ListPresentation'", true),
					new Import.Attribute("StreamWiring", "Stream Wiring", AttributeTypeEnum.String, "List of connections between the parts of this pipeline, each connection on one line, like 6730:Default>6732:Default", true),
					new Import.Attribute("TestParameters", "Test-Parameters", AttributeTypeEnum.String, "Static Parameters to test the Pipeline with. Format as [Token:Property]=Value", true)
			});

			var pipelinePartsAttributeSet = Import.AttributeSet.SystemAttributeSet(DataSource.DataPipelinePartStaticName, "A part in the data pipeline, usually a data source/target element.",
				new List<Import.Attribute>
				{
					new Import.Attribute("Name", "Name", AttributeTypeEnum.String, "The part name for easy identification by the user", true),
					new Import.Attribute("Description", "Description", AttributeTypeEnum.String, "Notes about this item", true),
					new Import.Attribute("PartAssemblyAndType", "Part Assembly and Type", AttributeTypeEnum.String, "Assembly and type info to help the system find this dll etc.", true),
					new Import.Attribute("VisualDesignerData", "Visual Designer Data", AttributeTypeEnum.String, "Technical data for the designer so it can save it's values etc.", true),
				});
			#endregion

			#region Define AttributeSets for DataSources Configurations

			var dsrcApp = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.App", "used to configure an App DataSource", new List<Import.Attribute>());

			var dsrcAttributeFilter = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.AttributeFilter", "used to configure an AttributeFilter DataSource",
				new List<Import.Attribute>
				{
					new Import.Attribute("AttributeNames", "AttributeNames", AttributeTypeEnum.String, null, true),
				});

			var dsrcEntityIdFilter = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityIdFilter", "used to configure an EntityIdFilter DataSource",
				new List<Import.Attribute>
				{
					new Import.Attribute("EntityIds", "EntityIds", AttributeTypeEnum.String, "Comma separated list of Entity IDs, like 503,522,5066,32", true),
				});

			var dsrcEntityTypeFilter = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityTypeFilter", "used to configure an EntityTypeFilter DataSource",
				new List<Import.Attribute>
				{
					new Import.Attribute("TypeName", "TypeName", AttributeTypeEnum.String, null, true),
				});

			var dsrcValueFilter = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueFilter", "used to configure an ValueFilter DataSource",
				new List<Import.Attribute>
				{
					new Import.Attribute("Attribute", "Attribute", AttributeTypeEnum.String, null, true),
					new Import.Attribute("Value", "Value", AttributeTypeEnum.String, null, true)
				});

			var dsrcValueSort = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueSort", "used to configure an ValueSort DataSource",
				new List<Import.Attribute>
				{
					new Import.Attribute("Attributes", "Attributes", AttributeTypeEnum.String, null, true),
					new Import.Attribute("Directions", "Directions", AttributeTypeEnum.String, null, true),
				});

			#endregion

			// Collect AttributeSets for use in Import
			var attributeSets = new List<Import.AttributeSet>
			{
				pipelinesAttributeSet,
				pipelinePartsAttributeSet,
				dsrcApp,
				dsrcAttributeFilter,
				dsrcEntityIdFilter,
				dsrcEntityTypeFilter,
				dsrcValueFilter,
				dsrcValueSort
			};
			var import = new Import.Import(DataSource.DefaultZoneId, DataSource.MetaDataAppId, _userName);
			import.RunImport(attributeSets, null);

			#region Mark all AttributeSets as shared and ensure they exist on all Apps
			foreach (var attributeSet in attributeSets)
				_metaDataCtx.GetAttributeSet(attributeSet.StaticName).AlwaysShareConfiguration = true;

			_metaDataCtx.SaveChanges();

			_metaDataCtx.EnsureSharedAttributeSets();
			#endregion
		}
	}
}
