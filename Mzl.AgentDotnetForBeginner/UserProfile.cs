namespace Mzl.AgentDotnetForBeginner
{
    public enum MemberLevel
    {
        New,
        Regular,
        VIP
    }

    public enum CommunicationStyle
    {
        Formal,
        Standard,
        Casual,
        Concise
    }

    public enum DetailLevel
    {
        Brief,
        Standard,
        Detailed
    }

    public class UserProfile
    {
        public string? Name { get; set; }
        public MemberLevel MemberLevel { get; set; } = MemberLevel.Regular;
        public CommunicationStyle CommunicationStyle { get; set; } = CommunicationStyle.Standard;
        public DetailLevel ResponseDetailLevel { get; set; } = DetailLevel.Standard;
        public bool IsTechnicalUser { get; set; } = false;
        public int InteractionCount { get; set; } = 0;
    }
}