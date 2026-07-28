using CommunityToolkit.VectorData.InMemory;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Mzl.Agent.Comm;
using System.Text.Json;

namespace Mzl.AgentDotnetForBeginner
{
    internal class CustomFileStore8
    {
        // 参考：https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/02-agents/Agents/Agent_Step04_3rdPartyChatHistoryStorage/Program.cs
        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ custom file store --------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            VectorStore vectorStore = new InMemoryVectorStore();
            // 创建 Agent
            AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "PersonalAssistant",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是专业助理，记住用户所有信息并提供个性化服务"
                },
                ChatHistoryProvider = new VectorChatHistoryProvider(vectorStore)
            });

            AgentSession session = await agent.CreateSessionAsync();

            Console.WriteLine("准备一些历史记录，请稍等...");
            await agent.RunAsync("我叫李明，是软件工程师，喜欢咖啡", session);
            await agent.RunAsync("你记得我叫什么吗", session);

            JsonElement serializedSession = await agent.SerializeSessionAsync(session);

            Console.WriteLine("\n--- 序列化 session ---\n");
            Console.WriteLine(JsonSerializer.Serialize(serializedSession, new JsonSerializerOptions { WriteIndented = true }));

            // ⭐ 使用 Agent 反序列化恢复 Session
            var resumedSession = await agent.DeserializeSessionAsync(serializedSession);

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