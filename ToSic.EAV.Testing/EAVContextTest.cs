using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.SqlSources;

namespace ToSic.Eav.Testing
{
	[TestFixture]
	class EAVContextTest
	{
		[Test]
		public void TestCache()
		{
			var Context = new EavContext();
			//Context.ItemCachingEnabled = true;

			for (int i = 0; i < 10000; i++)
			{
				//Context.GetItems(new List<int>() { 274, 275, 276 });
				//Context.GetItems(new List<int>() { 273 });
				//Context.GetItems(new List<int>() { 272 });
			}

			Context.SaveChanges();
		}

		[Test]
		public void DimensionsTest()
		{
			var Context = new EavContext();

			var Entity = Context.GetEntity(277);

			//Context.UpdateEntity();

		}

		[Test]
		public void DimensionsCacheTest()
		{
			var Context = new EavContext();

			var Entity1 = Context.GetLanguages();
			var Entity2 = Context.GetLanguages();
			var Entity3 = Context.GetLanguages();

			//Context.UpdateEntity();

		}

		[Test]
		public void TestGetEntity()
		{
			//var Context = new EavContext();
			//IEntity entity = Context.GetEntityModel(303);

			//var notesAttrib = (IAttribute<string>)entity["Notes"];
			////notesAttrib.TypedContents


			//IAttribute firstNameAttrib = entity["FirstName"];
			//var firstName = (IAttribute<string>)firstNameAttrib;

			//IAttribute ageAttrib = entity["Age"];
			////var age = (IAttribute<int?>)ageAttrib;
			////ageAttrib[0];

			//var firstNameContents = firstName.TypedContents;
		}

		[Test]
		public void TestInitialDataSource()
		{
			var dsrc = DataSource.GetInitialDataSource();
			//var allEntities = dsrc.GetEntities(null);
			//Assert.IsNotNull(allEntities);
		}

		[Test]
		public void UpdateWithVersioning()
		{
			var context = EavContext.Instance(1, 1);
			var entityId = 280;
			var userName = "Testing 2bg 17:51";
			context.UserName = userName;
			var newValues = new Dictionary<string, ValueViewModel>
				{
					{"FirstName", new ValueViewModel {Value = "Benjamin 17:51"}},
					{"LastName", new ValueViewModel {Value = "Gemperle 17:51"}},
					{"Address", new ValueViewModel {Value = "Churerstrasse 35 17:51"}},
					{"ZIP", new ValueViewModel {Value = "9470 17:51"}},
					{"City", new ValueViewModel {Value = "Buchs 17:51"}}
				};

			context.UpdateEntity(entityId, newValues);
		}

		[Test]
		public void AddEntity()
		{
			var context = EavContext.Instance(1, 1);
			var userName = "Testing 2bg 17:53";
			context.UserName = userName;
			var newValues = new Dictionary<string, ValueViewModel>
				{
					{"FirstName", new ValueViewModel {Value = "Benjamin 17:51"}},
					{"LastName", new ValueViewModel {Value = "Gemperle 17:51"}},
					{"Address", new ValueViewModel {Value = "Churerstrasse 35 17:51"}},
					{"ZIP", new ValueViewModel {Value = "9470 17:51"}},
					{"City", new ValueViewModel {Value = "Buchs 17:51"}}
				};

			context.AddEntity(37, newValues, null, null);
		}

		[Test]
		public void EntityRelationshipUpdate()
		{
			var context = EavContext.Instance(1, 1);
			var entityId = 2372;
			var newValues = new Dictionary<string, ValueViewModel>();
			context.UpdateEntity(entityId, newValues);
		}

		[Test]
		public void GetDataForCache()
		{
			var sqlStore = new EavSqlStore();
			var cache = new QuickCache { AppId = 1, ZoneId = 1 };
			sqlStore.GetDataForCache(cache);
		}

		[Test]
		public void GetDataForCache2()
		{
			var db = new EavContext();

			var appId = 1;
			var entitiesValues = from e in db.Entities
								 where !e.ChangeLogIDDeleted.HasValue && e.Set.AppID == appId
								 select new
								 {
									 e.EntityID,
									 e.EntityGUID,
									 e.AttributeSetID,
									 e.KeyGuid,
									 e.KeyNumber,
									 e.KeyString,
									 e.AssignmentObjectTypeID,
									 RelatedEntities = from r in e.EntityParentRelationships
													   group r by r.AttributeID into rg
													   select new
													   {
														   AttributeID = rg.Key,
														   AttributeName = rg.Select(a => a.Attribute.StaticName).FirstOrDefault(),
														   IsTitle = rg.Any(v1 => v1.Attribute.AttributesInSets.Any(s => s.IsTitle)),
														   Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityID)
													   },
									 Attributes = from v in e.Values
												  where !v.ChangeLogIDDeleted.HasValue
												  group v by v.AttributeID into vg
												  select new
												  {
													  AttributeID = vg.Key,
													  AttributeName = vg.Select(v1 => v1.Attribute.StaticName).FirstOrDefault(),
													  AttributeType = vg.Select(v1 => v1.Attribute.Type).FirstOrDefault(),
													  IsTitle = vg.Any(v1 => v1.Attribute.AttributesInSets.Any(s => s.IsTitle)),
													  Values = from v2 in vg
															   orderby v2.ChangeLogIDCreated
															   select new
															   {
																   v2.ValueID,
																   v2.Value,
																   Languages = from l in v2.ValuesDimensions select new { DimensionId = l.DimensionID, ReadOnly = l.ReadOnly, Key = l.Dimension.ExternalKey },
																   v2.ChangeLogIDCreated
															   }
												  }
								 };
			entitiesValues.ToList();
		}

		[Test]
		public void EnsureSharedAttributeSets()
		{
			var db = EavContext.Instance(appId: 2);
			//foreach (var app in db.Apps)
			//	db.EnsureSharedAttributeSets(app.);

			db.SaveChanges();
		}

		/// <summary>
		/// Add an App with Data and remove it again completely
		/// </summary>
		[Test]
		public void AddRemoveApp()
		{
			var db = EavContext.Instance();
			// Add new App
			var app = db.AddApp("Test Clean Remove");
			db.AppId = app.AppID;

			// Add new AttributeSet
			var attributeSet = db.AddAttributeSet("Sample Attribute Set", "Sample Attribute Set Description", null, "");
			db.AppendAttribute(attributeSet, "Attribute1", "String", true);
			var attribute2 = db.AppendAttribute(attributeSet, "Attribute2", "String");
			var attribute3 = db.AppendAttribute(attributeSet, "Attribute3", "String");
			var attribute4 = db.AppendAttribute(attributeSet, "Attribute4", "Entity");

			// Add new Entities
			var values = new Dictionary<string, ValueViewModel>
			{
				{"Attribute1", new ValueViewModel{ Value = "Sample Value 1"}},
				{"Attribute2", new ValueViewModel{ Value = "Sample Value 2"}},
				{"Attribute3", new ValueViewModel{ Value = "Sample Value 3"}},
			};
			var entity1 = db.AddEntity(attributeSet, values, null, null, dimensionIds: new[] { 2 });
			values.Add("Attribute4", new ValueViewModel { Value = new[] { entity1.EntityID } });
			var entity2 = db.AddEntity(attributeSet, values, null, null, dimensionIds: new[] { 2 });

			// Update existing Entity
			values["Attribute3"].Value = "Sample Value 3 modified";
			db.UpdateEntity(entity1.EntityID, values, dimensionIds: new[] { 2 });

			// update existing AttributeSets
			db.UpdateAttribute(attribute2.AttributeID, "Attribute2Renamed");
			db.RemoveAttributeInSet(attribute3.AttributeID, attributeSet.AttributeSetID);

			// Delete the App
			db.DeleteApp(app.AppID);
		}

		//[Test]
		//public void CloneEntity()
		//{
		//	var db = EavContext.Instance(appId: 2);
		//	var sourceEntity = db.GetEntity(330);
		//	var clone = db.CloneEntity(sourceEntity);
		//	clone.IsPublished = false;
		//	db.AddToEntities(clone);

		//	db.SaveChanges();
		//}
	}
}