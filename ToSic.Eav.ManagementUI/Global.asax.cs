using System;
using System.Web.Http;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.ImportExport.Refactoring.ValueConverter;

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

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}