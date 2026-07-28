using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;
using System.ComponentModel;

namespace Mzl.AgentDotnetForBeginner
{
    internal class HumanMachineCollaboration9
    {
        //✅ ApprovalRequiredAIFunction 让敏感工具一键升级为“人机协同”模式
        //✅ ToolApprovalRequestContent + CreateResponse() 构成审批闭环，兼容同步与流式调用
        //✅ 分类注册工具 + 审批循环 + 审计记录，才是企业可落地的整体解决方案

        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ human machine collaboration --------------------------------------");
            Console.WriteLine();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            // 1. 定义并包装敏感工具
            var transferTool = AIFunctionFactory.Create(GetWeather);
            var approvalTool = new ApprovalRequiredAIFunction(transferTool);

            // 2. 创建 Agent
            var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                Name = "WeatherAssistant",
                ChatOptions = new ChatOptions
                {
                    Instructions = "你是一位能提供天气信息的乐于助人的助手。",
                    Tools = [approvalTool]
                },
            });

            // 3. 审批循环
            var userRequest = "北京今天的天气如何";
            Console.WriteLine($"问题：{userRequest}");
            var session = await agent.CreateSessionAsync();
            var response = await agent.RunAsync(userRequest, session);

            // Check if there are any approval requests.
            List<ToolApprovalRequestContent> approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();

            while (approvalRequests.Count > 0)
            {
                // Ask the user to approve each function call request.
                List<ChatMessage> userInputMessages = approvalRequests
                    .ConvertAll(functionApprovalRequest =>
                    {
                        Console.WriteLine($"代理希望调用以下函数，请回复 Y 以批准：名称 {((FunctionCallContent)functionApprovalRequest.ToolCall).Name}");
                        bool approved = Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false;
                        return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(approved)]);
                    });

                response = await agent.RunAsync(userInputMessages, session);
                approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();
            }

            Console.WriteLine($"\n回复： {response}");
        }

        [Description("获取指定地点的天气信息。")]
        private static string GetWeather([Description("获取天气信息的位置。")] string location)
                => $"{location}的天气多云，最高气温为15°C";
    }
}