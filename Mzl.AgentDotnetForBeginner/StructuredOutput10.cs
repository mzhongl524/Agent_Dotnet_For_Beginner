using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;

namespace Mzl.AgentDotnetForBeginner
{
    internal class StructuredOutput10
    {
        // 非泛型方式（兼容所有返回类型）
        //        创建 Agent 时配置 ChatOptions.ResponseFormat
        //        调用 RunAsync()，再用 response.Deserialize<T>()
        // 泛型方式（推荐）
        //        无需预先配置，直接 await agent.RunAsync<PersonInfo>(...)
        //        自动生成 JSON Schema、设置 ResponseFormat、反序列化到 Result

        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ strucured output --------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "PersonInfoExtractor",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "提取姓名、年龄、职业、地点"
                }
            });

            var session = await agent.CreateSessionAsync();
            var userRequest = "张伟，35岁，软件工程师，在北京";
            Console.WriteLine($"测试：{userRequest}");
            var result = await agent.RunAsync<PersonInfo>(userRequest, session);
            var personInfo = result.Result;
            Console.WriteLine($"姓名：{personInfo.Name}，年龄：{personInfo.Age}，职业：{personInfo.Profession}，地址：{personInfo.Address}");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();
        }

        internal class PersonInfo
        {
            public string Name { get; set; }
            public int Age { get; set; }

            public string Profession { get; set; }
            public string Address { get; set; }
        }
    }
}