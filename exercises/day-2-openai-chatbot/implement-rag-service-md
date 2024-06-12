# Hackathon Exercise: Implementing RAG Retrieval with Magic: The Gathering Card Search

## Objective

In this exercise, you will extend your existing .NET 8 console application to implement Retrieval-Augmented Generation (RAG) by integrating a Magic: The Gathering (MTG) card search function using the Open MTG API. You will enhance the chatbot to determine if a user question is related to MTG, fetch relevant card information, and incorporate this data into the chat context.

## Prerequisites

- Existing .NET 8 console application with basic chatbot functionality using Azure OpenAI SDK.
- API Key, Resource Name, and Endpoint from your Azure OpenAI resource.

## Steps to Complete the Exercise

### Step 1: Determine if the User's question is related to Magic: The Gathering

Use the following function to ask the AI to determine if the user's question is related to the game, and if so, return a search query.
```csharp
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
```

### Step 2: Change your chat loop to check if the user input is related and needs to query the service.

### Step 3: Call the MtgAPI to get the data

Insert the following function into your code and use it to query the MTG Database to find any related cards from the AI search query.
```csharp
static async Task<string> GetMTGCardInfo(string query)
{
    using var httpClient = new HttpClient();
    string requestUri = $"https://api.magicthegathering.io/v1/cards?name={Uri.EscapeDataString(query)}";
    var response = await httpClient.GetStringAsync(requestUri);
    return response;
}
```

### Step 4: Call your AI with the results of the query and ask the user's original question.
You'll want to call your AI with a modified prompt again, and add the results of the MTG API to your Chat History at the last position in the chat history in order to give it the greatest weight to the AI context.

### Challenges:

* See if you can get the AI to write the RequestUri for more advanced queries.
* Change the AI query prompt to see if you can refine it's ability to determine if the question is properly related or not.
* Change the RAG implementation to an Assistant Tool Query - For assistance with how to do this, check out the [Assistants documentation](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.openai.assistants-readme?view=azure-dotnet-preview)


### Sample Questions related to MTG:
* If I use Dar-Dweller Oracle's ability, do I need to pay the casting cost for the card that I exile?
* What type of card is a Ruby Mox?
* What sets were Royal Assassin printed in?