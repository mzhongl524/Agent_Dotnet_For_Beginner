using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;

namespace Mzl.AgentDotnetForBeginner
{
    internal class SessionMemoryStore3
    {
        /*
         * 适合内存存储：
                短期对话（几轮到几十轮）
                开发测试环境
                原型验证和 Demo
                单用户桌面应用

          需要持久化存储：
                长期对话（需历史记录）
                生产环境（需可靠性）
                多服务器部署
                大规模用户场景
         */

        public static async Task RunAsync()
        {
            Console.WriteLine("-------------------------------------- session memory store ------------------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            // 创建 Agent
            AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "PersonalAssistant",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是专业助理，记住用户所有信息并提供个性化服务"
                },
            });

            List<ChatMessage> messages = new List<ChatMessage>();
            AgentSession session = await agent.CreateSessionAsync();
            session.SetInMemoryChatHistory(messages);

            Console.WriteLine("准备一些历史记录，请稍等...");
            await agent.RunAsync("我叫李明，是软件工程师，喜欢咖啡", session);
            await agent.RunAsync("你记得我叫什么吗", session);
            await agent.RunAsync("我喜欢什么饮料", session);

            //  改用session中的SetInMemoryChatHistory和TryGetInMemoryChatHistory方法

            // 通过抽象接口访问  ChatMessageStore的定义已经不存在;
            //var messageStore = session.GetService<ChatMessageStore>();

            // 获取所有历史消息  由于没有实现，所有不会有数据
            session.TryGetInMemoryChatHistory(out List<ChatMessage>? messageList);
            if (messageList != null)
            {
                Console.WriteLine($"📊 消息统计:");
                Console.WriteLine($"总消息数 = {messageList.Count}");
                Console.WriteLine($"User消息 = {messageList.Count(m => m.Role == ChatRole.User)}");
                Console.WriteLine($"Assistant消息 = {messageList.Count(m => m.Role == ChatRole.Assistant)}");

                // 遍历消息
                foreach (var message in messageList)
                {
                    Console.WriteLine($"[{message.Role}]: {message.Text}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();
        }
    }
}