using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using ToSic.Eav.Import;

namespace ToSic.Eav.Testing
{
	[TestFixture]
	class ImportTest
	{
		[Test]
		public void TestImport()
		{
			#region Prepare Date for Import
			var now = DateTime.Now.ToLongTimeString().Replace(":", "");
			//now = "130832";
			const int assignmentObjectTypeId = 3;
			var attributeSet1 = GetAttributeSet(1, now);
			var attributeSet2 = GetAttributeSet(2, now);
			var attributeSet3 = GetAttributeSet(3, now);
			var attributeSets = new List<Import.AttributeSet>
			{
				attributeSet1,
				attributeSet2,
				attributeSet3
			};

			var entities = new List<Import.Entity>();
			foreach (var attributeSet in attributeSets)
			{
				var entityGuid = Guid.NewGuid();
				//if (attributeSet.StaticName == "AttributeSet_130832_1")
				//	entityGuid = new Guid("fe4ea5d9-7edc-4080-8ae3-12c4c3176b32");	// Test update existing Entity

				var entity1 = GetEntity(assignmentObjectTypeId, attributeSet.StaticName, Guid.NewGuid(), 123, attributeSet);
				var entity2 = GetEntity(assignmentObjectTypeId, attributeSet.StaticName, entityGuid, null, attributeSet, entity1.EntityGuid);
				entities.Add(entity1);
				entities.Add(entity2);
			}
			#endregion

			// Start the Import
			var import = new Import.Import(2, 2, "ImportTest");
			var importTransaction = import.RunImport(attributeSets, entities, false, false);
			importTransaction.Commit();
			//importTransaction.Rollback();


			foreach (var logItem in import.ImportLog)
			{
				string message;
				switch (logItem.EntryType)
				{
					case EventLogEntryType.Error:
						message = logItem.Message;
						if (logItem.Exception != null)
							message += ". " + logItem.Exception.Message;
						Trace.TraceError(message);
						break;
					case EventLogEntryType.Warning:
						message = logItem.Message;
						if (logItem.Exception != null)
							message += ". " + logItem.Exception.Message;
						Trace.TraceWarning(message);
						break;
					case EventLogEntryType.Information:
						message = logItem.Message;
						if (logItem.Exception != null)
							message += ". " + logItem.Exception.Message;
						Trace.TraceInformation(message);
						break;
					default:
						message = logItem.Message;
						if (logItem.Exception != null)
							message += ". " + logItem.Exception.Message;
						Trace.WriteLine(message);
						break;
				}
			}
			Assert.IsFalse(import.ImportLog.Any(l => l.EntryType == EventLogEntryType.Error));
		}

		private Import.AttributeSet GetAttributeSet(int attributeSetNumber, string now)
		{
			var name = "AttributeSet_" + now + "_" + attributeSetNumber;
			var attributes = GetAttributes(attributeSetNumber, now);
			return new Import.AttributeSet
			{
				StaticName = name,
				Name = name + " FriendlyName",
				Description = name + " Description",
				Scope = "2SexyContent",
				Attributes = attributes,
				TitleAttribute = attributes.Last()
			};
		}

		private List<Import.Attribute> GetAttributes(int attributeSetNumber, string now)
		{
			return new List<Import.Attribute>
			{
				new Import.Attribute { StaticName = "SampleAttribute_" + now + "_" + attributeSetNumber + "_Bool", Type = "Boolean", AttributeMetaData = GetAttributeMetaData(attributeSetNumber, now, "Bool") },
				new Import.Attribute { StaticName = "SampleAttribute_" + now + "_" + attributeSetNumber + "_DateTime", Type = "DateTime", AttributeMetaData = GetAttributeMetaData(attributeSetNumber, now, "DateTime") },
				new Import.Attribute { StaticName = "SampleAttribute_" + now + "_" + attributeSetNumber + "_Entity", Type = "Entity", AttributeMetaData = GetAttributeMetaData(attributeSetNumber, now, "Entity") },
				new Import.Attribute { StaticName = "SampleAttribute_" + now + "_" + attributeSetNumber + "_Hyperlink", Type = "Hyperlink", AttributeMetaData = GetAttributeMetaData(attributeSetNumber, now, "Hyperlink") },
				new Import.Attribute { StaticName = "SampleAttribute_" + now + "_" + attributeSetNumber + "_Number", Type = "Number", AttributeMetaData = GetAttributeMetaData(attributeSetNumber, now, "Number") },
				new Import.Attribute { StaticName = "SampleAttribute_" + now + "_" + attributeSetNumber + "_String", Type = "String", AttributeMetaData = GetAttributeMetaData(attributeSetNumber, now, "String") },
				//new Import.Attribute { StaticName = "SampleAttribute_" + now + "_" + attributeSetNumber + "_String2", Type = "String", AttributeMetaData = GetAttributeMetaData(attributeSetNumber, now, "String") },
			};
		}

		private List<Import.Entity> GetAttributeMetaData(int attributeSetNumber, string now, string type)
		{
			var allEntity = new Import.Entity { AttributeSetStaticName = "@All" };
			allEntity.Values = new Dictionary<string, List<IValueImportModel>>
			{
				{"Name",new List<IValueImportModel>{new ValueImportModel<string>(allEntity) {Value = "Simple Attribute " + now + " " + attributeSetNumber + " " + type}}},
				{"Notes",new List<IValueImportModel>{new ValueImportModel<string>(allEntity) {Value = "Notes for Simple Attribute " + now + " " + attributeSetNumber + " " + type}}},
				{"VisibleInEditUI", new List<IValueImportModel> {new ValueImportModel<bool?>(allEntity) {Value = true}}}
			};

			Import.Entity typeEntity = null;

			switch (type)
			{
				case "DateTime":
					typeEntity = new Import.Entity { AttributeSetStaticName = "@DateTime" };
					typeEntity.Values = new Dictionary<string, List<IValueImportModel>>
					{
						{ "UseTimePicker", new List<IValueImportModel> { new ValueImportModel<bool?>(typeEntity) { Value = true } } }
					};
					break;
				case "Entity":
					typeEntity = new Import.Entity { AttributeSetStaticName = "@Entity" };
					typeEntity.Values = new Dictionary<string, List<IValueImportModel>>
					{
						{"AllowMultiValue", new List<IValueImportModel> {new ValueImportModel<bool?>(typeEntity) {Value = true}}},
						{"EntityType", new List<IValueImportModel> {new ValueImportModel<string>(typeEntity) {Value = ""}}}
					};
					break;
				case "Hyperlink":
					typeEntity = new Import.Entity { AttributeSetStaticName = "@Hyperlink" };
					typeEntity.Values = new Dictionary<string, List<IValueImportModel>>
					{
						{"DefaultDialog", new List<IValueImportModel> {new ValueImportModel<string>(typeEntity) {Value = "PagePicker"}}},
						{"ShowPagePicker", new List<IValueImportModel> {new ValueImportModel<bool?>(typeEntity) {Value = true}}},
						{"ShowImageManager", new List<IValueImportModel> {new ValueImportModel<bool?>(typeEntity) {Value = true}}},
						{"ShowFileManager", new List<IValueImportModel> {new ValueImportModel<bool?>(typeEntity) {Value = true}}},
					};
					break;
				case "Number":
					typeEntity = new Import.Entity { AttributeSetStaticName = "@Number" };
					typeEntity.Values = new Dictionary<string, List<IValueImportModel>>
					{
						{"Decimals", new List<IValueImportModel> {new ValueImportModel<decimal?>(typeEntity) {Value = 0}}},
						{"Min", new List<IValueImportModel> {new ValueImportModel<decimal?>(typeEntity) {Value = 0}}},
						{"Max", new List<IValueImportModel> {new ValueImportModel<decimal?>(typeEntity) {Value = 360}}},
						{"InputType", new List<IValueImportModel> {new ValueImportModel<string>(typeEntity) {Value = "gps"}}},
					};
					break;
				case "String":
					typeEntity = new Import.Entity { AttributeSetStaticName = "@String" };
					typeEntity.Values = new Dictionary<string, List<IValueImportModel>>
					{
						{"InputType", new List<IValueImportModel> {new ValueImportModel<string>(typeEntity) {Value = "wysiwyg"}}},
						{"WysiwygHeight", new List<IValueImportModel> {new ValueImportModel<decimal?>(typeEntity) {Value = 250}}}
					};
					break;
			}

			var entities = new List<Import.Entity> { allEntity };
			if (typeEntity != null)
				entities.Add(typeEntity);
			return entities;
		}

		private Import.Entity GetEntity(int assignmentObjectTypeId, string attributeSetStaticName, Guid? entityGuid, int? keyNumber, Import.AttributeSet attributeSet, Guid? relatedEntity = null)
		{
			var entity = new Import.Entity
			{
				AssignmentObjectTypeId = assignmentObjectTypeId,
				AttributeSetStaticName = attributeSetStaticName,
				EntityGuid = entityGuid,
				KeyNumber = keyNumber
			};
			entity.Values = GetEntityValues(entity, attributeSet, relatedEntity);

			return entity;
		}

		private Dictionary<string, List<IValueImportModel>> GetEntityValues(Import.Entity entity, Import.AttributeSet attributeSet, Guid? relatedEntity = null, bool isUpdate = false, string now = null)
		{
			var values = new Dictionary<string, List<IValueImportModel>>();

			foreach (var attribute in attributeSet.Attributes)
			{
				values.Add(attribute.StaticName, new List<IValueImportModel>
				{
					GetValue(entity, relatedEntity, attribute, "de-de", isUpdate, now),
					GetValue(entity, relatedEntity, attribute, "en-us", isUpdate, now)
				});
			}

			// Test value for invalid AttributeSet
			//values.Add("Test must fail", new List<IValueImportModel> { GetValue(entity, relatedEntity, attributeSet.Attributes.First(), "de-de") });

			return values;
		}

		private static IValueImportModel GetValue(Import.Entity entity, Guid? relatedEntity, Import.Attribute attribute, string language, bool isUpdate, string now)
		{
			IValueImportModel value;
			switch (attribute.Type)
			{
				case "Boolean":
					//value = new ValueImportModel<bool?>(entity) { Value = true };
					value = new ValueImportModel<bool?>(entity);	// Test null-Value
					break;
				case "DateTime":
					value = new ValueImportModel<DateTime?>(entity) { Value = DateTime.Now };
					break;
				case "Entity":
					var entities = new List<Guid>();
					if (relatedEntity.HasValue)
						entities.Add(relatedEntity.Value);
					value = new ValueImportModel<List<Guid>>(entity) { Value = entities };
					break;
				case "Hyperlink":
					value = new ValueImportModel<string>(entity) { Value = "http://www.2sic.com" + (isUpdate ? " Updated " + now : "") };
					break;
				case "Number":
					value = new ValueImportModel<decimal?>(entity) { Value = (decimal?)123.12d };
					break;
				case "String":
					value = new ValueImportModel<string>(entity) { Value = "SampleString " + attribute.StaticName + " " + language + (isUpdate ? " Updated" + now : "") };
					break;
				default:
					throw new ArgumentOutOfRangeException(attribute.Type);
			}

			value.ValueDimensions = new List<Import.ValueDimension>
			{
				new Import.ValueDimension {DimensionExternalKey = language, ReadOnly = false}
			};
			return value;
		}

		[Test]
		public void UpdateExistingData()
		{
			// Update Attribute Set
			const string attributeSetStaticName = "AttributeSet_181655_1";
			const string now = "181655";
			var attributeSet = new Import.AttributeSet
			{
				Attributes = GetAttributes(1, now),
				Description = "New Description",
				Name = "New Name",
				Scope = "New Scope",
				StaticName = attributeSetStaticName,
			};
			attributeSet.Attributes.Add(new Import.Attribute { StaticName = "AttributeAddedLater", Type = "String" });
			attributeSet.TitleAttribute = attributeSet.Attributes.Last();

			const bool overwriteExistingEntityValues = true;
			var import = new Import.Import(2, 2, "ImportTest2", overwriteExistingEntityValues);
			var attributeSets = new List<Import.AttributeSet> { attributeSet };

			// Update Entity
			const int entityId = 5483;
			var db = new EavContext();
			var entityGuid = db.Entities.Where(e => e.EntityID == entityId).Select(e => e.EntityGUID).Single();

			var entity = new Import.Entity
			{
				EntityGuid = entityGuid,
				AttributeSetStaticName = attributeSetStaticName,
				KeyNumber = 999,
				AssignmentObjectTypeId = 1
			};
			entity.Values = GetEntityValues(entity, attributeSet, isUpdate: true, now: DateTime.Now.ToLongTimeString().Replace(":", ""));
			var entities = new List<Import.Entity> { entity };

			import.RunImport(null, entities);

			Assert.IsEmpty(import.ImportLog);
			Assert.IsFalse(import.ImportLog.Any(l => l.EntryType == EventLogEntryType.Error));
		}

		[Test]
		public void UpdateExistingDataMultilingual(int appId = 2, int entityId = 5449)
		{
			var db = EavContext.Instance(appId: appId);
			var entityGuid = db.Entities.Where(e => e.EntityID == entityId).Select(e => (Guid?)e.EntityGUID).SingleOrDefault();
			var attributeSetStaticName = db.AttributeSets.Where(a => a.AppID == db.AppId && a.Name == "News").Select(a => a.StaticName).Single();

			var entity = new Import.Entity
			{
				EntityGuid = entityGuid,
				AttributeSetStaticName = attributeSetStaticName,
				AssignmentObjectTypeId = 1
			};
			// Title first Import
			var titleValue = new List<IValueImportModel>
			{
				new ValueImportModel<string>(entity)
				{
					Value = "Third News en & de",
					ValueDimensions = new List<Import.ValueDimension>
					{
						new Import.ValueDimension {DimensionExternalKey = "en-us", ReadOnly = false},
						new Import.ValueDimension {DimensionExternalKey = "de-ch", ReadOnly = false},
					}
				}
			};
			// title second Import
			if (entityGuid.HasValue)
				titleValue = new List<IValueImportModel>
				{
					new ValueImportModel<string>(entity)
					{
						Value = "Third News en",
						ValueDimensions = new List<Import.ValueDimension>
						{
							new Import.ValueDimension {DimensionExternalKey = "en-us", ReadOnly = false},
						}
					},
					new ValueImportModel<string>(entity)
					{
						Value = "Third News de",
						ValueDimensions = new List<Import.ValueDimension>
						{
							new Import.ValueDimension {DimensionExternalKey = "de-ch", ReadOnly = false},
						}
					}
				};

			entity.Values = new Dictionary<string, List<IValueImportModel>>
			{
				{"Title", titleValue},
				{"Date", new List<IValueImportModel>
				{
					new ValueImportModel<DateTime?>(entity)
					{
						Value = new DateTime(2014,3,18),
						ValueDimensions = new List<Import.ValueDimension>
						{
							new Import.ValueDimension{ DimensionExternalKey = "en-US", ReadOnly = false},
							new Import.ValueDimension{ DimensionExternalKey = "de-CH", ReadOnly = true},
						}
					}
				}},
				{"Short", new List<IValueImportModel>
				{
					new ValueImportModel<string>(entity)
					{
						Value = "Third news short",
						ValueDimensions = new List<Import.ValueDimension>{ new Import.ValueDimension{ DimensionExternalKey = "en-US", ReadOnly = false}}
					},
					new ValueImportModel<string>(entity)
					{
						Value = "Dritte News kurz",
						ValueDimensions = new List<Import.ValueDimension>{ new Import.ValueDimension{ DimensionExternalKey = "de-CH", ReadOnly = false}}
					}
				}},
				{"Long", new List<IValueImportModel>
				{
					new ValueImportModel<string>(entity)
					{
						Value = "Third news long",
						ValueDimensions = new List<Import.ValueDimension>{ new Import.ValueDimension{ DimensionExternalKey = "en-US", ReadOnly = false}}
					},
					new ValueImportModel<string>(entity)
					{
						Value = "Dritte News lang",
						ValueDimensions = new List<Import.ValueDimension>{ new Import.ValueDimension{ DimensionExternalKey = "de-CH", ReadOnly = false}}
					}
				}},
			};


			var entities = new List<Import.Entity> { entity };

			var import = new Import.Import(db.ZoneId, db.AppId, "UpdateExistingDataMultilingual", true);
			import.RunImport(null, entities);

			//Assert.IsEmpty(import.ImportLog);
			Assert.IsFalse(import.ImportLog.Any(l => l.EntryType == EventLogEntryType.Error));
		}


		[Test]
		public void UpdateEntityHavingDraft()
		{
			int appId = 2, entityId = 5454;

			var db = EavContext.Instance(appId: appId);
			var entityGuid = db.Entities.Where(e => e.EntityID == entityId && !e.ChangeLogIDDeleted.HasValue).Select(e => (Guid?)e.EntityGUID).SingleOrDefault();

			var entities = new List<Import.Entity> { GetSampleNewsEntity(db, entityGuid) };

			var import = new Import.Import(db.ZoneId, db.AppId, "UpdateDraftEntity", true);
			import.RunImport(null, entities);

			Assert.IsFalse(import.ImportLog.Any(l => l.EntryType == EventLogEntryType.Error));
		}


		private static Import.Entity GetSampleNewsEntity(EavContext db, Guid? entityGuid)
		{
			var attributeSetStaticName = db.AttributeSets.Where(a => a.AppID == db.AppId && a.Name == "News").Select(a => a.StaticName).Single();

			var entity = new Import.Entity
			{
				EntityGuid = entityGuid,
				AttributeSetStaticName = attributeSetStaticName,
				AssignmentObjectTypeId = 1
			};

			entity.Values = new Dictionary<string, List<IValueImportModel>>
			{
				{"Title", new List<IValueImportModel>
				{
					new ValueImportModel<string>(entity)
					{
						Value = "Fivth News en",
						ValueDimensions = new List<Import.ValueDimension>
						{
							new Import.ValueDimension {DimensionExternalKey = "en-us", ReadOnly = false},
						}
					},
					new ValueImportModel<string>(entity)
					{
						Value = "Fünfte News de",
						ValueDimensions = new List<Import.ValueDimension>
						{
							new Import.ValueDimension {DimensionExternalKey = "de-ch", ReadOnly = false},
						}
					}
				}},
				{"Date", new List<IValueImportModel>
				{
					new ValueImportModel<DateTime?>(entity)
					{
						Value = new DateTime(2014,3,18),
						ValueDimensions = new List<Import.ValueDimension>
						{
							new Import.ValueDimension{ DimensionExternalKey = "en-US", ReadOnly = false},
							new Import.ValueDimension{ DimensionExternalKey = "de-CH", ReadOnly = true},
						}
					}
				}},
				{"Short", new List<IValueImportModel>
				{
					new ValueImportModel<string>(entity)
					{
						Value = "Third news short",
						ValueDimensions = new List<Import.ValueDimension>{ new Import.ValueDimension{ DimensionExternalKey = "en-US", ReadOnly = false}}
					},
					new ValueImportModel<string>(entity)
					{
						Value = "Dritte News kurz",
						ValueDimensions = new List<Import.ValueDimension>{ new Import.ValueDimension{ DimensionExternalKey = "de-CH", ReadOnly = false}}
					}
				}},
				{"Long", new List<IValueImportModel>
				{
					new ValueImportModel<string>(entity)
					{
						Value = "Third news long",
						ValueDimensions = new List<Import.ValueDimension>{ new Import.ValueDimension{ DimensionExternalKey = "en-US", ReadOnly = false}}
					},
					new ValueImportModel<string>(entity)
					{
						Value = "Dritte News lang",
						ValueDimensions = new List<Import.ValueDimension>{ new Import.ValueDimension{ DimensionExternalKey = "de-CH", ReadOnly = false}}
					}
				}},
			};

			return entity;
		}
	}
}
