using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using ToSic.Eav.DataSources;


namespace ToSic.Eav
{
	public partial class Default : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Configuration.SetConnectionString("SiteSqlServer");

			//AddEntity();

			switch ((Request["Test"] ?? "").ToLower())
			{
				case "typefilter":
					//EntityTypeFilter("Type 11:26");
					ShowDataSource(EntityTypeFilter("Person ML"), "EntityTypeFilter", true);
					break;
				case "valuefilter":
					ShowDataSource(TestValueFilter("Person ML", "FirstName", "Daniel"), "ValueFilter", true);
					break;
				case "attributefilter":
					var typeFiltered = EntityTypeFilter("Person ML");
					ShowDataSource(AttributeFilter(typeFiltered), "AttributeFilter (remover)", true);
					break;
				case "valuesort":
					typeFiltered = EntityTypeFilter("Person ML");
					ShowDataSource(ValueSort(typeFiltered, "FirstName,LastName", "asc"), "Sorted Value", true);
					break;
				Default:
					break;
			}

			//Tests2dm();

			//Chain5();
			//InitialDataSource();
			//var typeFiltered = EntityTypeFilter();
			//AttributeFilter(typeFiltered);
			//Chain6();
			//EntityIdFilter();
			//DataPipelineFactory();



			//var source = DataSource.GetInitialDataSource(1, 1);
			//var entities = source.Out["Default"].List;
			//ShowEntity(entities[3378]);
		}

		// create a entity for test purposes in the DB
		public void AddEntity()
		{
			var context = EavContext.Instance(1, 1);
			var userName = "Testing 2dm";
			context.UserName = userName;
			var newValues = new Dictionary<string, ValueViewModel>
				{
					{"FirstName", new ValueViewModel {Value = "Andreas"}},
					{"LastName", new ValueViewModel {Value = "Müller"}},
					{"Address", new ValueViewModel {Value = "Räfiserhalde 34"}},
					{"ZIP", new ValueViewModel {Value = "9470"}},
					{"City", new ValueViewModel {Value = "Räfis"}}
				};

			context.AddEntity(37, newValues, null, null);
		}


		private void DataPipelineFactory()
		{
			Trace.Write("DataPipelineFactory", "Before Init");
			var source = DataSources.DataPipelineFactory.GetDataSource(1, 1, 347, null);
			Trace.Write("DataPipelineFactory", "End Init");

			ShowDataSource(source, "DataPipelineFactory", true);
		}

		//private EntityIdFilter EntityIdFilter()
		//{
		//	var source = DataSource.GetInitialDataSource("SiteSqlServer", 2, 2);
		//	//var filterPipeline = (EntityIdFilter)DataSource.GetDataSource("ToSic.Eav.DataSources.EntityIdFilter", source);

		//	var dataSourceId = 1;
		//	var pipelineId = 1;

		//	var configList = new Dictionary<string, object> { { "EntityIds", "[Settings:EntityIds]" } };
		//	var filterPipeline = new EntityIdFilter(dataSourceId, pipelineId, source);

		//	//filterPipeline.Configuration["EntityIds"] = "329, 330";
		//	//filterPipeline.Configuration["EntityIds"] = Request.QueryString["entities"];

		//	var settingsPropertyProvider = new SimplePropertyProvider();
		//	settingsPropertyProvider.Values.Add("EntityIds", new[] { 329, 330 });
		//	filterPipeline.ConfigurationProvider.Sources.Add("Settings", settingsPropertyProvider);

		//	ShowDataSource(filterPipeline, "EntityTypeFilter", true);

		//	return filterPipeline;
		//}

		//private DataSource DemoFactory()
		//{
		//	var DataSources = new[] { "230203, EntityTypeFilter", "230244, ICache", "306010, EntityTypeFilter" };
		//	var Connections = new[]
		//		{
		//			"230244:Default>230203:Default",
		//			"230244:Default>306010:Default"
		//		};
		//	var x = new ConfigurationProvider();


		//}

		//private void Chain6()
		//{
		//	string[] chain = { "ToSic.Eav.DataSources.Testing.TestStoreWithManyEntities", "ToSic.Eav.DataSources.Caches.ICache" };
		//	var source = DataSource.AssembleDataSource(chain);
		//	((TestStoreWithManyEntities)source).TotalItems = 100000;
		//	ShowDataSource(source, "Chain6");
		//}

		//private void Tests2dm()
		//{
		//	Response.Write("<br>" + DateTime.Now.ToString() + "<br>");
		//	var emptySource = new Empty();
		//	Response.Write(emptySource.Ready + "<br>");

		//	// Assemble chain
		//	string[] chain1 =
		//		{
		//			"ToSic.Eav.DataSources.Empty", "ToSic.Eav.DataSources.PassThrough", "ToSic.Eav.DataSources.PassThrough"
		//		};

		//	var source = DataSource.AssembleDataSource(chain1);
		//	//Response.Write("<br><br>length:" + source.DistanceFromSource);
		//	Response.Write("<br>count:" + source.Out["Default"].List.Count + " ; ready: " + source.Ready);

		//	// Assemble chain with type filtering
		//	string[] chain2 =
		//		{
		//			"ToSic.Eav.DataSources.Testing.TestStoreWithManyEntities","ToSic.Eav.DataSources.PassThrough","ToSic.Eav.DataSources.EntityTypeFilter"
		//		};
		//	source = DataSource.AssembleDataSource(chain2);
		//	var filterSource = (EntityTypeFilter)source;
		//	//filterSource.TypeName = "Demo";
		//	Response.Write("<h1>type filter</h1>");
		//	//Response.Write("length:" + source.DistanceFromSource);
		//	Response.Write("<br>count:" + source.Out["Default"].List.Count + " ; ready: " + source.Ready);


		//	// Assemble chain with dependency
		//	string[] chain3 =
		//		{
		//			"ToSic.Eav.DataSources.Testing.TestStoreWithManyEntities",
		//			"ToSic.Eav.DataSources.PassThrough",
		//			"ToSic.Eav.DataSources.PassThrough",
		//			"ToSic.Eav.DataSources.FilterAndSort"
		//		};
		//	source = DataSource.AssembleDataSource(chain3);
		//	ShowDataSource(source, "Dependency");

		//	// Assemble chain with 10'000 entities
		//	string[] chain4 =
		//		{
		//			"ToSic.Eav.DataSources.Testing.TestStoreWithManyEntities",
		//			"ToSic.Eav.DataSources.Caches.ICache", 
		//	"ToSic.Eav.DataSources.PassThrough",
		//			"ToSic.Eav.DataSources.PassThrough"
		//		};
		//	source = DataSource.AssembleDataSource(chain4);
		//	//var generator = ((IDataPipeline)source).UpstreamSource.UpstreamSource.UpstreamSource;
		//	//generator.TotalItems = 1000000;
		//	//ShowDataSource(source, "1'000'000 items, no cache");


		//	//// Response.Write("<h2>with a multiple ID filter</h2>");
		//	//var source3 = DataSource.GetDataSource("ToSic.Eav.DataSources.FilterAndSort", source);
		//	////((IDataSourceInternals)source3).UpstreamSource = source;
		//	//var filterAndSort = (FilterAndSort)source3;
		//	//filterAndSort.EntityIdFilterUrlParameterName = "ID2";
		//	//ShowDataSource(source3, "1'000'000 items, multipleId-filter");


		//	//Response.Write("<h2>with a type filter</h2>");
		//	var source2 = DataSource.GetDataSource("ToSic.Eav.DataSources.EntityTypeFilter", source);
		//	//((IDataSourceInternals)source2).UpstreamSource = source;
		//	filterSource = (EntityTypeFilter)source2;
		//	//filterSource.TypeName = "Kitchenware";
		//	ShowDataSource(source2, "With Type Filter");
		//	//Trace.Write("Filtering type", "Start");
		//	//Response.Write("<br>count:" + source2.Entities.Count + "; Length:" + source2.DistanceFromSource + " ; ready: " + source2.Ready);
		//	//Trace.Write("Filtering type", "Done");


		//	//Response.Write("<h2>with a extensions assignments filter</h2>");
		//	var source4 = DataSource.GetDataSource("ToSic.Eav.DataSources.Testing.TestStoreWithManyExtensions");

		//	//((IDataSourceInternals)source4).UpstreamSource = source.UpstreamSource.UpstreamSource.UpstreamSource;
		//	//((IDataSourceInternals)source.UpstreamSource.UpstreamSource).UpstreamSource = source4;
		//	//ShowDataSource(source, "Extension Assignments, Unused");

		//	// doesn't work yet...
		//	// Response.Write(((IMetaDataSource)source).IndexForExternalInt[17, 20].ToString());
		//}

		private void InitialDataSource()
		{
			var source = DataSource.GetInitialDataSource();
			ShowDataSource(source, "Initial DataSource");
		}

		private EntityTypeFilter EntityTypeFilter(string typeName)
		{
			var source = DataSource.GetInitialDataSource();

			var filterPipeline = DataSource.GetDataSource<EntityTypeFilter>(1, 1, source);
			//old: filterPipeline.Configuration["TypeName"] = typeName;
			filterPipeline.TypeName = typeName;
			// ShowDataSource(filterPipeline, "EntityTypeFilter", true);

			return filterPipeline;
		}

		private ValueFilter TestValueFilter(string typeName, string attrName, string valueFilter)
		{
			var source = DataSource.GetInitialDataSource();

	      // var filterPipeline = (EntityTypeFilter)DataSource.GetDataSource("ToSic.Eav.DataSources.EntityTypeFilter", 1, 1, source);
			var filterPipeline = DataSource.GetDataSource<EntityTypeFilter>(1, 1, source);
			filterPipeline.TypeName = typeName;
			var valuePipeline = DataSource.GetDataSource<ValueFilter>(1, 1, filterPipeline);
			valuePipeline.Attribute = attrName;
			valuePipeline.Value = valueFilter;

			//var list = valuePipeline.Out["Default"].List;
			//var filtered = (from e in list
			// where e.Value.Attributes[attrName].Values.FirstOrDefault().ToString() == valueFilter
			// select e).ToDictionary(x => x.Key, y => y.Value);

			return valuePipeline;
		}

		private AttributeFilter AttributeFilter(DataSources.IDataSource source)
		{
			var filterPipeline = DataSource.GetDataSource<AttributeFilter>(1, 1, source);
			filterPipeline.AttributeNames = "LastName,FirstName";
			return filterPipeline;
			//ShowDataSource(filterPipeline, "AttributeFilter", true);
		}

		private ValueSort ValueSort(DataSources.IDataSource source, string attributes, string directions)
		{
			var filterPipeline = DataSource.GetDataSource<ValueSort>(1, 1, source);
			filterPipeline.Attributes = attributes;// "LastName,FirstName";
			filterPipeline.Directions = directions;
			return filterPipeline;
		}

		//private void Chain5()
		//{
		//	string[] chain = { "ToSic.Eav.DataSources.Caches.DNNFarmCache, ToSic.Eav.Professional", "ToSic.Eav.DataSources.SqlSources.EavSqlStore" };
		//	var source = DataSource.AssembleDataSource(chain);

		//	var eavSqlStore = source;
		//	while (!(eavSqlStore is EavSqlStore))
		//		eavSqlStore = ((IDataTarget)eavSqlStore).UpstreamSource;
		//	((EavSqlStore)eavSqlStore).Init("SiteSqlServer");

		//	ShowDataSource(source, "EavSqlStore");
		//}


		public void ShowDataSource(DataSources.IDataSource source, string title, bool fullEntities = false)
		{
			Response.Write("<h2>" + title + " (Name: " + source.Name + ")</h2>");
			Trace.Write("Filtering" + title, "Start");
			//Response.Write("Ready: " + source.Ready);//; Chain: " + source.NameChain + "; Length: " + source.DistanceFromSource);
			//Response.Write("<br>count:" + source.Out["Default"].List.Count);

			foreach (var dataStream in source.Out)
			{
				Response.Write("<h3>" + dataStream.Key + " Count:" + dataStream.Value.List.Count + "</h3>");

				if (fullEntities)
				{
					Response.Write("<h4>Entities Details</h4><hr/>");
					foreach (var entity in dataStream.Value.List.Select(e => e.Value))
						ShowEntity(entity);
				}

			}

			Trace.Write("Filtering" + title, "Done");
		}

		public void ShowEntity(IEntity entity)
		{
			Response.Write(entity.EntityId + "<br/>");
			foreach (var attribute in entity.Attributes)
			{
				Response.Write(attribute.Key + ": " + attribute.Value[0] + "<br/>");

				var relationship = attribute.Value as AttributeModel<EntityRelationshipModel>;
				if (relationship != null)
				{
					Response.Write("Entities count: " + relationship.TypedContents.Count() + "<br/>");
					// 2014-03-22 2dm: deactivated, wasn't able to run ATM
					//Response.Write("Entity Titles: " + string.Join(", ", relationship.TypedContents.Select(e => e.Title[0])) + "<br/>");
				}
			}

			Response.Write("Children[\"People\"]: " + entity.Relationships.Children["People"].Count() + "<br/>");
			Response.Write("AllChildren: " + entity.Relationships.AllChildren.Count() + "<br/>");
			Response.Write("AllParents: " + entity.Relationships.AllParents.Count() + "<br/>");

			Response.Write("<hr/>");
		}
	}
}