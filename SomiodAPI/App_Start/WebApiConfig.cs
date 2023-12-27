using System.Web.Http;

namespace SomiodAPI
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web API configuration and services

			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/somiod/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}