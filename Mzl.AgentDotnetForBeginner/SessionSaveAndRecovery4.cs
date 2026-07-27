using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;
using System.Text.Json;

namespace Mzl.AgentDotnetForBeginner
{
    internal class SessionSaveAndRecovery4
    {
        // 业务价值：避免用户重复描述，提升服务效率和满意度
        //企业场景：在线客服、审批流程、无状态服务、多渠道接入
        //最佳实践：每次对话后立即保存，做好异常处理和数据加密

        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ session save and recovery ----------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            // 创建 Agent 并开始对话
            var agent = chatClient.AsAIAgent(new ChatClientAgentOptions()
            {
                Name = "PersonalAssistant",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是专业助理，记住用户所有信息并提供个性化服务"
                },
            });
            var session = await agent.CreateSessionAsync();

            // 用户与 AI 进行多轮对话
            Console.WriteLine("准备一些历史记录，请稍等...");
            await agent.RunAsync("我叫李明，是软件工程师，喜欢咖啡", session);
            await agent.RunAsync("你记得我叫什么吗", session);

            Console.WriteLine("开始序列化并保存session内容，请稍等...");
            // ⭐ 序列化 Session（保存完整对话历史）
            JsonElement serializedSession = await agent.SerializeSessionAsync(session);

            // 转换为字符串并保存到存储
            string jsonString = JsonSerializer.Serialize(serializedSession);
            var filePath = Path.Combine(AppContext.BaseDirectory, $"sessions/session4.txt");
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

            Console.WriteLine("已恢复session并开始新的提问(--我喜欢什么饮料--)，请稍等...");
            // 基于恢复的 Session  继续对话
            var response = await agent.RunAsync("我喜欢什么饮料", resumedSession);
            Console.WriteLine(response);

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();
        }
    }
}