using System.Net.Http.Json;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;

// == Retrieve the local secrets saved during the Azure deployment ==========
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
string openAIEndpoint = config["AZURE_OPENAI_ENDPOINT"];
string openAIDeploymentName = config["AZURE_OPENAI_GPT_NAME"];
string openAiKey = config["AZURE_OPENAI_KEY"];
// == If you skipped the deployment because you already have an Azure OpenAI available,
// == edit the previous lines to use hardcoded values.
// == ex: string openAIEndpoint = "https://cog-demo123.openai.azure.com/";

// == Creating the AIClient ==========
var endpoint = new Uri(openAIEndpoint);
var credentials = new AzureKeyCredential(openAiKey);
var openAIClient = new OpenAIClient(endpoint, credentials);

var completionOptions = new ChatCompletionsOptions
{
    Messages = { new ChatRequestSystemMessage("You are a helpful assistant. Answer the user's questions to the best of your abilities. You will be passed information about Magic: The Gathering cards in json fomat to assist in answering any related questions.") },
    MaxTokens = 1000,
    Temperature = 1f,
    FrequencyPenalty = 0.0f,
    PresencePenalty = 0.0f,
    NucleusSamplingFactor = 0.95f, // Top P
    DeploymentName = openAIDeploymentName
};

Console.WriteLine("Azure OpenAI Chatbot is running. Type 'exit' to end the chat.");

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (userInput == null)
        continue;
    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    var mtgQuery = await GetMtgQueryIfRelated(openAIClient, userInput, openAIDeploymentName);
    if (mtgQuery != string.Empty) {
        var mtgCardData = await GetMTGCardInfo(mtgQuery);
        completionOptions.Messages.Add(new ChatRequestUserMessage($"---\n[Magic: The Gathering Card Data]:\n{mtgCardData}\n---"));
    }
    completionOptions.Messages.Add(new ChatRequestUserMessage(userInput));
    var completionsResponse = await openAIClient.GetChatCompletionsAsync(completionOptions);
    var responseText = completionsResponse.Value.Choices[0].Message;
    completionOptions.Messages.Add(new ChatRequestAssistantMessage(responseText.Content));
    Console.WriteLine($"Bot: {responseText.Content}");
}

return;


static async Task<string> GetMtgQueryIfRelated(OpenAIClient client, string question, string deploymentName)
{
    var completionOptions = new ChatCompletionsOptions
    {
        Messages = { new ChatRequestSystemMessage($"Is the following question related to Magic: The Gathering? If it is, return a search query to find the card the user is looking for, if it is not, return NO.\n\n{question}") },
        MaxTokens = 15,
        Temperature = 0.0f,
        DeploymentName = deploymentName
    };
    
    var completionsResponse = await client.GetChatCompletionsAsync(completionOptions);
    var responseText = completionsResponse.Value.Choices[0].Message.Content.Trim().ToLower();

    return responseText.ToUpper() == "NO" ? string.Empty : responseText;
}

static async Task<string> GetMTGCardInfo(string query)
{
    using var httpClient = new HttpClient();
    string requestUri = $"https://api.magicthegathering.io/v1/cards?name={Uri.EscapeDataString(query)}";
    var response = await httpClient.GetStringAsync(requestUri);
    return response;
}