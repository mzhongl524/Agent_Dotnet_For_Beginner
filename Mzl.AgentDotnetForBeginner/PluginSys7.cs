using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Mzl.Agent.Comm;
using System.ComponentModel;

namespace Mzl.AgentDotnetForBeginner
{
    //✅ 插件系统价值：模块化设计，独立开发，按需扩展
    //✅ 依赖注入：使用 DI 容器管理插件依赖，提高可测试性
    //✅ 抽象基类：统一实现 AsAITools()，减少 33% 代码量
    //✅ 选择性暴露：通过 GetToolMethods() 精确控制 AI 能力边界
    //✅ 企业级标准：接口+基类架构，实现完全标准化
    //✅ 多插件集成：通过 IAgentPlugin 接口统一管理所有插件

    internal class PluginSys7
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ DI plugin system --------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            ServiceCollection services = new();
            // 3. 使用 DI 容器管理
            services.AddSingleton<WeatherProvider>();
            services.AddSingleton<WeatherPlugin2>();

            //services.AddSingleton<IAgentPlugin, StandardAgentPluginBase>();
            services.AddSingleton<IAgentPlugin, StandardWeatherPlugin>();

            var serviceProvider = services.BuildServiceProvider();

            var plugins = serviceProvider.GetRequiredService<IAgentPlugin>();

            var tools = plugins.AsAITools();

            // 4. 创建 Agent
            var agent = chatClient.AsAIAgent(
                instructions: "你是企业智能工作助手",
                name: "WorkAssistant",
                tools: tools.ToList(),
                services: serviceProvider);

            var userRequest = "北京的天气如何，麻烦告诉我";
            Console.WriteLine($"测试：{userRequest}");
            await foreach (var weather in agent.RunStreamingAsync(userRequest))
            {
                Console.Write(weather);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();
        }
    }

    // 企业级接口标准

    // 1. 定义插件接口
    public interface IAgentPlugin
    {
        string PluginName { get; }

        IEnumerable<AITool> AsAITools();

        object GetPluginInfo();
    }

    // 2. 抽象基类实现接口
    public abstract class StandardAgentPluginBase : IAgentPlugin
    {
        public virtual string PluginName => GetType().Name;

        protected abstract IEnumerable<Delegate> GetToolMethods();

        public IEnumerable<AITool> AsAITools()
        {
            return GetToolMethods()
                .Select(method => AIFunctionFactory.Create(method));
        }

        public virtual object GetPluginInfo()
        {
            return new { Name = PluginName };
        }
    }

    // 3. 插件实现接口
    public sealed class StandardWeatherPlugin : StandardAgentPluginBase
    {
        private readonly WeatherProvider _provider;

        public override string PluginName => "天气查询插件";

        public StandardWeatherPlugin(WeatherProvider provider)
        {
            _provider = provider;
        }

        [Description("查询天气")]
        public string GetWeather([Description("城市")] string location)
        {
            return _provider.GetWeather(location);
        }

        protected override IEnumerable<Delegate> GetToolMethods()
        {
            yield return this.GetWeather;
        }
    }

    // 抽象基类统一实现

    // 1. 定义抽象基类
    public abstract class AgentPluginBase
    {
        // 子类只需重写这个方法，声明要暴露的工具
        protected abstract IEnumerable<Delegate> GetToolMethods();

        // 基类统一实现 AsAITools()
        public IEnumerable<AITool> AsAITools()
        {
            return GetToolMethods()
                .Select(method => AIFunctionFactory.Create(method));
        }
    }

    // 2. 插件继承基类
    public sealed class WeatherPlugin3 : AgentPluginBase
    {
        private readonly WeatherProvider _provider;

        public WeatherPlugin3(WeatherProvider provider)
        {
            _provider = provider;
        }

        [Description("查询天气")]
        public string GetWeather([Description("城市")] string location)
        {
            return _provider.GetWeather(location);
        }

        // 只需声明要暴露的方法
        protected override IEnumerable<Delegate> GetToolMethods()
        {
            yield return this.GetWeather;
        }
    }

    // 依赖注入

    // 1. 定义服务类
    public sealed class WeatherProvider
    {
        public string GetWeather(string location)
        {
            return $"📍 {location} 天气数据: 晴转多云，温度 15°C，空气质量良好";
        }
    }

    // 2. 插件通过构造函数注入
    public sealed class WeatherPlugin2
    {
        private readonly WeatherProvider _provider;

        public WeatherPlugin2(WeatherProvider provider)
        {
            _provider = provider;
        }

        [Description("查询天气")]
        public string GetWeather([Description("城市")] string location)
        {
            return _provider.GetWeather(location);
        }
    }

    // 手动处理

    public sealed class WeatherPlugin1
    {
        [Description("查询指定城市的天气信息")]
        public string GetWeather([Description("城市名称，如：北京、上海")] string location)
        {
            return $"📍 {location}：晴转多云，温度 15°C，空气质量良好";
        }
    }

    // 注册插件
    //private var weatherPlugin = new WeatherPlugin1();

    //private var tools = new[] { AIFunctionFactory.Create(weatherPlugin.GetWeather) };
}