# Postman Collection
[<img src="https://run.pstmn.io/button.svg" alt="Run In Postman" style="width: 128px; height: 32px;">](https://app.getpostman.com/run-collection/11076036-7b7c5b8d-22fa-491c-832c-8f54fbf8324f?action=collection%2Ffork&source=rip_markdown&collection-url=entityId%3D11076036-7b7c5b8d-22fa-491c-832c-8f54fbf8324f%26entityType%3Dcollection%26workspaceId%3Db12abc2b-9c33-4641-a917-76fb9b2b3705)

* You can use the button above, or import the collection beside this readme file to load the Postman Collection into the Postman desktop or web apps.
* Launch the Postman Collection and Go to the Environments Icon on the left
* From the environments page, select the Production Environment
* In the "Current Value" column, fill out each of the settings listed
  * If you are using OAuth, you'll need to change the collection Authorization to OAuth and setup Postman to use [OAuth 2.0.](#oauth-20). Use of Service principals is out of the scope of this hackathon as it requires a global Entra ID admin to setup and configure an Azure Application entry with the proper permissions, an a client secret that you will need to use. Documentation on Postman's OAuth2.0 site is provided below which can help.



# OAuth 2.0

## Configure your AAD application
Add a redirect URI to your AAD application for Postman to receive the authorization code. In the AAD Portal, navigate to your app registration, then to the "Authentication" tab, then add https://oauth.pstmn.io/v1/callback as a new redirect URI. This URI was provided by Postman when you check the ✅ Authorize using browser box (below) and is also documented on their website.

## Configure Postman
You can set this up under "Authorization" for a single request, a folder of requests, or a Collection. Open the "Authorization" tab and select OAuth 2.0. Under the "Configure New Token" section, enter the following information:

* `Token Name`: anything you want. I chose the name of the AAD application.
* `Grant Type`: Authorization Code (With PKCE)
* `Callback URL`: check the ✅ Authorize using browser box
* `Auth URL`: copy this from the AAD portal under Overview > Endpoints (on top bar) > OAuth 2.0 authorization endpoint (v2). Mine looks like https://login.microsoftonline.com/<tenant>/oauth2/v2.0/authorize
* `Access token URL`: copy this from the AAD portal under Overview > Endpoints (on top bar) > OAuth 2.0 token endpoint (v2). Mine looks like https://login.microsoftonline.com/<tenant>/oauth2/v2.0/token
* `Client ID`: copy this from the AAD portal under Overview. It's also commonly referred to as an "Application ID" and is a GUID.
* `Client Secret`: leave blank (not used by this grant type)
* `Code Challenge Method`: SHA-256
* `Code Verifier`: leave blank or provide your own
* `Scope`: the scopes you're requesting in your token, separated by spaces
* `State`: leave blank or provide a value such as a random GUID.
* `Client Authentication`: Send as Basic Auth Header (not used by this grant type)
Refer to Postman's documentation on [OAuth 2.0](https://learning.postman.com/docs/sending-requests/authorization/#requesting-an-oauth-20-token) options for more information.

### Get a token
Click Get New Access Token to open the auth flow in your machine's default web browser. After authentication, it should redirect back to the Postman application and a new token will be created with the name you provided earlier for "Token Name." Select "Use Token" in the top-right corner of the popup to copy it into your current token. If the token expires and your requests start failing authorization (probably a 401 or 403 error), you can revisit this tab and get a new access token.

Occasionally, you may want to visit Current Token > Access Token > Manage Tokens > Delete > Expired tokens, otherwise you'll be inundated with nearly-identical tokens.

# Exercises

1. Update your System Prompt to get OpenAI to respond with a funny accent. Try getting it to talk like a pirate, or use a bunch of emoji like a teenager texting.
2. Take the response from the AI, add it to your request, and then ask a follow up question.
3. Use the [OpenAPI reference](https://platform.openai.com/docs/api-reference/chat) docs to insert an image into your chat request, and ask OpenAI to describe the image.
4. Set the `max_tokens` in the API and see how that changes the AI behavior
5. Provide context to the AI by injecting a message into the message stack. See how it influences the AI's behavior. Try using JSON or other data from an open api, and see if you can get the AI to respond about the data contained.
   1. Example, ask OpenAI, `When is the next MCU Film?`
   2. Add a `user` message to your message collection with the response data from: https://www.whenisthenextmcufilm.com/api
   3. Send the same question to OpenAI and see how the response changes
6. Add `temperature` (between 0 and 2), `top_p` (between 0 and 1), `frequency_penalty` and `presence_penalty` (between -2 and 2) to the call, change the values, see how they affect the response of the AI.
7. Try and get the AI to answer a difficult Logic Puzzle, for exmaple:
   1. `There are three on/off switches on the ground floor of a building. Only one operates a single lightbulb on the third floor. The other two switches are not connected to anything. Put the switches in any on/off order you like. Then go to the third floor to check the bulb. Without leaving the third floor, can you figure out which switch is genuine? You get only one try.`
   2. `Jack has 3 sisters, each of Jack's sisters has two brothers, how many brothers does Jack have?`
   3. Change the `temperature` value and see how it affects your results. If you can, try older models like GPT-3.5 and see how it differs as well.

# Creating your first Azure OpenAI Chatbot
* Launch your preferred terminal window
* Navigate to where you want to store your code
* Create a directory for your project and change to it
* Type `dotnet new console`
* Open your new project in VS 2019+ or VS Code
* Add the `Azure.AI.OpenAI` 1.0.0-beta.13 nuget package to the csproj
* Add the `Microsoft.Extensions.Configuration` and `Microsoft.Extensions.Configuration.UserSecrets` version 8.0.0 nuget package to the csproj

In your Program CS, we need to add some imports.
```csharp

using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
```

Then after that, we need to setup the OpenAI Instance we are going to use. First, let's add the code:
```csharp
// == Retrieve the local secrets saved during the Azure deployment ==========
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
string openAIEndpoint = config["AZURE_OPENAI_ENDPOINT"];
string openAIDeploymentName = config["AZURE_OPENAI_GPT_NAME"];
string openAiKey = config["AZURE_OPENAI_KEY"];
// == If you skipped the deployment because you already have an Azure OpenAI available,
// == edit the previous lines to use hardcoded values.
// == ex: string openAIEndpoint = "https://cog-demo123.openai.azure.com/";
```

Now, in the project, navigate to your user secrets in VS, and edit your config. Add the 3 keys above to the empty JSON file, and set them to the values you should have for your OpenAI environment.

Next up, we're going to configure our OpenAI Client
```csharp
// == Creating the AIClient ==========
var endpoint = new Uri(openAIEndpoint);
var credentials = new AzureKeyCredential(openAiKey);
var openAIClient = new OpenAIClient(endpoint, credentials);

var completionOptions = new ChatCompletionsOptions
{
    MaxTokens = 1000,
    Temperature = 1f,
    FrequencyPenalty = 0.0f,
    PresencePenalty = 0.0f,
    NucleusSamplingFactor = 0.95f, // Top P
    DeploymentName = openAIDeploymentName
};
```

Here we're creating an endpoint, and AzureKeyCredentials to communicate with the OpenAIClient SDK. Then we configure our ChatCompletionOptions with the values we want for the AI controls.

Now we're going to load some data from a markdown file, and setup the system prompt to include it for grounding the AI:
```csharp
//== Read markdown file  ==========
string markdown = System.IO.File.ReadAllText("hikes.md");
// == Providing context for the AI model ==========
var systemPrompt =
"""
You are upbeat and friendly. You introduce yourself when first saying hello. 
Provide a short answer only based on the user hiking records below:  

""" + markdown;
completionOptions.Messages.Add(new ChatRequestSystemMessage(systemPrompt));

Console.WriteLine($"\n\n\t\t-=-=- Hiking History -=-=--\n{markdown}");
```

Now finally we can greet our user and start creating turns between the user and AI and having our conversation!
```csharp
// == Starting the conversation ==========
string userGreeting = """
Hi!
""";

completionOptions.Messages.Add(new ChatRequestUserMessage(userGreeting));
Console.WriteLine($"\n\nUser >>> {userGreeting}");

ChatCompletions response = await openAIClient.GetChatCompletionsAsync(completionOptions);
ChatResponseMessage assistantResponse = response.Choices[0].Message;
Console.WriteLine($"\n\nAssistant >>> {assistantResponse.Content}");
completionOptions.Messages.Add(new ChatRequestAssistantMessage(assistantResponse.Content));


// == Providing the user's request ==========
var hikeRequest =
"""
I would like to know the ration of hike I did in Canada compare to hikes done in other countries.
""";


Console.WriteLine($"\n\nUser >>> {hikeRequest}");
completionOptions.Messages.Add(new ChatRequestUserMessage(hikeRequest));

// == Retrieve the answer from HikeAI ==========
response = await openAIClient.GetChatCompletionsAsync(completionOptions);
assistantResponse = response.Choices[0].Message;

Console.WriteLine($"\n\nAssistant >>> {assistantResponse.Content}");
```
Compile and run your code, and have fun!