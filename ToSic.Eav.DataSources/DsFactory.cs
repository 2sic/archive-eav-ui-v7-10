using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// The Eav Factory, used to construct a DataSource
	/// </summary>
	public class DsFactory
	{
		private static IUnityContainer _container;

		/// <summary>
		/// The IoC Container responsible for our Inversion of Control
		/// Use this everywhere!
		/// Currently a bit of overkill, but will help with testability in the future. 
		/// </summary>
		public static IUnityContainer Container
		{
			get
			{
				if (_container == null)
				{
				    _container = ToSic.Eav.Factory.Container.CreateChildContainer();
				    // new DataSources.Configuration().ConfigureDefaultMappings(_container);

				}
				return _container;
			}
		}

	}
}