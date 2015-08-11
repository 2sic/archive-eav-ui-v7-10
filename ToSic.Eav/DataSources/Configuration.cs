using Microsoft.Practices.Unity;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.RootSources;
using ToSic.Eav.DataSources.SqlSources;

namespace ToSic.Eav.DataSources
{
    public class Configuration
    {
        /// <summary>
        /// Register Types in Unity Container
        /// </summary>
        /// <remarks>If Unity is not configured in App/Web.config this can be used</remarks>
        public IUnityContainer ConfigureDefaultMappings(IUnityContainer cont)
        {
            cont.RegisterType<ICache, QuickCache>();
            cont.RegisterType<IRootSource, EavSqlStore>();

            // register some Default Constructors
            cont.RegisterType<SqlDataSource>(new InjectionConstructor());
            cont.RegisterType<DataTableDataSource>(new InjectionConstructor());
            return cont;
        }
    }
}
