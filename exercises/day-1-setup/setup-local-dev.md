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