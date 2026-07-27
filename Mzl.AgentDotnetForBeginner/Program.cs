namespace Mzl.AgentDotnetForBeginner
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //await FisrtAgent1.RunAsync();

            //await AgentSession2.RunAsync();

            //await SessionMemoryStore3.RunAsync();

            //await SessionSaveAndRecovery4.RunAsync();

            //await SessionReduce5.RunAsync();

            await ToolCalling6.RunAsync();

            Console.ReadKey();
        }
    }
}