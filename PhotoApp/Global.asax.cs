using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using StructureMap;
using PhotoApp.PhotoServices;
using PhotoApp.OAuthServices;

namespace PhotoApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly HttpMethodConstraint MethodGet = new HttpMethodConstraint(new string[] { "GET" });

        private static void RegisterIoc()
        {
            ObjectFactory.Initialize(x =>
            {
                x.For<IPhotoService>().Singleton().Use<FlickrAPI>();
                x.Forward<IPhotoService, IOAuthService>();
            });
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Index", // Route name
                "", // URL with parameters
                new { controller = "Home", action = "Index" },
                new { httpMethod = MethodGet }
            );

            routes.MapRoute(
                "LogOn", // Route name
                "LogOn", // URL with parameters
                new { controller = "Home", action = "LogOn" },
                new { httpMethod = MethodGet }
            );

            routes.MapRoute(
                "LogOut", // Route name
                "LogOut", // URL with parameters
                new { controller = "Home", action = "LogOut" },
                new { httpMethod = MethodGet }
            );

            routes.MapRoute(
                "Photos", // Route name
                "photos", // URL with parameters
                new { controller = "Home", action = "Photos" }, 
                new { httpMethod = MethodGet }
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            RegisterIoc();
        }
    }
}