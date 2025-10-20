using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using ModelContextProtocol.Client;

namespace DataFactory.MCP.EvaluationTests
{
    public abstract class EvalTestBase
    {
        /// The below <see cref="ChatConfiguration"/> identifies the LLM endpoint that should be used for all evaluations
        /// performed in the current sample project. <see cref="s_chatConfiguration"/> is initialized with the value
        /// returned from <see cref="TestSetup.GetChatConfiguration"/> inside <see cref="InitializeAsync(TestContext)"/>
        /// below.
        protected static ChatConfiguration? s_chatConfiguration;

        /// All unit tests in the current sample project evaluate the LLM's response to the following question: "How far is
        /// the planet Venus from the Earth at its closest and furthest points?".
        /// 
        /// We invoke the LLM once inside <see cref="InitializeAsync(TestContext)"/> below to get a response to this
        /// question and store this response in a static variable <see cref="s_response"/>. Each unit test in the current
        /// project then performs a different evaluation on the same stored response.

        protected static readonly IList<ChatMessage> s_messages = [
            new ChatMessage(
            ChatRole.System,
            ""),
        new ChatMessage(
            ChatRole.User,
            "")];

        protected static ChatResponse s_response = new();
        protected static async Task InitializeAsync(TestContext _)
        {
            /// Set up the <see cref="ChatConfiguration"/> which includes the <see cref="IChatClient"/> that all the
            /// evaluators used in the current sample project will use to communicate with the LLM.
            s_chatConfiguration = TestSetup.GetChatConfiguration();
            StdioClientTransport mcpClientTransport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "Everything",
                Command = "dotnet",
                Arguments = ["run", "--project", "./DataFactory.MCP/DataFactory.MCP.csproj"],
            });

            var client = await McpClient.CreateAsync(mcpClientTransport);
            IList<McpClientTool> tools = await client.ListToolsAsync();
            var chatOptions =
                new ChatOptions
                {
                    Tools = [.. tools],
                    Temperature = 0.0f,
                    ResponseFormat = ChatResponseFormat.Text
                };
            /// Fetch the response to be evaluated and store it in a static variable <see cref="s_response" />.
            s_response = await s_chatConfiguration.ChatClient.GetResponseAsync(s_messages, chatOptions);
        }
    }
}
