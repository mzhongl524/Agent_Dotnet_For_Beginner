using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Mzl.AgentDotnetForBeginner
{
    // ✅ FAQ Agent：成本降低 100%，响应提升 60 倍
    // ✅ 审批 Agent：多轮对话 + 规则引擎 + 状态持久化
    // ✅ 混合 Agent：智能路由 + 降级策略，年节省 16 万

    internal class CustomAgent19
    {
    }

    public class FaqAgent : AIAgent
    {
        private readonly Dictionary<string, string> _faqDatabase = new()
        {
            ["营业时间"] = "周一至周五 9:00-18:00",
            ["退货"] = "请登录账户 → 订单详情 → 申请退货",
            ["配送"] = "同城 24 小时，省内 2-3 天",
        };

        public override string? Name => "FaqAgent";

        protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement serializedState, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
        {
            var userText = messages.LastOrDefault()?.Text ?? "";

            // 关键词匹配
            var answer = _faqDatabase.FirstOrDefault(
                kvp => userText.Contains(kvp.Key)).Value;

            var responseText = answer ?? "抱歉，未找到相关 FAQ";

            ChatMessage responseMessage = new ChatMessage(ChatRole.Assistant, responseText)
            {
                AuthorName = this.Name,
                MessageId = Guid.NewGuid().ToString("N")
            };

            session?.SetInMemoryChatHistory(messages.Concat([responseMessage]).ToList());

            return new AgentResponse { Messages = [responseMessage] };
        }

        protected override IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<JsonElement> SerializeSessionCoreAsync(AgentSession session, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class ApprovalAgent : AIAgent
    {
        private readonly List<ApprovalRule> _rules = new()
        {
            new() { Type = "请假", MaxDays = 3, Result = "自动通过" },
            new() { Type = "请假", MaxDays = 7, Result = "需要主管审批" },
            new() { Type = "报销", MaxAmount = 1000, Result = "自动通过" },
        };

        protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement serializedState, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<JsonElement> SerializeSessionCoreAsync(AgentSession session, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        //private string ProcessConversation(ApprovalAgentThread thread, string userInput)
        //{
        //    var state = thread.State;

        //    // 步骤 1: 识别审批类型
        //    if (!state.HasType) { /* 询问类型 */ }

        //    // 步骤 2: 收集金额或天数
        //    if (!state.HasAmount) { /* 询问数值 */ }

        //    // 步骤 3: 执行规则匹配
        //    var rule = MatchRule(state.Type, state.Amount);
        //    return $"审批结果: {rule.Result}";
        //}
    }

    internal class ApprovalRule
    {
        public string Type { get; set; }

        public int MaxDays { get; set; }
        public long MaxAmount { get; set; }
        public string Result { get; set; }
    }

    public class HybridAgent : AIAgent
    {
        //private readonly FaqAgent _faqAgent = new();
        //private readonly DataQueryAgent _dataQueryAgent = new();
        //private readonly IChatClient _aiClient;

        //public override async Task<AgentRunResponse> RunAsync(...)
        //{
        //    var intent = ClassifyIntent(userText);  // 意图识别

        //    if (intent == "faq")
        //    {
        //        var response = await _faqAgent.RunAsync(userText);
        //        if (IsSuccessful(response)) return response;
        //    }

        //    if (intent == "data")
        //    {
        //        var response = await _dataQueryAgent.RunAsync(userText);
        //        if (IsSuccessful(response)) return response;
        //    }

        //    // 降级到 AI
        //    return await CallAI(userText);
        //}

        protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement serializedState, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentSession? session = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<JsonElement> SerializeSessionCoreAsync(AgentSession session, JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}