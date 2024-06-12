# Hackathon Exercise: Adding Chatbot Functionality to Your Azure OpenAI Console Application

## Objective

In this exercise, you will extend your existing .NET 8 console application to implement a simple chatbot using the Azure OpenAI SDK. This exercise assumes you have already created an Azure OpenAI Deployment, have your API key, resource name, and endpoint, and have connected to the OpenAI endpoint.

## Prerequisites

- Existing .NET 8 console application connected to Azure OpenAI Service.
- API Key, Resource Name, and Endpoint from your Azure OpenAI resource.

## Steps to Complete the Exercise

### Step 1: Install the Azure.AI.OpenAI NuGet Package

1. **Ensure you have the Azure OpenAI SDK installed:**
   ```bash
   dotnet add package Azure.AI.OpenAI --version 1.0.0
   ```

### Step 2: Implement the Chatbot

1. **Open `Program.cs` and Change the `ChatCompletionOptions`:**

```charp
var completionOptions = new ChatCompletionsOptions
{
    Messages = { new ChatRequestSystemMessage("You are a helpful assistant. Answer the user's questions to the best of your abilities.") },
    MaxTokens = 1000,
    Temperature = 1f,
    FrequencyPenalty = 0.0f,
    PresencePenalty = 0.0f,
    NucleusSamplingFactor = 0.95f, // Top P
    DeploymentName = openAIDeploymentName
};
```

2. **replace lines 29 to the end with:**

   ```csharp    
    Console.WriteLine("Azure OpenAI Chatbot is running. Type 'exit' to end the chat.");

    while (true)
    {
        Console.Write("You: ");
        var userInput = Console.ReadLine();
        if (userInput == null)
            continue;
        if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            break;
        completionOptions.Messages.Add(new ChatRequestUserMessage(userInput));
        var completionsResponse = await openAIClient.GetChatCompletionsAsync(completionOptions);
        var responseText = completionsResponse.Value.Choices[0].Message;
        completionOptions.Messages.Add(new ChatRequestAssistantMessage(responseText.Content));
        Console.WriteLine($"Bot: {responseText.Content}");
    }
   ```

### Step 2: Run Your Chatbot

1. **Build and run the application:**
   ```bash
   dotnet run
   ```

2. **Interact with your chatbot by typing messages and observing the responses. Type 'exit' to end the chat.**

## Goals

By the end of this exercise, you should have:

- Added chatbot functionality to your .NET 8 console application.
- Implemented interaction with the Azure OpenAI service to generate responses.
- Successfully run and tested your chatbot application.

## Next Steps

- Experiment with different `CompletionsOptions` settings to see how they affect the responses.
- Extend the application to handle more complex interactions or integrate it into a larger system.

Feel free to ask for help or clarification if you encounter any issues. Happy coding!
