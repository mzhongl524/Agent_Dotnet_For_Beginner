using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.IO.Pipelines;

namespace Mzl.Agent.Comm
{
    public class McpHelper
    {
        public static async Task<(McpClient, McpServer)> CreateInMemoryClientAndServerAsync(McpServerPrimitiveCollection<McpServerTool> tools)
        {
            Pipe clientToServer = new();
            Pipe serverToClient = new();

            // Stream conventions:
            //   StreamClientTransport(serverInput, serverOutput, ...): serverInput is what the client
            //   WRITES to (server reads it); serverOutput is what the client READS from (server writes it).
            //   StreamServerTransport(input, output, ...): input is what the server READS from; output
            //   is what the server WRITES to.
            Stream clientWriteStream = clientToServer.Writer.AsStream();
            Stream clientReadStream = serverToClient.Reader.AsStream();
            Stream serverReadStream = clientToServer.Reader.AsStream();
            Stream serverWriteStream = serverToClient.Writer.AsStream();

            StreamServerTransport serverTransport = new(
                serverReadStream,
                serverWriteStream,
                "test-server",
                NullLoggerFactory.Instance);

#pragma warning disable MCPEXP001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            McpServerOptions serverOptions = new()
            {
                ServerInfo = new Implementation { Name = "test-server", Version = "1.0.0" },
                ToolCollection = tools,
            };
#pragma warning restore MCPEXP001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            McpServer server = McpServer.Create(
                serverTransport,
                serverOptions,
                NullLoggerFactory.Instance);

            CancellationTokenSource cts = new();
            Task serverLoop = Task.Run(() => server.RunAsync(cts.Token), cts.Token);

            StreamClientTransport clientTransport = new(
                clientWriteStream,
                clientReadStream,
                NullLoggerFactory.Instance);

            McpClient client = await McpClient.CreateAsync(
                clientTransport,
                clientOptions: null,
                NullLoggerFactory.Instance).ConfigureAwait(false);

            return (client, server);
        }
    }
}