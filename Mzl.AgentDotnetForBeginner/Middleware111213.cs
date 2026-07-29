using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Mzl.Agent.Comm;

namespace Mzl.AgentDotnetForBeginner
{
    internal class Middleware111213
    {
        // 11
        //✅ 三层架构：ChatClient、Agent Run、Function Invocation 各司其职
        //✅ 洋葱模型：请求从外向内，响应从内向外
        //✅ 灵活组合：根据需求选择合适的中间件层级
        //✅ 企业级应用：安全检查最外层，性能优化靠内层

        // 12
        //✅ 洋葱模型：请求从外向内穿透，响应从内向外返回
        //✅ 触发频率：Agent Run 1次，ChatClient/Function 多次
        //✅ 注册顺序：先注册 = 更外层 = 先执行 Pre、后执行 Post
        //✅ 实践建议：日志最外层，缓存最内层

        // 13
        //✅ 嵌套关系：UseFunctionInvocation → FunctionInvoker → MAF Middleware → 工具
        //✅ 触发次数：FunctionInvoker 触发次数 == MAF Middleware 触发次数
        //✅ 职责分离：MEAI 负责自动化，MAF 负责业务逻辑
        //✅ 最佳实践：不要在两层做重复的事，统一在 MAF 层处理

        public static async Task RunAsync()
        {
            Console.WriteLine("------------------------------------ middleware --------------------------------------");
            Console.WriteLine();

            var meaiChatClient = (AIClientHelper.GetDefaultOllamaApiChatClient() as IChatClient)
                .AsBuilder()
                .UseFunctionInvocation(configure: options =>  // ChatClient 层配置
                {
                    options.AllowConcurrentInvocation = true; //并发调用
                    options.MaximumIterationsPerRequest = 10; // 迭代控制

                    // 可选，自定义 FunctionInvoker
                    // 不配置 FunctionInvoker，让 MAF 层统一处理
                    options.FunctionInvoker = async (context, ct) =>
                    {
                        Console.WriteLine($"📝 MEAI: 调用 {context.Function.Name}");
                        return await context.Function.InvokeAsync(context.Arguments, ct);
                    };
                })
                //.Use(getResponseFunc: async (messages, options, innerClient, ct) =>   // 使用 MEAI 的 DelegatingChatClient 或 Use() 方法
                //{
                //    Console.WriteLine($"📊 [ChatClient] 请求消息数: {messages.Count()}");
                //    var response = await innerClient.GetResponseAsync(messages, options, ct);
                //    Console.WriteLine($"📊 [ChatClient] Token: {response.Usage?.TotalTokenCount}");
                //    return response;
                //})
                .Build();

            // 1. 获取底层Ollama ChatClient
            var chatClient = AIClientHelper.GetDefaultOllamaApiChatClient();

            var startTime = DateTime.UtcNow;
            double GetTimestamp() => (DateTime.UtcNow - startTime).TotalMicroseconds;

            var agent = chatClient.AsAIAgent(instructions: "你是智能助手")
                .AsBuilder()
                .Use(async (messages, session, options, innerAgent, ct) =>
                {
                    Console.WriteLine($"[Agent Run] Pre-Run 检查");
                    Console.WriteLine($"[T+{GetTimestamp():F0}ms]  Agent Run Pre");
                    var response = await innerAgent.RunAsync(messages, session, options, ct);
                    Console.WriteLine($"[T+{GetTimestamp():F0}ms]  Agent Run Post");
                    Console.WriteLine($"[Agent Run] Post-Run 完成");
                    return response;
                }, null)
                .Use(async (agent, context, next, ct) =>   // Agent 层配置
                {
                    // 拦截工具函数的执行
                    Console.WriteLine($"🔧 [Function] 调用: {context.Function.Name}");
                    var result = await next(context, ct);
                    Console.WriteLine($"🔧 [Function] 结果: {result}");

                    //ValidatePermission(context.Function.Name);  // 权限检查
                    //LogFunctionCall(context);                   // 审计日志

                    return result;
                })
                .Build();

            Console.WriteLine(await agent.RunAsync("你是谁？"));

            Console.WriteLine();
            Console.WriteLine("------------------------------------ 回复完毕 ---------------------------------------------------");
            Console.WriteLine();

            // 企业级 注册顺序示例
            //var agent = chatClient.CreateAIAgent(...)
            //        .AsBuilder()
            //        .Use(LoggingMiddleware)    // 最先注册 → 最外层
            //        .Use(SecurityMiddleware)   // 中间注册 → 中间层
            //        .Use(CachingMiddleware)    // 最后注册 → 最内层
            //        .Build();
        }
    }
}