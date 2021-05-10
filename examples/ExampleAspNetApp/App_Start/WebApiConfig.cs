using System.Web.Http;

namespace ExampleAspNetApp {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            // Remove XML formatter.
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Add Web API routes
            config.MapHttpAttributeRoutes();
        }
    }
}
