using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using ToSic.Eav;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.PropertyAccess;
using Entity = ToSic.Eav.Data.Entity;
using IDataSource = ToSic.Eav.DataSources.IDataSource;

public partial class Default : Page
{
	private const string ConnectionStringName = "SiteSqlServer";
	private readonly DateTime _startTime = DateTime.Now;

	protected void Page_Load(object sender, EventArgs e)
	{
		Configuration.SetConnectionString(ConnectionStringName);

		//AddEntity();

		int appId;
		int.TryParse(Request.QueryString["AppId"], out appId);
		if (appId == 0)
			appId = 1;


		switch ((Request["Test"] ?? "").ToLower())
		{
			case "all":
				ShowDataSource(InitialDataSource(), "Initial DataSource");
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
				ClearCache();
				break;
			case "inmemoryentity":
				litResults.Text = ShowEntity(GetIEntity());
				break;
			case "datatabledatasource1":
				ShowDataSource(GetDataTableDataSource1(), "DataTable DataSource 1", true);
				break;
			case "datatabledatasource2":
				ShowDataSource(GetDataTableDataSource2(), "DataTable DataSource 2", true);
				break;
			case "datatabledatasource3":
				ShowDataSource(GetDataTableDataSource3(), "DataTable DataSource 3", true);
				break;
			case "sqldatasourcesimple":
				ShowDataSource(GetSqlDataSourceSimple(), "SQL DataSource (simple)", true);
				break;
			case "sqldatasourcesimple2":
				ShowDataSource(GetSqlDataSourceSimple2(), "SQL DataSource (simple 2)", true);
				break;
			case "sqldatasourcecomplex":
				ShowDataSource(GetSqlDataSourceComplex(), "SQL DataSource (complex)", true);
				break;
			case "sqldatasourcewithconfiguration":
				ShowDataSource(GetSqlDataSourceWithConfiguration(), "SQL DataSource with configuration", true);
				break;
			case "relationshipfilter":
				ShowRelationshipFilterOptions();
				//if(Request.QueryString["tag"] != null || Request.QueryString["tagcat"] != null)
				ShowDataSource(ApplyRelationshipFilter(), "Relationship filter", true);
				break;
			case "datapipelinefactory":
				IEnumerable<IPropertyAccess> config = null;
				var source = DataPipelineFactory.GetDataSource(1, 347, config, new PassThrough());
				ShowDataSource(source, "DataPipelineFactory");
				break;
			case "appds":
				litResults.Text += TestAppDataSource(false);
				litResults.Text += TestAppDataSource(true);
				break;
			case "other":
				RunOtherTests();
				break;
			case "token":
				RunTokenTest();
				break;
		}

		//Tests2dm();

		//Chain5();
		//InitialDataSource();
		//var typeFiltered = EntityTypeFilter();
		//AttributeFilter(typeFiltered);
		//Chain6();
		//EntityIdFilter();

		//var source = DataSource.GetInitialDataSource(1, 1);
		//var entities = source.Out["Default"].List;
		//ShowEntity(entities[3378]);
	}

	private void RunOtherTests()
	{
		switch (Request.QueryString["T2"].ToLower())
		{
			case "RequestProvider":
				//var rp = new QueryStringValueProvider();
				break;
		}
	}

	private void RunTokenTest()
	{
		var configurationProvider = new ConfigurationProvider();
		var queryStringSource = new QueryStringPropertyAccess();
		var configList = new Dictionary<string, string> { { "Name", "[QueryString:Name|Format|Alternate]" } };

		ShowConfigurationList("Before load", configList);

		configurationProvider.Sources.Add(queryStringSource.Name, queryStringSource);

		configurationProvider.LoadConfiguration(configList);

		ShowConfigurationList("After load", configList);
	}

	protected void Page_PreRender(object sender, EventArgs e)
	{
		var loadTime = (DateTime.Now - _startTime).TotalMilliseconds;
		lblTimeToRender.Text = string.Format(lblTimeToRender.Text, loadTime);
	}

	private static IDataSource GetSqlDataSourceSimple()
	{
		var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString;
		var selectCommand = "select object_id as " + SqlDataSource.EntityIdDefaultColumnName + ", * FROM sys.tables";
		var source = new SqlDataSource(connectionString, selectCommand, "SqlTableInformation", titleField: "name");
		return source;
	}

	private static IDataSource GetSqlDataSourceSimple2()
	{
		var source = DataSource.GetDataSource<SqlDataSource>();
		source.SelectCommand = "Select TOP 10 object_id as EntityId, name as EntityTitle, * FROM sys.tables WHERE name like '%' + @search_name + '%'";
		source.ConnectionStringName = "SiteSqlServer";
		source.Configuration.Add("@search_name", "value");

		return source;
	}

	private static IDataSource GetSqlDataSourceComplex()
	{
		var source = DataSource.GetDataSource<SqlDataSource>();
		source.TitleField = "name"; // Which field is used for the Title, optional, default is "Title"
		source.EntityIdField = "object_id"; // Which field is used for the EntityId, optional, default is "EntityId"
		source.ContentType = "File"; // what this data-type is called internally, optional, default is "SqlData"
		source.SelectCommand = "select * FROM sys.tables WHERE name LIKE '%' + @search_name + '%'";
		source.ConnectionString = null; // you could apply a different one here
		source.ConnectionStringName = ConnectionStringName;
		source.Configuration.Add("@search_name", "value");

		return source;
	}

	private static IDataSource GetSqlDataSourceWithConfiguration()
	{
		var source = DataSource.GetDataSource<SqlDataSource>();
		source.Configuration["TitleField"] = "name";
		source.Configuration["ContentType"] = "SqlTableInformation";
		source.Configuration["SelectCommand"] = "select object_id as " + SqlDataSource.EntityIdDefaultColumnName + ", * FROM sys.tables WHERE name like '%' + @search_name + '%'";
		source.Configuration["ConnectionString"] = null;
		source.Configuration["ConnectionStringName"] = ConnectionStringName;
		source.Configuration.Add("@search_name", "value");

		return source;
	}

	private static IDataSource GetDataTableDataSource1()
	{
		var dataTable = new DataTable();
		dataTable.Columns.AddRange(new[]
		{
			new DataColumn(DataTableDataSource.EntityIdDefaultColumnName, typeof(int)),
			new DataColumn("FullName"),
			new DataColumn("FirstName"),
			new DataColumn("LastName"),
			new DataColumn("City"),
			new DataColumn("Male", typeof(bool)), 
			new DataColumn("Birthdate", typeof(DateTime))
		});
		AddDummyPeople(dataTable);

		var source = new DataTableDataSource(dataTable, "SampleContentType", titleField: "FullName");

		return source;
	}

	private static void AddDummyPeople(DataTable dataTable)
	{
		for (var i = 1; i <= 10; i++)
		{
			var firstName = "First Name " + i;
			var lastName = "Last Name " + i;
			var fullName = firstName + " " + lastName;
			dataTable.Rows.Add(i + 10000, fullName, firstName, lastName, "City " + i, i % 3 == 0, DateTime.Now.AddYears(-27));
		}
	}

	private static IDataSource GetDataTableDataSource2()
	{
		var dataTable = new DataTable();
		dataTable.Columns.AddRange(new[]
		{
			new DataColumn("EntityId", typeof(int)),
			new DataColumn("EntityTitle"),
			new DataColumn("FirstName"),
			new DataColumn("LastName"),
			new DataColumn("City"),
			new DataColumn("Male", typeof(bool)), 
			new DataColumn("Birthdate", typeof(DateTime))
		});
		AddDummyPeople(dataTable);

		var source = new DataTableDataSource(dataTable, "SampleContentType");

		return source;
	}

	private static IDataSource GetDataTableDataSource3()
	{
		var dataTable = new DataTable();
		dataTable.Columns.AddRange(new[]
		{
			new DataColumn("PersonNumber", typeof(int)),
			new DataColumn("FullName"),
			new DataColumn("FirstName"),
			new DataColumn("LastName"),
			new DataColumn("City"),
			new DataColumn("Male", typeof(bool)), 
			new DataColumn("Birthdate", typeof(DateTime))
		});
		AddDummyPeople(dataTable);

		var source = new DataTableDataSource(dataTable, "SampleContentType", "PersonNumber", "FullName");

		return source;
	}

	private static IEntity GetIEntity()
	{
		var values = new Dictionary<string, object>
		{
			{"Title", "Test"},
			{"FirstName", "Test"},
			{"Demo1", true},
			{"Demo2", null},
			{"Demo3", 123},
			{"Demo4", 123.12},
			{"Date", DateTime.Now}
		};
		var entityModel = new Entity(100001, "SampleContentType", values, "Title");

		return entityModel;
	}

	public void ClearCache()
	{
		var ctx = EavContext.Instance();
		var apps = ctx.GetApps();
		foreach (var app in apps)
			DataSource.GetCache(app.ZoneID, app.AppID).PurgeCache(app.ZoneID, app.AppID);

		if (Request.UrlReferrer != null)
			Response.Redirect(Request.UrlReferrer.AbsoluteUri);
	}

	// create a entity for test purposes in the DB
	public void AddEntity()
	{
		var context = EavContext.Instance(1, 1);
		const string userName = "Testing 2dm";
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

	private static IDataSource InitialDataSource(bool showDrafts = false)
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

	private static ValueFilter TestValueFilter(string typeName, string attrName, string valueFilter)
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

	private static AttributeFilter AttributeFilter(IDataSource source)
	{
		var filterPipeline = DataSource.GetDataSource<AttributeFilter>(1, 1, source);
		filterPipeline.AttributeNames = "LastName,FirstName";
		return filterPipeline;
		//ShowDataSource(filterPipeline, "AttributeFilter", true);
	}

	private static ValueSort ValueSort(IDataSource source, string attributes, string directions)
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

	private void ShowRelationshipFilterOptions()
	{
		var tags = new[] { "C#", "Razor", "Responsive", "Video", "Gallery" };
		var tagIds = new[] { 6484, 6483, 6485, 6488, 6487 };
		var tagCats = new[] { "Design", "Technology", "Use Case" };
		var parentNames = new[] { "2sic", "2sic 2", "mettlers home" };
		//var parentIds = new[]{}
		string output = "<ul>";

		for (var i = 0; i <= tags.Length - 1; i++)
			output += "<li>"
					  + "<a href='?Test=RelationshipFilter&tag=" + tags[i] + "'>test with " + tags[i] + "</a>"
					  + " (<a href='?Test=RelationshipFilter&tagid=" + tagIds[i] + "'>id " + tagIds[i] + "</a>)"
					  + "</li>";

		for (var i = 0; i <= tagCats.Length - 1; i++)
			output += "<li><a href='?Test=RelationshipFilter&tagCat=" + tagCats[i] + "'>tag category with " + tagCats[i] + "</a></li>";
		for (var i = 0; i <= tagCats.Length - 1; i++)
			output += "<li><a href='?Test=RelationshipFilter&parent=" + parentNames[i] + "'>parents with " + parentNames[i] + "</a></li>";
		output += "</ul>";

		litResults.Text = output;
	}

	private RelationshipFilter ApplyRelationshipFilter()
	{
		var filterPipeline = EntityTypeFilter("Store");
		var relFilter = DataSource.GetDataSource<RelationshipFilter>(1, 1, filterPipeline);
		relFilter.Relationship = "Tags";
		if (Request.QueryString["tag"] != null)
		{
			relFilter.CompareAttribute = "EntityTitle";
			relFilter.Filter = Request.QueryString["Tag"];
		}
		else if (Request.QueryString["tagcategory"] != null)
		{
			relFilter.CompareAttribute = "TagCategory";
			relFilter.Filter = Request.QueryString["TagCat"];
		}
		else
		{
			relFilter.CompareAttribute = "EntityId";
			relFilter.Filter = Request.QueryString["tagid"];
		}
		return relFilter;
	}

	public string TestAppDataSource(bool swapAppAndZone)
	{
		var source = DataSource.GetInitialDataSource();
		var appDs = DataSource.GetDataSource<ToSic.Eav.DataSources.App>(1, 1, source);
		if (swapAppAndZone)
		{
			appDs.AppSwitch = 2;
			appDs.ZoneSwitch = 2;
		}

		var result = "";
		if (!swapAppAndZone)
			result += appDs["Test"].Name;
		result += "<h2>Lists in out (should contain all content-types of App " + appDs.AppId + " Zone " +
				  appDs.ZoneId + ")</h2><ol>";
		foreach (var outStream in appDs.Out)
			result += "<li>\"" + outStream.Key + "\" (" + outStream.Value.List.Count + ")" + "</li>";

		result += "</ol>";
		return result;
	}

	// this was a test for a parent-filter, but I don't think I'll ever need this feature
	//private RelationshipFilter ApplyRelationshipParentFilter()
	//{
	//	var filterPipeline = EntityTypeFilter("Tag");
	//	//filterPipeline.TypeName = "Store";
	//	var relFilter = DataSource.GetDataSource<RelationshipFilter>(1,1, filterPipeline);
	//	// relFilter.Relationship = "Store";
	//	// relFilter.CompareAttribute = "TagCategory";
	//	relFilter.ChildOrParent = "parent";
	//	relFilter.ParentType = "Store";
	//	relFilter.Filter = Request.QueryString["parent"];


	//	return relFilter;
	//}

	#region Show Stuff

	public void ShowDataSource(IDataSource source, string title, bool fullEntities = false)
	{
		var output = "<h2>" + title + " (Name: " + source.Name + ")</h2>";
		Trace.Write("Filtering" + title, "Start");
		//Response.Write("Ready: " + source.Ready);//; Chain: " + source.NameChain + "; Length: " + source.DistanceFromSource);
		//Response.Write("<br>count:" + source.Out["Default"].List.Count);

		foreach (var dataStream in source.Out)
		{
			output += "<h3>Stream: " + dataStream.Key + " - List.Count: " + dataStream.Value.List.Count + "</h3>";

			if (!fullEntities)
				continue;

			output += "<h4>Entities Details</h4><hr/>";
			foreach (var entity in dataStream.Value.List.Select(e => e.Value))
				output += ShowEntity(entity);
		}

		Trace.Write("Filtering" + title, "Done");
		litResults.Text += output;
	}

	private static string ShowEntity(IEntity entity)
	{
		var output = new StringBuilder("<ul><li><b>EntityId</b>: " + entity.EntityId + "</li>");
		output.Append("<li><b>RepositoryId</b>: " + entity.RepositoryId + "</li>");
		output.Append("<li><b>IsPublished</b>: " + entity.IsPublished + "</li>");
		output.Append("<li><b>ContentType</b>: " + entity.Type.Name + "</li>");
		output.Append("<li><b>Title</b>: " + entity.Title[0] + "</li>");
		output.Append("<li><b>Modified</b>: " + entity.Modified + "</li>");
		output.Append("<li><b>Values:</b><ul>");
		foreach (var attribute in entity.Attributes)
		{
			var value = attribute.Value[0];
			output.AppendFormat("<li><b>{0}</b> (Type: {1}): {2}</li>", attribute.Key, value != null ? value.GetType().ToString() : "(null)", value);

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

	private void ShowConfigurationList(string title, Dictionary<string, string> configList)
	{
		var output = new StringBuilder("<h1>ConfigurationList - ");
		output.Append(title);
		output.Append("</h1><ul>");

		foreach (var config in configList)
			output.AppendFormat("<li>{0}: {1}</li>\n", config.Key, config.Value);

		output.Append("</ul>");

		litResults.Text += output.ToString();
	}

	#endregion
}