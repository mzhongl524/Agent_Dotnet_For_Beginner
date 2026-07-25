using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;
using OpenAI.Chat;

namespace Mzl.AgentDotnetForBeginner
{
    internal class FisrtAgent1
    {
        // ChatClient = 纯函数：给定输入，返回输出，不保留状态
        // Agent = 有记忆的助手：有固定身份、能记住上下文、能使用工具

        public static async Task RunAsync()
        {
            Console.WriteLine("-------------------------------------- first agent ------------------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            // 2. 创建 Agent
            AIAgent spokenEnglishCoach = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "SpokenEnglishCoach",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是一位专业的英语口语教练。帮助学生提升英语口语能力，始终保持鼓励和友好的态度。"
                }
            });

            // 2. 创建 Agent (文中写法)
            //AIAgent spokenEnglishCoach = chatClient.CreateAIAgent(
            //    instructions: "你是一位专业的英语口语教练。帮助学生提升英语口语能力，始终保持鼓励和友好的态度。",
            //    name: "SpokenEnglishCoach"
            //);
            var userMessage = "我想提高英语口语能力，但不知道从哪里开始。你能给我一些建议吗?";
            Console.WriteLine($"发送消息(streaming)：{userMessage}");
            Console.WriteLine("---------------------------- 等待回复 (non-streaming) -------------------------------------");
            Console.WriteLine();

            // 3. 调用 Agent (同步模式)
            var response = await spokenEnglishCoach.RunAsync(userMessage);

            Console.WriteLine($"🤖 {spokenEnglishCoach.Name}: {response}");

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            userMessage = "请讲解如何练习英语的连读技巧，并给出例子。";
            Console.WriteLine($"发送消息(streaming)：{userMessage}");
            Console.WriteLine("-------------------------------- 等待回复  (streaming)---------------------------------------");
            Console.WriteLine();
            // 流式调用，逐块输出
            await foreach (var chunk in spokenEnglishCoach.RunStreamingAsync(userMessage))
            {
                Console.Write(chunk); // 逐块输出，不换行
            }

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ------------------------------------------------");
            Console.WriteLine();

            var chatOpenClient = AIClientHelper.GetDefaultOpenAIChatClient();
            AIAgent agent = chatOpenClient.AsAIAgent(new ChatClientAgentOptions
            {
                // 配置一样，后面的打印输出等内容也是一样，不重复写
                Name = "",
                ChatOptions = new ChatOptions
                {
                    Instructions = ""
                }
            });
        }
    }
}