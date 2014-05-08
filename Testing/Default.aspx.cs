using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using ToSic.Eav.DataSources;
using IDataSource = ToSic.Eav.DataSources.IDataSource;


namespace ToSic.Eav
{
	public partial class Default : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Configuration.SetConnectionString("SiteSqlServer");

			//AddEntity();

			int appId;
			int.TryParse(Request.QueryString["AppId"], out appId);
			if (appId == 0)
				appId = 1;


			switch ((Request["Test"] ?? "").ToLower())
			{
				case "all":
					ShowDataSource(InitialDataSource(), "Initial DataSource", false);
					break;
				case "typefilter":
					//EntityTypeFilter("Type 11:26");
					ShowDataSource(EntityTypeFilter("Person ML"), "EntityTypeFilter", true);
					break;
				case "valuefilter":
					ShowDataSource(TestValueFilter("Person ML", "FirstName", "Daniel EN"), "ValueFilter", true);
					break;
				case "attributefilter":
					var typeFiltered = EntityTypeFilter("Person ML");
					ShowDataSource(AttributeFilter(typeFiltered), "AttributeFilter (remover)", true);
					break;
				case "valuesort":
					typeFiltered = EntityTypeFilter("Person ML");
					// ShowDataSource(ValueSort(typeFiltered, "FirstName,entitytitle", "a,a"), "Sorted Value", true);
					ShowDataSource(ValueSort(typeFiltered, "entityid,entitytitle", "d,a"), "Sorted Value", true);
					// ShowDataSource(ValueSort(typeFiltered, "entitytitle,entityid", "a,a"), "Sorted Value", true);
					//ShowDataSource(ValueSort(typeFiltered, "id,entitytitle", "a,a"), "Sorted Value", true);
					break;
				case "publishingwithdrafts":
					ShowDataSource(PublishingFilter("News", true), "News with Drafts", true);
					break;
				case "publishingpublishedonly":
					ShowDataSource(PublishingFilter("News", false), "News - published only", true);
					break;
				case "listproperty":
					typeFiltered = EntityTypeFilter("Person ML");
					litResults.Text = "Found " + typeFiltered.List.Count + " items in the main list";
					break;
				case "entityidfilter":
					var entityIds = Request.QueryString["EntityIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();
					ShowDataSource(EntityIdFilter(entityIds, appId), "EntityId Filter", true);
					break;
				case "clearcache":
					ClearCache(appId);
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

		public void ClearCache(int appId)
		{
			var ctx = EavContext.Instance(appId: appId);

			DataSource.GetCache(ctx.ZoneId, ctx.AppId).PurgeCache(ctx.ZoneId, ctx.AppId);

			if (Request.UrlReferrer != null)
				Response.Redirect(Request.UrlReferrer.AbsoluteUri);
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

		private EntityIdFilter EntityIdFilter(IEnumerable<int> entityIds, int appId = 1)
		{
			var source = DataSource.GetInitialDataSource(appId: appId);

			var filterPipeline = DataSource.GetDataSource<EntityIdFilter>(appId: appId, upstream: source);
			filterPipeline.EntityIds = string.Join(",", entityIds);

			return filterPipeline;

		}

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

		private IDataSource InitialDataSource(bool showDrafts = false)
		{
			var source = DataSource.GetInitialDataSource(showDrafts: showDrafts);
			return source;
		}

		private static IDataSource PublishingFilter(string typeName, bool showDrafts)
		{
			var source = DataSource.GetInitialDataSource(appId: 2, showDrafts: showDrafts);

			var filterPipeline = DataSource.GetDataSource<EntityTypeFilter>(2, 2, source);
			filterPipeline.TypeName = typeName;

			return filterPipeline;
		}



		private static EntityTypeFilter EntityTypeFilter(string typeName, int appId = 1)
		{
			var source = DataSource.GetInitialDataSource(appId: appId);

			var filterPipeline = DataSource.GetDataSource<EntityTypeFilter>(appId: appId, upstream: source);
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

		#region ShowStuff
		public void ShowDataSource(IDataSource source, string title, bool fullEntities = false)
		{
			var output = "<h2>" + title + " (Name: " + source.Name + ")</h2>";
			Trace.Write("Filtering" + title, "Start");
			//Response.Write("Ready: " + source.Ready);//; Chain: " + source.NameChain + "; Length: " + source.DistanceFromSource);
			//Response.Write("<br>count:" + source.Out["Default"].List.Count);

			foreach (var dataStream in source.Out)
			{
				output += "<h3>Stream: " + dataStream.Key + " - List.Count: " + dataStream.Value.List.Count + "</h3>";

				if (fullEntities)
				{
					output += "<h4>Entities Details</h4><hr/>";
					foreach (var entity in dataStream.Value.List.Select(e => e.Value))
						output += ShowEntity(entity);
				}

			}

			Trace.Write("Filtering" + title, "Done");
			litResults.Text = output;
		}

		public string ShowEntity(IEntity entity)
		{
			var output = new StringBuilder("<ul><li><b>EntityId</b>: " + entity.EntityId + "</li>");
			output.Append("<li><b>RepositoryId</b>: " + entity.RepositoryId + "</li>");
			output.Append("<li><b>IsPublished</b>: " + entity.IsPublished + "</li>");
			output.Append("<li><b>Values:</b><ul>");
			foreach (var attribute in entity.Attributes)
			{
				output.AppendFormat("<li><b>{0}</b>: {1}</li>", attribute.Key, attribute.Value[0]);

				var relationship = attribute.Value as AttributeModel<EntityRelationshipModel>;
				if (relationship != null && relationship.TypedContents != null)
				{
					output.Append("<ul>");
					output.Append("<li>Entities count: " + relationship.TypedContents.Count() + "</li>");
					if (relationship.TypedContents.Any())
						output.Append("<li>Relationship Titles: " + string.Join(", ", relationship.TypedContents.Where(e => e.Attributes.Any()).Select(e => e.Title == null ? "(no Title)" : e.Title[0])) + "</li>");
					output.Append("</ul>");
				}
			}
			output.Append("</li></ul>");
			output.Append("<li><b>Children[\"People\"]</b>: " + entity.Relationships.Children["People"].Count() + "</li>");
			output.Append("<li><b>AllChildren</b>: " + entity.Relationships.AllChildren.Count() + "</li>");
			output.Append("<li><b>AllParents</b>: " + entity.Relationships.AllParents.Count() + "</li>");
			output.Append("</ul>");

			output.Append("<hr/>");
			return output.ToString();
		}
		#endregion
	}
}