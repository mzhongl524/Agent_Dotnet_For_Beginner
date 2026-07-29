using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text;

namespace Mzl.AgentDotnetForBeginner
{
    //✅ 多维度配置：会员等级、语言风格、响应长度、专业度
    //✅ 动态学习：根据用户行为自动调整偏好
    //✅ 职责分离：数据模型 + Provider 逻辑
    //✅ 企业应用：智能客服、学习助手、医疗问诊

    public sealed class PersonalizationProvider : AIContextProvider
    {
        private readonly IChatClient _chatClient;
        private readonly ProviderSessionState<UserInfo> _sessionState;

        public PersonalizationProvider(IChatClient chatClient, Func<AgentSession?, UserInfo>? stateInitializer = null)
        {
            _chatClient = chatClient;
            _sessionState = new ProviderSessionState<UserInfo>(stateInitializer ?? (_ => new UserInfo()), this.GetType().Name);
        }

        public UserProfile UserProfile { get; set; }

        public override IReadOnlyList<string> StateKeys => [this._sessionState.StateKey];

        // 调用前：注入个性化 Instructions
        protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = default)
        {
            var instructions = BuildPersonalizedInstructions();
            return new ValueTask<AIContext>(new AIContext { Instructions = instructions });
        }

        protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
        {
            UserProfile.InteractionCount++;

            var userMessage = context.RequestMessages.Last().Text ?? "";

            // 1️⃣ 分析消息长度，判断简洁偏好
            if (userMessage.Length < 50)
            {
                UserProfile.CommunicationStyle = CommunicationStyle.Concise;
            }

            // 2️⃣ 检测用户反馈，调整详细程度
            if (userMessage.Contains("太详细") || userMessage.Contains("简短"))
            {
                UserProfile.ResponseDetailLevel = DetailLevel.Brief;
            }

            // 3️⃣ 根据交互次数升级会员等级
            if (UserProfile.InteractionCount >= 15 && UserProfile.MemberLevel == MemberLevel.Regular)
            {
                UserProfile.MemberLevel = MemberLevel.VIP;
            }

            // 4️⃣ 检测技术术语，标记技术用户
            if (userMessage.Contains("api") || userMessage.Contains("代码"))
            {
                UserProfile.IsTechnicalUser = true;
            }

            return base.StoreAIContextAsync(context, cancellationToken);
        }

        private string BuildPersonalizedInstructions()
        {
            var sb = new StringBuilder();

            // 会员等级配置
            switch (UserProfile.MemberLevel)
            {
                case MemberLevel.VIP:
                    sb.AppendLine($"User is VIP. Use respectful title: '尊敬的{UserProfile.Name}'");
                    sb.AppendLine("Prioritize their requests and provide premium service");
                    break;

                case MemberLevel.New:
                    sb.AppendLine("User is new. Provide warm welcome and detailed guidance");
                    break;
            }

            // 语言风格配置
            switch (UserProfile.CommunicationStyle)
            {
                case CommunicationStyle.Concise:
                    sb.AppendLine("Keep responses brief. Maximum 2-3 sentences.");
                    break;

                case CommunicationStyle.Formal:
                    sb.AppendLine("Use formal and professional language. Avoid emojis.");
                    break;
            }

            return sb.ToString();
        }
    }
}