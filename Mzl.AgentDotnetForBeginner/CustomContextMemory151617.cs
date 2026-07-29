using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;

namespace Mzl.AgentDotnetForBeginner
{
    //✅ 完整实现：InvokingAsync（注入）+ InvokedAsync（提取）+ Serialize（持久化）
    //✅ 智能询问：根据记忆状态动态生成 Instructions
    //✅ 序列化支持：保存和恢复对话状态
    //✅ 跨 Thread 共享：通过直接赋值实现记忆共享

    internal class CustomContextMemory151617
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ cutom context memory --------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是一个非常友好的助理",
                },
                AIContextProviders = [new UserInfoMemory(chatClient)]
            });

            var session = await agent.CreateSessionAsync();

            var userRequest = "你好";
            Console.WriteLine($"问题1：{userRequest}");
            await foreach (var assist in agent.RunStreamingAsync(userRequest, session))
            {
                Console.Write(assist);
            }
            Console.WriteLine();

            userRequest = "我叫张三";
            Console.WriteLine($"问题2：{userRequest}");
            await foreach (var assist in agent.RunStreamingAsync(userRequest, session))
            {
                Console.Write(assist);
            }
            Console.WriteLine();

            userRequest = "25岁";
            Console.WriteLine($"问题3：{userRequest}");
            await foreach (var assist in agent.RunStreamingAsync(userRequest, session))
            {
                Console.Write(assist);
            }
            Console.WriteLine();

            userRequest = "天气怎么样？";
            Console.WriteLine($"问题4：{userRequest}");
            await foreach (var assist in agent.RunStreamingAsync(userRequest, session))
            {
                Console.Write(assist);
            }
            Console.WriteLine();

            // 序列化当前状态
            var serialized = await agent.SerializeSessionAsync(session);

            // 恢复对话
            Console.WriteLine("恢复会话...");
            var restoredSession = await agent.DeserializeSessionAsync(serialized);
            var userInfo = agent.GetService<UserInfoMemory>()?.GetUserInfo(restoredSession);
            Console.WriteLine($"用户名：{userInfo.UserName}， 年龄：{userInfo.UserAge}");  // "张三"

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();
        }
    }
}