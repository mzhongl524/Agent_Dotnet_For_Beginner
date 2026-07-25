using System.Configuration;

namespace Mzl.Agent.Comm
{
    public class Arguments
    {
        public Arguments()
        {
            var provider = ConfigurationManager.AppSettings["Provider"];
            var url = ConfigurationManager.AppSettings["Url"];
            var model = ConfigurationManager.AppSettings["Model"];
            var apiKey = ConfigurationManager.AppSettings["ApiKey"];

            Provider = provider ?? "openai";
            Uri = new Uri(url ?? "http://localhost:11434");
            Model = model ?? "gpt-4";
            ApiKey = apiKey ?? "";
        }

        public string Provider { get; set; } = "openai";
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gpt-4";
        public Uri Uri { get; set; } = new Uri("http://localhost:11434");
    }
}