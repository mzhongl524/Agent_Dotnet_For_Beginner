using OllamaSharp;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Mzl.Agent.Comm
{
    public class AIClientHelper
    {
        public static OllamaApiClient GetDefaultOllamaApiChatClient()
        {
            var arguments = new Arguments();
            if (arguments.Provider.Equals("ollama", StringComparison.OrdinalIgnoreCase))
            {
                var httpClient = new HttpClient()
                {
                    BaseAddress = arguments.Uri,
                    Timeout = TimeSpan.FromMinutes(5),
                };
                return new OllamaApiClient(httpClient, arguments.Model);
            }
            throw new Exception("当前配置不是ollama的相关配置信息，请检查！");
        }

        public static ChatClient GetDefaultOpenAIChatClient()
        {
            var arguments = new Arguments();
            if (arguments.Provider.Equals("openai", StringComparison.OrdinalIgnoreCase))
            {
                var clientOptions = new OpenAIClientOptions
                {
                    Endpoint = arguments.Uri,
                };

                var aiClient = new OpenAIClient(new ApiKeyCredential(arguments.ApiKey), clientOptions);
                return aiClient.GetChatClient(arguments.Model);
            }
            throw new Exception("当前配置不是openai的相关配置信息，请检查！");
        }
    }
}