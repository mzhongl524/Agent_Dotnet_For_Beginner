using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;
using System.Text.Json;

namespace Mzl.AgentDotnetForBeginner
{
    //✅ MAF 集成方式：通过 ChatMessageStoreFactory 配置，AgentThread 自动管理
    //✅ 仅限本地存储：OpenAI Chat Completion 可用，Azure AI Foundry 不可用
    //✅ 持久化友好：序列化/反序列化时自动保留裁剪后的状态
    //✅ 灵活策略：可使用内置 Reducer 或自定义关键词过滤等策略
    //✅ 性能优先：高频场景优先使用 MessageCountingChatReducer

    internal class SessionReduce5
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ session reduce ----------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

#pragma warning disable MEAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "笑话大师",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是一个擅长讲笑话的幽默助手。",
                },
                // 🔑 关键配置：通过工厂方法创建带 Reducer 的消息存储
                ChatHistoryProvider = new InMemoryChatHistoryProvider(new()
                {
                    ChatReducer = new MessageCountingChatReducer(2),
                    // 自定义
                    //ChatReducer = new KeywordBasedChatReducer(keywords: new[] { "重要", "订单", "支付" }, maxMessages: 3)
                })
            });
#pragma warning restore MEAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            var session = await agent.CreateSessionAsync();

            Console.WriteLine("准备一些历史记录，请稍等...");
            Console.WriteLine("历史: [系统消息, 用户1, 助手1] -- 讲一个关于海盗的笑话");
            // 第 1 轮对话
            Console.WriteLine(await agent.RunAsync("讲一个关于海盗的笑话", session));
            // 历史: [系统消息, 用户1, 助手1]
            Console.WriteLine();

            Console.WriteLine("历史: [系统消息, 用户1, 助手1, 用户2, 助手2] -- 讲一个关于机器人的笑话");
            // 第 2 轮对话
            Console.WriteLine(await agent.RunAsync("讲一个关于机器人的笑话", session));
            // 历史: [系统消息, 用户1, 助手1, 用户2, 助手2]
            Console.WriteLine();

            Console.WriteLine("历史: [系统消息, 用户2, 助手2, 用户3, 助手3] ✅ 用户1/助手1 被裁剪 -- 讲一个关于机器人讲一个关于狐猴的笑话的笑话");
            // 第 3 轮对话
            Console.WriteLine(await agent.RunAsync("讲一个关于狐猴的笑话", session));
            // 历史: [系统消息, 用户2, 助手2, 用户3, 助手3] ✅ 用户1/助手1 被裁剪
            Console.WriteLine();

            Console.WriteLine("开始序列化并保存session内容，请稍等...");
            // ⭐ 序列化 Session（保存完整对话历史）
            JsonElement serializedSession = await agent.SerializeSessionAsync(session);

            // 转换为字符串并保存到存储
            string jsonString = JsonSerializer.Serialize(serializedSession);
            var filePath = Path.Combine(AppContext.BaseDirectory, $"sessions/session5.txt");
            await File.WriteAllTextAsync(filePath, jsonString);

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 保存完毕 -------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("------------------------------------ 恢复保存的session ------------------------------------------------");
            // 从存储加载 JSON 数据
            string jsonStringRecovery = await File.ReadAllTextAsync(filePath);

            // 解析为 JsonElement
            JsonElement reloadedJson = JsonSerializer.Deserialize<JsonElement>(jsonStringRecovery);

            // ⭐ 使用 Agent 反序列化恢复 Session
            var resumedSession = await agent.DeserializeSessionAsync(reloadedJson);
            // Reducer 仍然生效
            Console.WriteLine();
            Console.WriteLine("恢复后的提问：讲一个关于大象的笑话");
            Console.WriteLine(await agent.RunAsync("讲一个关于大象的笑话", resumedSession));

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();
        }
    }
}