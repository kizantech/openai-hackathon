## Copilot API Plugins (private preview)

>As of current writing, the following exercise will only work for members of the Azure Copilot API Plugin Private Preview

### Create an API

For working with Copilot API Plugins, you simply need any REST capable API. You can use your language of choice, as long as it supports the OpenAPI v3.0 specification. For this example, we're going to create an API using .NET 8 SDK WebAPI project.

```bash
dotnet new 
```

### Create the OpenAPI Specification Document

If you are building your own API, you'll need to generate your own `openapi.yaml` file. If you are using .NET, you can use the `Swashbuckle` open source package to generate an OpenAPI v3 specification file from ASP.NET Core microservices.

### Create your API Manifest File

The API Manifest, describes to Copilot how to interact with your API. Below is a sample manifest for the API.guru API:

```json
{
  "schema_version": "v1",
  "name_for_model": "apiguru",
  "description_for_model": "Plugin for finding API's available to developers on the web that are open and freely available for use. These API's are all categorized by provider.",
  "name_for_human": "APIs.Guru",
  "description_for_human": "Find open APIs available on the web, useful for development and demo purposes!",
  "api": {
    "type": "openapi",
    "url": "https://api.apis.guru/v2/openapi.yaml",
    "is_user_authenticated": false
  },
  "auth": {
    "type": "none"
  },
  "logo_url": "https://apis.guru/branding/logo_vertical.svg",
  "contact_email": "contact@contoso.com",
  "legal_info_url": "https://github.com/APIs-guru/openapi-directory?tab=CC0-1.0-1-ov-file",
  "privacy_policy_url": "https://apis.guru/about/"
}
```