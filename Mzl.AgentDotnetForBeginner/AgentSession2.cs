using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;

namespace Mzl.AgentDotnetForBeginner
{
    internal class AgentSession2
    {
        // ✅  多轮记忆：Agent 自动记住所有对话历史
        //✅ 线程隔离：多用户场景下对话互不干扰
        //✅ 简单易用：三行代码实现带记忆的对话
        //✅ 流式支持：同时支持同步和流式多轮对话

        public static async Task RunAsync()
        {
            Console.WriteLine("-------------------------------------- agent session ------------------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            // 2. 创建 Agent
            AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "PersonalAssistant",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是专业助理，记住用户所有信息并提供个性化服务"
                }
            });

            // 3. 创建thread -- 关键   AgentThread 已改为使用AgentSession
            // https://github.com/microsoft/agent-framework/blob/c6442de52882a47fa6796fb380c213cd65f2fc8e/dotnet/src/Microsoft.Agents.AI.DurableTask/CHANGELOG.md?plain=1#L30
            //AgentThread thread = agent.GetNewThread();
            AgentSession session = await agent.CreateSessionAsync();

            Console.WriteLine($"测试同一个session的多轮对话，非流式");
            Console.WriteLine("---------------------------- 等待回复 (non-streaming) -------------------------------------");
            Console.WriteLine();

            // 4. 多轮对话（复用同一个session）
            var userRequest = "我叫李明，是软件工程师，喜欢咖啡";
            Console.WriteLine(userRequest);
            Console.WriteLine(await agent.RunAsync(userRequest, session));
            Console.WriteLine();

            userRequest = "你记得我叫什么吗";
            Console.WriteLine(userRequest);
            Console.WriteLine(await agent.RunAsync(userRequest, session));
            Console.WriteLine();

            userRequest = "我喜欢什么饮料";
            Console.WriteLine(userRequest);
            Console.WriteLine(await agent.RunAsync(userRequest, session));
            Console.WriteLine();

            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            // 流式多轮对话
            // 创建故事创作 Agent
            AIAgent storyWriter = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "StoryWriter",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是创意写作助手，记住所有故事元素保持一致性"
                }
            });

            // 创建session
            AgentSession storySession = await storyWriter.CreateSessionAsync();

            Console.WriteLine($"测试同一个session的多轮对话，流式");
            Console.WriteLine("---------------------------- 等待回复 (streaming) -------------------------------------");
            Console.WriteLine();

            // 第一轮：设定背景
            userRequest = "科幻故事，2150年火星殖民地，主角阿尔法是工程师";
            Console.WriteLine(userRequest);
            await foreach (var chunk in storyWriter.RunStreamingAsync(userRequest, storySession))
            {
                Console.Write(chunk);
            }
            Console.WriteLine();

            // 第二轮：引入冲突（Agent 记得前面设定）
            userRequest = "生命支持系统故障，阿尔法需24小时内修复";
            Console.WriteLine(userRequest);
            await foreach (var chunk in storyWriter.RunStreamingAsync(userRequest, storySession))
            {
                Console.Write(chunk);
            }
            Console.WriteLine();

            // 第三轮：添加转折（Agent 记得所有情节）
            userRequest = "阿尔法发现故障是人为破坏";
            Console.WriteLine(userRequest);
            await foreach (var chunk in storyWriter.RunStreamingAsync(userRequest, storySession))
            {
                Console.Write(chunk);
            }

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            //  多用户场景：线程隔离
            // 创建客服 Agent
            AIAgent customerService = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "CustomerService",
                ChatOptions = new ChatOptions
                {
                    Instructions = "电商客服，记住每位客户信息提供个性化服务"
                }
            });

            // 为两个用户创建独立session
            AgentSession sessionZhangSan = await customerService.CreateSessionAsync();
            AgentSession sessionLiSi = await customerService.CreateSessionAsync();

            Console.WriteLine($"测试多用户场景：线程隔离");
            Console.WriteLine($"张三的对话");
            Console.WriteLine("---------------------------- 等待回复 (non-streaming) -------------------------------------");
            Console.WriteLine();

            // 张三的对话
            userRequest = "我叫张三，想买笔记本电脑，预算8000元";
            Console.WriteLine(userRequest);
            Console.WriteLine(await customerService.RunAsync(userRequest, sessionZhangSan));
            Console.WriteLine();

            userRequest = "主要用来做软件开发";
            Console.WriteLine(userRequest);
            Console.WriteLine(await customerService.RunAsync(userRequest, sessionZhangSan));

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine($"李四的对话");
            Console.WriteLine("---------------------------- 等待回复 (non-streaming) -------------------------------------");
            Console.WriteLine();

            // 李四的对话（完全隔离）
            userRequest = "我是李四，想买游戏鼠标";
            Console.WriteLine(userRequest);
            Console.WriteLine(await customerService.RunAsync(userRequest, sessionLiSi));
            Console.WriteLine();

            userRequest = "价格300元以内";
            Console.WriteLine(userRequest);
            Console.WriteLine(await customerService.RunAsync(userRequest, sessionLiSi));

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine($"验证隔离：两个用户各自记得自己的信息");
            Console.WriteLine("---------------------------- 等待回复 (non-streaming) -------------------------------------");
            Console.WriteLine();
            // 验证隔离：两个用户各自记得自己的信息
            userRequest = "你记得我的预算和用途吗?";
            Console.WriteLine(userRequest);
            Console.WriteLine(await customerService.RunAsync(userRequest, sessionZhangSan));
            Console.WriteLine();

            userRequest = "我刚才想买什么?";
            Console.WriteLine(userRequest);
            Console.WriteLine(await customerService.RunAsync(userRequest, sessionLiSi));

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            // 获取历史消息
            IList<ChatMessage>? chatHistory = session.GetService<IList<ChatMessage>>();
            Console.WriteLine("获取历史消息");
            if (chatHistory != null)
            {
                Console.WriteLine($"总消息数: {chatHistory.Count}");

                // 统计不同角色的消息
                var userMessages = chatHistory.Count(m => m.Role == ChatRole.User);
                var assistantMessages = chatHistory.Count(m => m.Role == ChatRole.Assistant);

                // 遍历显示
                foreach (var message in chatHistory)
                {
                    Console.WriteLine($"[{message.Role}]: {message.Text}");
                }

                Console.WriteLine();
                Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            }
        }
    }
}