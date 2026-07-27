using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;
using System.ComponentModel;

namespace Mzl.AgentDotnetForBeginner
{
    //✅ Function Calling 原理：让 Agent 调用外部函数获取信息或执行操作
    //✅ 三步创建工具：定义函数 → 添加 Description → 注册到 Agent
    //✅ ChatToolMode 配置：Auto（推荐）、Required（强制）、None（禁用）
    //✅ 多工具 + Thread：支持多轮对话中智能组合使用多个工具
    //✅ 最佳实践：清晰描述、友好错误处理、快速响应、基本类型参数

    internal class ToolCalling6
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ tool calling --------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            // 将 C# 方法转换为 AI 工具
            var weatherTool = AIFunctionFactory.Create(GetWeather);

            // 创建 Agent 并注册工具
            AIAgent weatherAssistant = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "WeatherAssistant",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是专业的天气助手。询问天气时使用 GetWeather 工具查询实时信息。",
                    Tools = [weatherTool]
                },
            });

            var userReqeust = "上海的天气怎么样?";
            // 测试 1：明确的天气查询 → Agent 自动调用工具
            Console.WriteLine($"测试1：{userReqeust}");
            await foreach (var response in weatherAssistant.RunStreamingAsync(userReqeust))
            {
                Console.Write(response);
            }
            Console.WriteLine();
            Console.WriteLine();
            // 输出：上海今天是多云，温度20度

            // 测试 2：不需要工具 → Agent 直接回答
            userReqeust = "你好,你是谁?";
            Console.WriteLine($"测试2：{userReqeust}");
            await foreach (var response in weatherAssistant.RunStreamingAsync(userReqeust))
            {
                Console.Write(response);
            }
            Console.WriteLine();
            Console.WriteLine();
            // 输出：你好！我是天气助手...

            // 测试 3：多城市对比 → Agent 自动调用两次工具
            userReqeust = "北京和深圳哪个天气更好?";
            Console.WriteLine($"测试3：{userReqeust}");
            await foreach (var response in weatherAssistant.RunStreamingAsync(userReqeust))
            {
                Console.Write(response);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            // 多工具 + 多轮对话：构建智能旅行助手
            AIAgent travelAssistant = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "TravelAssistant",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是专业的旅行顾问，使用工具查询景点、美食和酒店信息。",
                    Tools = [
                        AIFunctionFactory.Create(GetWeather),
                        AIFunctionFactory.Create(GetAttractions),
                        AIFunctionFactory.Create(GetLocalFood),
                        AIFunctionFactory.Create(GetHotelPrice)],
                    ToolMode = ChatToolMode.Auto // 自动判断
                },
            });

            // 创建对话
            AgentSession travelSession = await travelAssistant.CreateSessionAsync();

            // 第一轮：初步咨询
            userReqeust = "我想去杭州旅游,能给我一些建议吗?";
            Console.WriteLine($"测试1：{userReqeust}");
            await foreach (var response in weatherAssistant.RunStreamingAsync(userReqeust, travelSession))
            {
                Console.Write(response);
            }
            Console.WriteLine();
            Console.WriteLine();
            // Agent 自动调用 GetAttractions("杭州")、GetWeather("杭州")

            // 第二轮：深入询问（Agent 记住上下文）
            userReqeust = "我那边有什么好吃的?";
            Console.WriteLine($"测试1：{userReqeust}");
            await foreach (var response in weatherAssistant.RunStreamingAsync(userReqeust, travelSession))
            {
                Console.Write(response);
            }
            Console.WriteLine();
            Console.WriteLine();
            // Agent 自动调用 GetLocalFood("杭州") - 记住了之前讨论的城市

            // 第三轮：综合决策
            userReqeust = "酒店价格怎么样?我预算不高。";
            Console.WriteLine($"测试3：{userReqeust}");
            await foreach (var response in weatherAssistant.RunStreamingAsync(userReqeust))
            {
                Console.Write(response);
            }
            // Agent 调用 GetHotelPrice("杭州") 并推荐经济型酒店

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();
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

        // 工具 1：景点推荐
        [Description("推荐指定城市的热门旅游景点")]
        private static string GetAttractions([Description("城市名称")] string city)
        {
            var scenicSpot = new Dictionary<string, string>
            {
                ["北京"] = "颐和园",
                ["上海"] = "外滩",
                ["深圳"] = "梧桐山",
                ["杭州"] = "西湖",
            };

            return scenicSpot.TryGetValue(city, out var scenic)
                ? $"{city}的景点有: {scenic}等"
                : $"暂无{city}的景点信息";
        }

        // 工具 2：美食推荐
        [Description("推荐指定城市的特色美食")]
        private static string GetLocalFood([Description("城市名称")] string city)
        {
            var localFood = new Dictionary<string, string>
            {
                ["北京"] = "烤鸭",
                ["上海"] = "鹅肝",
                ["深圳"] = "金枪鱼",
                ["杭州"] = "西湖醋鱼",
            };

            return localFood.TryGetValue(city, out var food)
                ? $"{city}的美食有: {food}等"
                : $"暂无{city}的美食信息";
        }

        // 工具 3：酒店价格
        [Description("查询指定城市的酒店平均价格")]
        private static string GetHotelPrice([Description("城市名称")] string city)
        {
            var hotelPrice = new Dictionary<string, (string hotel, int price)>
            {
                ["北京"] = ("锦江之星", 150),
                ["上海"] = ("如家", 200),
                ["深圳"] = ("香格里拉", 925),
                ["杭州"] = ("锦江之星", 225)
            };

            return hotelPrice.TryGetValue(city, out var hp)
                ? $"{city}的酒店: {hp.hotel}, 价格: {hp.price}元/每晚"
                : $"暂无{city}的酒店价格信息";
        }
    }
}