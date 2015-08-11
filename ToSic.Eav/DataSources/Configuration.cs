using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;

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
            cont.RegisterType<Eav.DataSources.Caches.ICache, Eav.DataSources.Caches.QuickCache>();
            cont.RegisterType<Eav.DataSources.RootSources.IRootSource, Eav.DataSources.SqlSources.EavSqlStore>();

            // register some Default Constructors
            cont.RegisterType<SqlDataSource>(new InjectionConstructor());
            cont.RegisterType<DataTableDataSource>(new InjectionConstructor());
            return cont;
        }
    }
}
