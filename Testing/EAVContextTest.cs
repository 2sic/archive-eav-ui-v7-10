using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ToSic.EAV;

namespace ToSic.EAV.Testing
{
	[TestFixture]
	class EAVContextTest
	{
		[Test]
		public void TestCache()
		{
			var Context = new EAVContext();
			Context.ItemCachingEnabled = true;

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
			var Context = new EAVContext();

			var Entity = Context.GetEntity(277);

			//Context.UpdateEntity();

		}

		[Test]
		public void DimensionsCacheTest()
		{
			var Context = new EAVContext();

			var Entity1 = Context.GetLanguages();
			var Entity2 = Context.GetLanguages();
			var Entity3 = Context.GetLanguages();

			//Context.UpdateEntity();

		}

		[Test]
		public void TestGetEntity()
		{
			var Context = new EAVContext();
			IEntity entity = Context.GetEntityModel(303);

			var notesAttrib = (IAttribute<string>) entity["Notes"];
			//notesAttrib.TypedContents
			

			IAttribute firstNameAttrib = entity["FirstName"];
			var firstName = (IAttribute<string>)firstNameAttrib;
			
			var firstNameContents = firstName.TypedContents;
		}

	}
}
