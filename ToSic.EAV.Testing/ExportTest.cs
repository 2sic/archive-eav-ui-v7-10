using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using ToSic.Eav.ImportExport;

namespace ToSic.Eav.Testing
{
	class ExportTest
	{
		private readonly EavContext _ctx = EavContext.Instance(appId: 2);

		[Test]
		public void EntityExportTest()
		{
			var entity = _ctx.Entities.Single(e => e.EntityID == 317);
			var dsrc = DataSource.GetInitialDataSource(_ctx.AppId);
			var entityModel = dsrc[DataSource.DefaultStreamName].List[317];
			

			var export = new XmlExport(_ctx);
			var entityXElement = export.GetEntityXElement(entity, ExtendValueDelegate);
			var entityXElement2 = export.GetEntityXElement(entityModel);
			Debug.Write(entityXElement);
			Debug.Write(entityXElement2);
		}

		private void ExtendValueDelegate(string attributeStaticname, string attributeSetStaticName, string value, XElement valueXElement)
		{
			// Special cases for Template ContentTypes
			if (attributeSetStaticName == "2SexyContent-Template-ContentTypes" && !String.IsNullOrEmpty(value))
			{
				switch (attributeStaticname)
				{
					case "ContentTypeID":
						var attributeSet = _ctx.GetAllAttributeSets().FirstOrDefault(a => a.AttributeSetID == int.Parse(valueXElement.Attribute("Value").Value));
						valueXElement.Attribute("Value").SetValue(attributeSet != null ? attributeSet.StaticName : String.Empty);
						break;
					case "DemoEntityID":
						var entityID = int.Parse(valueXElement.Attribute("Value").Value);
						var demoEntity = _ctx.Entities.FirstOrDefault(e => e.EntityID == entityID);
						valueXElement.Attribute("Value").SetValue(demoEntity != null ? demoEntity.EntityGUID.ToString() : String.Empty);
						break;
				}
			}
		}
	}
}
