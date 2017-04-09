using SportsStore.Domain.Entities;
using SportsStore.WebUI.Infrastructure.Binders;
using System.Web.Mvc;
using System.Web.Routing;

namespace SportsStore.WebUI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // 告訴 MVC 框架使用 CartModelBinder 類來創建 Cart 實例
            ModelBinders.Binders.Add(typeof(Cart), new CartModelBinder());
        }
    }
}