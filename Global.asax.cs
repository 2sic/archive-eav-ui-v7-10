using System;
using System.Web.Http;
using System.Web.Routing;
using Microsoft.Practices.Unity;

namespace ToSic.Eav.ManagementUI
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/EAV/{controller}/{action}"
            );

            new Configuration().ConfigureDefaultMappings(Factory.Container);
            Factory.Container.RegisterType(typeof(IEavValueConverter), typeof(NeutralValueConverter), new InjectionConstructor());
        }
    }
}