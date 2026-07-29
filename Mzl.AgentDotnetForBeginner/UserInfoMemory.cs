using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Data;
using System.Text;

namespace Mzl.AgentDotnetForBeginner
{
    //✅ 三个核心方法：ProvideAIContextAsync（注入上下文）、StoreAIContextAsync（提取信息、持久化）
    //✅ 两个构造函数：首次创建 + 反序列化恢复
    //✅ 分离原则：数据状态（序列化）vs 服务依赖（不序列化）
    //✅ 应用场景：用户信息记忆、RAG、个性化配置、统计分析

    public sealed class UserInfoMemory : AIContextProvider
    {
        private readonly IChatClient _chatClient;  // ❌ 不序列化
        private IReadOnlyList<string>? _stateKeys;
        private readonly ProviderSessionState<UserInfo> _sessionState;
        private static UserInfo _userInfo = new UserInfo();

        // 构造函数1：首次创建
        public UserInfoMemory(IChatClient chatClient, Func<AgentSession?, UserInfo>? stateInitializer = null)
        {
            _chatClient = chatClient;
            _sessionState = new ProviderSessionState<UserInfo>(stateInitializer ?? (_ => new UserInfo()), this.GetType().Name);
        }

        public override IReadOnlyList<string> StateKeys => this._stateKeys ??= [this._sessionState.StateKey];

        public UserInfo GetUserInfo(AgentSession session)
            => this._sessionState.GetOrInitializeState(session);

        // 调用前：注入用户信息到上下文
        protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // 调用后：从对话中提取用户信息
        protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = default)
        {
            var userInfo = this._sessionState.GetOrInitializeState(context.Session);
            var instructions = new StringBuilder();

            instructions.AppendLine(userInfo.UserName is null
                ? "询问用户的姓名。"
                : $"用户名是： {userInfo.UserName}.");

            instructions.AppendLine(userInfo.UserAge is null
                ? "询问用户的年龄。"
                : $"用户年龄是： {userInfo.UserAge}.");

            return new ValueTask<AIContext>(new AIContext
            {
                Instructions = instructions.ToString()
            });
        }

        protected override async ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
        {
            var userInfo = this._sessionState.GetOrInitializeState(context.Session);
            if ((userInfo.UserName is null || userInfo.UserAge is null) &&
            context.RequestMessages.Any(x => x.Role == ChatRole.User))
            {
                try
                {
                    var result = await _chatClient.GetResponseAsync<UserInfo>(
                        context.RequestMessages,
                        new ChatOptions
                        {
                            Instructions = "从消息中提取用户姓名和年龄（如有），没有则返回null。"
                        }, cancellationToken: cancellationToken);

                    if (!string.IsNullOrEmpty(result.Result.UserName))
                    {
                        _userInfo.UserName = result.Result.UserName;
                    }
                    if (result.Result.UserAge != null)
                    {
                        _userInfo.UserAge = result.Result.UserAge;
                    }

                    // 仅更新未知信息
                    if (string.IsNullOrEmpty(userInfo.UserName))
                    {
                        userInfo.UserName = _userInfo.UserName;
                    }
                    userInfo.UserAge ??= _userInfo.UserAge;
                }
                catch { /* 提取失败不影响主流程 */ }
            }
            this._sessionState.SaveState(context.Session, userInfo);
        }
    }

    public class UserInfo
    {
        public string UserName { get; set; } = string.Empty;
        public int? UserAge { get; set; }
    }
}