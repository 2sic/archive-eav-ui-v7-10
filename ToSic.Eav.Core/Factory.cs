using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
//using ToSic.Eav.DataSources;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav
{
	/// <summary>
	/// The Eav Factory, used to construct a DataSource
	/// </summary>
	public class MaybeFactory: IIoC
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
					_container = new UnityContainer();
                    //_container = ConfigureDefaultMappings(_container);

					var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
					if (section != null && section.Containers["ToSic.Eav"] != null)
						_container.LoadConfiguration("ToSic.Eav");
				}
				return _container;
			}
		}

        /// <summary>
        /// Public property delivering the configured container - neceessary so we can access it through Interfaces
        /// </summary>
        public IUnityContainer ConfiguredContainer { get { return Container; } }

        //private static bool _DSMappingsCompleted = false;

	    public bool DataSourceMappingsReady { get; set; }
	

        ///// <summary>
        ///// Register Types in Unity Container
        ///// </summary>
        ///// <remarks>If Unity is not configured in App/Web.config this can be used</remarks>
        //private static IUnityContainer ConfigureDefaultMappings(IUnityContainer cont)
        //{
        //    cont.RegisterType<DataSources.Caches.ICache, DataSources.Caches.QuickCache>();
        //    cont.RegisterType<DataSources.RootSources.IRootSource, DataSources.SqlSources.EavSqlStore>();

        //    // register some Default Constructors
        //    _container.RegisterType<SqlDataSource>(new InjectionConstructor());
        //    _container.RegisterType<DataTableDataSource>(new InjectionConstructor());

        //    _DSMappingsCompleted = true;
        //    return cont;
        //}
	}
}