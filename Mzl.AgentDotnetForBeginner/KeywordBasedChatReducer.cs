using Microsoft.Extensions.AI;

namespace Mzl.AgentDotnetForBeginner
{
    // 适用于客服、医疗等需要追踪关键信息的场景

    public class KeywordBasedChatReducer : IChatReducer
    {
        private readonly string[] _keywords;
        private readonly int _maxMessages;

        public KeywordBasedChatReducer(string[] keywords, int maxMessages = 5)
        {
            _keywords = keywords;
            _maxMessages = maxMessages;
        }

        public Task<IEnumerable<ChatMessage>> ReduceAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
        {
            var messageList = messages.ToList();

            // 保留所有系统消息
            var systemMessages = messageList.Where(m => m.Role == ChatRole.System);

            // 保留包含关键词的消息
            var keywordMessages = messageList.Where(m => m.Role != ChatRole.System && _keywords.Any(kw => m.Text?.Contains(kw) == true));

            // 保留最近的消息
            var keywordCount = 0;
            if (keywordMessages != null)
            {
                keywordCount = keywordMessages.Count();
            }
            var recentMessages = messageList.Where(m => m.Role != ChatRole.System && keywordMessages != null && !keywordMessages.Contains(m))
                .TakeLast(_maxMessages - keywordCount);

            // 合并并按原始顺序排序
            var reducedMessages = systemMessages
                .Concat(keywordMessages)
                .Concat(recentMessages)
                .OrderBy(m => messageList.IndexOf(m));

            return Task.FromResult((IEnumerable<ChatMessage>)reducedMessages);
        }
    }
}