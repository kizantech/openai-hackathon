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