using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using Mzl.Agent.Comm;
using System.ComponentModel;

namespace Mzl.AgentDotnetForBeginner
{
    internal class AgentAsFuc14
    {
        //✅ AsAIFunction：应用内嵌套，性能高，适合层次化架构
        //✅ MCP Tool：跨平台互操作，支持 Claude/VS Code 等客户端
        //✅ 混合使用：内部 Agent 用 AsAIFunction，外部服务用 MCP
        //✅ 企业应用：构建可复用、可组合的 Agent 生态

        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ agent as fuction --------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            // 2 创建子 Agent（天气助手）
            var weatherAgent = chatClient.AsAIAgent(
                instructions: "你是天气查询助手",
                name: "WeatherAgent",
                tools: [AIFunctionFactory.Create(GetWeather)]
            );

            // 3 转换为 AIFunction
            var weatherFunction = weatherAgent.AsAIFunction();

            // 4 注册到主 Agent
            var travelAgent = chatClient.AsAIAgent(
                instructions: "你是旅行助手，可以调用天气助手查询天气",
                name: "TravelAgent",
                tools: [weatherFunction]  // 注册子 Agent 作为工具
            );

            Console.WriteLine("问题：我想去上海旅游，不知道天气如何？");
            await foreach (var travel in travelAgent.RunStreamingAsync("我想去上海旅游，有什么建议？"))
            {
                Console.Write(travel.Text);
            }
            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            // 1️⃣ 将 Agent 转换为 MCP 工具
            var weatherAgentFunction = weatherAgent.AsAIFunction();
            var weatherMcpTool = McpServerTool.Create(weatherAgentFunction);

            // 2️⃣ 创建 MCP Server 并注册工具
            var (mcpClient, mcpServer) = await McpHelper.CreateInMemoryClientAndServerAsync(
                tools: [weatherMcpTool]
            );

            // 3️⃣ 通过 MCP 调用
            Console.WriteLine("mcp调用，问题：北京天气如何");
            var result = await mcpClient.CallToolAsync(
                toolName: "WeatherAgent",
                arguments: new Dictionary<string, object> { { "query", "北京天气如何" } }
            );

            Console.WriteLine(result.Content.FirstOrDefault());

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            //企业级实战：多 Agent 协作   智能客服系统
            //    // 创建多个专项 Agent
            //    var weatherAgent = CreateWeatherAgent();
            //    var orderAgent = CreateOrderAgent();

            //    // 通过 MCP 获取外部服务
            //    var couponMcpFunctions = await couponMcpClient.ListToolsAsync();

            //    // 组合到主 Agent
            //    var mainAgent = chatClient.CreateAIAgent(
            //        instructions: "你是智能客服助手",
            //        tools: [
            //            weatherAgent.AsAIFunction(),      // 内部 Agent
            //orderAgent.AsAIFunction(),        // 内部 Agent
            //..couponMcpFunctions.Cast<AIFunction>()  // 外部 MCP 工具
            //        ]
            //    );
        }

        [Description("查询指定城市的当前天气信息,包括天气状况和温度")]
        private static string GetWeather([Description("要查询天气的城市名称,例如: 北京、上海、深圳")] string city)
        {
            var weatherData = new Dictionary<string, (string condition, int temperature)>
            {
                ["北京"] = ("晴天", 15),
                ["上海"] = ("多云", 20),
                ["深圳"] = ("阴天", 25)
            };

            return weatherData.TryGetValue(city, out var weather)
                ? $"{city}的天气: {weather.condition}, 温度: {weather.temperature}°C"
                : $"暂无{city}的天气信息";
        }
    }
}