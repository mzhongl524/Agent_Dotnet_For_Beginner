namespace Mzl.AgentDotnetForBeginner
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await FisrtAgent1.RunAsync();

            Console.ReadKey();
        }
    }
}