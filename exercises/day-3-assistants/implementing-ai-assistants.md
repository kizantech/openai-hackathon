# Hackathon Exercise: Enhancing the Console Chatbot with Azure OpenAI Assistants SDK

## Objective

In this exercise, you will enhance your existing .NET 8 console application to fetch Magic: The Gathering (MTG) card information using the Azure OpenAI Assistants SDK. The application will determine if a user's question is related to MTG, retrieve relevant card information, and incorporate this data into the chat context.

## Prerequisites

- Existing .NET 8 console application with basic chatbot functionality using Azure OpenAI SDK.
- API Key, Resource Name, and Endpoint from your Azure OpenAI resource.

## Steps to Complete the Exercise

### Step 1: Install the Required NuGet Packages

1. **Ensure you have the following NuGet packages installed:**
   ```bash
   dotnet add package Azure.AI.OpenAI.Assistants --prerelease
   ```

2. ** At the top of your `Program.cs` file, add the following using statements:
    ```csharp
    using System.Text.Json;
    using Azure.AI.OpenAI.Assistants;
    ```

### Step 2: Implement the MTG Card Information Fetch

1. **In `Program.cs`, on line 21, we need to create an AssistantsClient:**

   ```csharp
    var aiAssistantsClient = new AssistantsClient(endpoint, credentials);
   ```

   

2. **Next up, on line 35, after the `Console.WriteLine` message to the user, we
need to add our FunctionToolDefinition.**
    ```csharp
    // create a function tool definition
    FunctionToolDefinition searchMtgDatabaseByCardNameTool = new(
        name: "searchMtgDatabaseByCardName",
        description: "Searches the Magic: The Gathering Card database by card name, and returns details about any matching cards.",
        parameters: BinaryData.FromObjectAsJson(
            new
            {
                Type = "object",
                Properties = new
                {
                    Name = new
                    {
                        Type = "string",
                        Description = "The name of the card to search for. Partial card names are fine."
                    }
                },
                Required = new[] { "name" },
            },
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    ```
    This block of code instructs the AI how our tool works, when to call it, and what parameters the tool requires to function.

3. ** Now before the `while(true)` loop, add the following code to create our AI Assistant and thread:**
   ```csharp
   // Create the AI Assistant
    var assistantResponse = await aiAssistantsClient.CreateAssistantAsync(
        new AssistantCreationOptions(openAIDeploymentName)
        {
            Name = "MTG Card Bot",
            Instructions = "You are a Magic: The Gathering card expert bot. Use the provided functions to search the database of Magic: The Gathering cards. And answer any questions the user has to the best of your abilities.",
            Tools = { searchMtgDatabaseByCardNameTool }
        });

    var assistant = assistantResponse.Value;

    // Create an AI Assistant Thread. All questions should run through this now instead of chat.
    var threadCreationResponse = await aiAssistantsClient.CreateThreadAsync();
    var thread = threadCreationResponse.Value;
    ```
    We are replacing the old Chatbot Client with the new AI Assistant Client. This client allows us to use tools in our responses in addition to the trained knowledge the AI already has. This approach allows us to accomplish a similar setup as the previous RAG exercise, but instead of always asking the AI if we need to resolve the query, the AI will tell us when the query needs additional data from our tooling.

### Step 3: Change the user input loop:

#### Starting the Loop and Getting User Input

```csharp
while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (userInput == null)
        continue;
    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;
}
```

Clear out the existing loo implementation and replace it with just this to begin with. We're going to make some major modifications. The following steps will all add code to the end of the current code inside the loop.

#### Creating a Message on the Thread

```csharp
// Create a message on the thread
var messageResponse = await aiAssistantsClient.CreateMessageAsync(
    thread.Id,
    MessageRole.User,
    userInput);

var message = messageResponse.Value;
```
**Explanation:**
- `var messageResponse = await aiAssistantsClient.CreateMessageAsync(thread.Id, MessageRole.User, userInput);` sends the user's message to the Azure OpenAI Assistants API, associating it with the current conversation thread.
- `var message = messageResponse.Value;` extracts the created message from the response.

#### Running and Evaluating the Thread with the Assistant

```csharp
// Run and evaluate the thread with the assistant
var runResponse = await aiAssistantsClient.CreateRunAsync(
    thread.Id,
    new CreateRunOptions(assistant.Id)
    {
        AdditionalInstructions =
            "Use the AI Assistants in this thread to answer any questions the user has to the best of your abilities."
    });

var run = runResponse.Value;
```
**Explanation:**
- `var runResponse = await aiAssistantsClient.CreateRunAsync(thread.Id, new CreateRunOptions(assistant.Id) { ... });` initiates a new "run" of the assistant to process the messages in the thread. It passes additional instructions to guide the assistant's responses.
- `var run = runResponse.Value;` extracts the run information from the response.

#### Polling the Run Until It Finishes

```csharp
// Poll the run until it finishes.
do
{
    await Task.Delay(TimeSpan.FromMilliseconds(500));
    runResponse = await aiAssistantsClient.GetRunAsync(thread.Id, runResponse.Value.Id);

    if (runResponse.Value.Status == RunStatus.RequiresAction
        && runResponse.Value.RequiredAction is SubmitToolOutputsAction submitToolOutputsAction)
    {
        List<ToolOutput> toolOutputs = new();

        foreach (var toolCall in submitToolOutputsAction.ToolCalls)
        {
            toolOutputs.Add(await GetResolvedToolOutput(toolCall));
        }

        runResponse = await aiAssistantsClient.SubmitToolOutputsToRunAsync(runResponse.Value, toolOutputs);
    }
} while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);
```
**Explanation:**
- The loop continues to check the status of the run until it completes.
- `await Task.Delay(TimeSpan.FromMilliseconds(500));` waits for half a second before polling again.
- `runResponse = await aiAssistantsClient.GetRunAsync(thread.Id, runResponse.Value.Id);` checks the current status of the run.
- If the run status is `RequiresAction` and it involves submitting tool outputs (`SubmitToolOutputsAction`), it gathers the necessary tool outputs and submits them back to the run.
- This continues until the run status is neither `Queued` nor `InProgress`.

#### Listing Messages and Showing Results

```csharp
// List messages and show results
var afterRunMessagesResponse = await aiAssistantsClient.GetMessagesAsync(thread.Id);
var messages = afterRunMessagesResponse.Value;

foreach (var threadMessage in messages)
{
    Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
    foreach (var contentItem in threadMessage.ContentItems)
    {
        if (contentItem is MessageTextContent textItem)
        {
            Console.Write(textItem.Text);
        }
        else if (contentItem is MessageImageFileContent imageFileContent)
        {
            Console.Write($"<image from ID: {imageFileContent.FileId}");
        }
        Console.WriteLine();
    }
}
```
**Explanation:**
- `var afterRunMessagesResponse = await aiAssistantsClient.GetMessagesAsync(thread.Id);` retrieves all messages from the thread after the run completes.
- `var messages = afterRunMessagesResponse.Value;` extracts the list of messages.
- The `foreach` loop iterates through each message in the thread.
- `Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");` prints the timestamp and role (user or assistant) of each message.
- The inner `foreach` loop iterates through each content item in the message, printing the text content or indicating the presence of an image.

### Complete Code for the While Loop

Here is the completed code:

```csharp
while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (userInput == null)
        continue;
    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    // Create a message on the thread
    var messageResponse = await aiAssistantsClient.CreateMessageAsync(
        thread.Id,
        MessageRole.User,
        userInput);

    var message = messageResponse.Value;

    // Run and evaluate the thread with the assistant
    var runResponse = await aiAssistantsClient.CreateRunAsync(
        thread.Id,
        new CreateRunOptions(assistant.Id)
        {
            AdditionalInstructions =
                "Use the AI Assistants in this thread to answer any questions the user has to the best of your abilities."
        });

    var run = runResponse.Value;

    // Poll the run until it finishes.
    do
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        runResponse = await aiAssistantsClient.GetRunAsync(thread.Id, runResponse.Value.Id);

        if (runResponse.Value.Status == RunStatus.RequiresAction
            && runResponse.Value.RequiredAction is SubmitToolOutputsAction submitToolOutputsAction)
        {
            List<ToolOutput> toolOutputs = new();

            foreach (var toolCall in submitToolOutputsAction.ToolCalls)
            {
                toolOutputs.Add(await GetResolvedToolOutput(toolCall));
            }

            runResponse = await aiAssistantsClient.SubmitToolOutputsToRunAsync(runResponse.Value, toolOutputs);
        }
    } while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

    // List messages and show results
    var afterRunMessagesResponse = await aiAssistantsClient.GetMessagesAsync(thread.Id);
    var messages = afterRunMessagesResponse.Value;

    foreach (var threadMessage in messages)
    {
        Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
        foreach (var contentItem in threadMessage.ContentItems)
        {
            if (contentItem is MessageTextContent textItem)
            {
                Console.Write(textItem.Text);
            }
            else if (contentItem is MessageImageFileContent imageFileContent)
            {
                Console.Write($"<image from ID: {imageFileContent.FileId}");
            }
            Console.WriteLine();
        }
    }
}
```
### Step 4: Adding Tool Resolution logic
1. **Add the Following Code after the `while(true)` loop:**
   ```csharp
    async Task<ToolOutput> GetResolvedToolOutput(RequiredToolCall toolCall)
    {
        if (toolCall is RequiredFunctionToolCall functionToolCall)
        {
            using JsonDocument argumentsJson = JsonDocument.Parse(functionToolCall.Arguments);
            if (functionToolCall.Name == searchMtgDatabaseByCardNameTool.Name)
            {
                string nameArugment = argumentsJson.RootElement.GetProperty("name").GetString();
                return new ToolOutput(toolCall, await GetMTGCardInfo(nameArugment));
            }
        }

        return null;
    }
   ```
   This code will resolve your ToolOutput calls and call the code you need when the AI requests it.

### Step 5: Testing your new Assistant
1. **Interact with your chatbot by typing various questions. If you ask about a Magic: The Gathering card, the chatbot should fetch relevant information and incorporate it into the response. Type 'exit' to end the chat. Try some of the queries from the previous exercise and see if the AI responds better or worse.**

## Goals

By the end of this exercise, you should have:

- Implemented logic to determine if a user question is related to Magic: The Gathering.
- Integrated a Magic: The Gathering card search function to retrieve relevant information.
- Enhanced your chatbot to incorporate fetched data into the chat context for more relevant responses.
- Successfully run and tested your enhanced chatbot application.

## Next Steps

- Experiment with different queries and observe how the chatbot's responses change based on the retrieved information.
- Add additional capabilities for the AI to handle card searches by other APIs, such as set, format, and types: https://docs.magicthegathering.io
- Extend the application to handle more complex queries or integrate additional data sources.

Feel free to ask for help or clarification if you encounter any issues. Happy coding!
