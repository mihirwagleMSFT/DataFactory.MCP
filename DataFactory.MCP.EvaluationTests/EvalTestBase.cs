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

        /// The MCP client used to connect to the Data Factory MCP server
        protected static McpClient? s_mcpClient;

        /// The chat options containing the tools and settings for the chat client
        protected static ChatOptions? s_chatOptions;

        /// The tools available from the MCP server
        protected static IList<McpClientTool>? s_tools;

        /// All unit tests in the current sample project evaluate the LLM's response to Data Factory management queries.
        /// 
        /// We invoke the LLM once inside <see cref="InitializeAsync(TestContext)"/> below to get a response to this
        /// question and store this response in a static variable <see cref="s_response"/>. Each unit test in the current
        /// project then performs a different evaluation on the same stored response.

        protected static readonly IList<ChatMessage> s_messages = [
            new ChatMessage(
            ChatRole.System,
            "You are a helpful Microsoft Data Factory assistant. Use the available tools to help users manage their Data Factory resources including gateways, connections, workspaces, and dataflows."),
        new ChatMessage(
            ChatRole.User,
            "Can you help me understand what Data Factory resources are available in my environment? I'd like to see an overview of my gateways and connections.")];

        protected static ChatResponse s_response = new();

        protected static async Task InitializeTestAsync()
        {
            /// Set up the <see cref="ChatConfiguration"/> which includes the <see cref="IChatClient"/> that all the
            /// evaluators used in the current sample project will use to communicate with the LLM.
            s_chatConfiguration = TestSetup.GetChatConfiguration();

            StdioClientTransport mcpClientTransport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "DataFactory.MCP",
                Command = "dotnet",
                Arguments = ["run", "--project", "..\\..\\..\\..\\DataFactory.MCP\\DataFactory.MCP.csproj"],
            });

            s_mcpClient = await McpClient.CreateAsync(mcpClientTransport);
            s_tools = await s_mcpClient.ListToolsAsync();
            s_chatOptions = new ChatOptions
            {
                Tools = [.. s_tools],
                Temperature = 0.0f,
                ResponseFormat = ChatResponseFormat.Text
            };

            // Get the initial response using the shared messages

            s_response = await s_chatConfiguration.ChatClient.GetResponseAsync(s_messages, s_chatOptions);
        }

        protected static async Task CleanupTestAsync()
        {
            if (s_mcpClient != null)
            {
                await s_mcpClient.DisposeAsync();
                s_mcpClient = null;
            }
        }

        /// <summary>
        /// Generic method to evaluate how well an actual response matches an expected response pattern using LLM-based evaluation.
        /// </summary>
        /// <param name="originalMessages">The original conversation messages</param>
        /// <param name="actualResponse">The actual AI response to evaluate</param>
        /// <param name="expectedResponsePattern">Description of the expected response pattern</param>
        /// <param name="evaluationCriteria">Specific criteria for evaluation (optional, will use default if null)</param>
        /// <param name="scenarioName">Name of the scenario being evaluated (for logging purposes)</param>
        /// <param name="minimumAcceptableScore">Minimum score (1-5) to pass the evaluation (default: 3)</param>
        /// <returns>The evaluation score (1-5) or null if evaluation failed</returns>
        protected static async Task<int?> EvaluateResponseMatchAsync(
            List<ChatMessage> originalMessages,
            ChatResponse actualResponse,
            string expectedResponsePattern,
            string? evaluationCriteria = null,
            string scenarioName = "Response",
            int minimumAcceptableScore = 3)
        {
            var defaultCriteria = """
                1. Accuracy: Did the AI correctly identify the situation and provide appropriate guidance?
                2. Helpfulness: Did the AI provide useful information to address the user's needs?
                3. Tone and Politeness: Is the response professional and offers assistance?
                4. Completeness: Does the response adequately address the user's request?
                5. Technical Correctness: Is any technical information provided accurate?
                """;

            var evaluationMessages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System,
                    $"""
                    You are an expert evaluator comparing AI assistant responses against expected patterns.

                    Evaluate how well the actual response matches the expected behavior pattern based on these criteria:
                    {evaluationCriteria ?? defaultCriteria}

                    Rate the match quality on a scale of 1-5:
                    - 5: Excellent match - meets or exceeds expected behavior
                    - 4: Good match - minor differences but meets expectations  
                    - 3: Acceptable match - adequate handling of the scenario
                    - 2: Poor match - significant gaps in expected behavior
                    - 1: Very poor match - fails to meet basic expectations

                    Respond with just the number (1-5), followed by a brief explanation of the match quality.
                    """),
                new ChatMessage(ChatRole.User,
                    $"""
                    User's Original Request: "{originalMessages.LastOrDefault()?.Text ?? "No request"}"
                    
                    Expected Response Pattern: "{expectedResponsePattern}"
                    
                    Actual AI Response: "{actualResponse}"
                    
                    How well does the actual response match the expected pattern (1-5 scale)?
                    """)
            };


            var evaluationResponse = await s_chatConfiguration!.ChatClient.GetResponseAsync(
                evaluationMessages,
                new ChatOptions { Temperature = 0.1f });

            var evaluationText = evaluationResponse.ToString();

            // Try to extract the numeric score
            if (!string.IsNullOrWhiteSpace(evaluationText) && char.IsDigit(evaluationText.FirstOrDefault()))
            {
                var scoreChar = evaluationText.First();
                if (int.TryParse(scoreChar.ToString(), out var score) && score >= 1 && score <= 5)
                {
                    /// Assert that the response meets minimum expectations
                    Assert.IsGreaterThanOrEqualTo(minimumAcceptableScore, score,
                        $"{scenarioName} should meet basic expectations. Got score: {score}");

                    return score;
                }
            }
            return null;
        }
    }
}
