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

		/// <summary>
		/// Constructor
		/// </summary>
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
					Import.Attribute.StringAttribute("Name", "Pipeline name", "Descriptive Name", true),
					Import.Attribute.StringAttribute("Description", "Description", "Short info about this pipeline, what it's for", true),
					Import.Attribute.BooleanAttribute("AllowEdit", "Allow Edit", "If set to false, the pipeline-system will only show this pipeline but not allow changes.", true, true),
					Import.Attribute.StringAttribute("StreamsOut", "Streams Out", "Comma separated list of streams this pipeline offers to the target. Like 'Content, Presentation, ListContent, ListPresentation'", false),
					Import.Attribute.StringAttribute("StreamWiring", "Stream Wiring", "List of connections between the parts of this pipeline, each connection on one line, like 6730:Default>6732:Default", false, rowCount: 10),
					Import.Attribute.StringAttribute("TestParameters", "Test-Parameters", "Static Parameters to test the Pipeline with. Format as [Token:Property]=Value", true, rowCount: 10)
			});

			var pipelinePartsAttributeSet = Import.AttributeSet.SystemAttributeSet(DataSource.DataPipelinePartStaticName, "A part in the data pipeline, usually a data source/target element.",
				new List<Import.Attribute>
				{
					Import.Attribute.StringAttribute("Name", "Name", "The part name for easy identification by the user", true),
					Import.Attribute.StringAttribute("Description", "Description", "Notes about this item", true),
					Import.Attribute.StringAttribute("PartAssemblyAndType", "Part Assembly and Type", "Assembly and type info to help the system find this dll etc.", true),
					Import.Attribute.StringAttribute("VisualDesignerData", "Visual Designer Data", "Technical data for the designer so it can save it's values etc.", true),
				});
			#endregion

			#region Define AttributeSets for DataSources Configurations

			var dsrcApp = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.App", "used to configure an App DataSource", new List<Import.Attribute>());

			var dsrcAttributeFilter = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.AttributeFilter", "used to configure an AttributeFilter DataSource",
				new List<Import.Attribute>
				{
					Import.Attribute.StringAttribute("AttributeNames", "AttributeNames", null, true),
				});

			var dsrcEntityIdFilter = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityIdFilter", "used to configure an EntityIdFilter DataSource",
				new List<Import.Attribute>
				{
					Import.Attribute.StringAttribute("EntityIds", "EntityIds", "Comma separated list of Entity IDs, like 503,522,5066,32", true),
				});

			var dsrcEntityTypeFilter = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityTypeFilter", "used to configure an EntityTypeFilter DataSource",
				new List<Import.Attribute>
				{
					Import.Attribute.StringAttribute("TypeName", "TypeName", null, true),
				});

			var dsrcValueFilter = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueFilter", "used to configure a ValueFilter DataSource",
				new List<Import.Attribute>
				{
					Import.Attribute.StringAttribute("Attribute", "Attribute", null, true),
					Import.Attribute.StringAttribute("Value", "Value", null, true)
				});

			var dsrcValueSort = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueSort", "used to configure a ValueSort DataSource",
				new List<Import.Attribute>
				{
					Import.Attribute.StringAttribute("Attributes", "Attributes", null, true),
					Import.Attribute.StringAttribute("Directions", "Directions", null, true),
				});

			var dsrcRelationshipFilter = Import.AttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.RelationshipFilter", "used to configur a RelationshipFilter DataSource",
				new List<Import.Attribute>
				{
					Import.Attribute.StringAttribute("Relationship", "Relationship", null, true),
					Import.Attribute.StringAttribute("Filter", "Filter", null, true),
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
				dsrcValueSort,
				dsrcRelationshipFilter
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
