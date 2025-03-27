# Kogase HTTP Request System

This system provides a flexible way to make HTTP requests in Unity using a fluent builder pattern. It includes support for API key authentication and various request types.

## Basic Usage

### Creating and Sending a Request

```csharp
using Kogase.Core.Http;
using UnityEngine;
using System.Collections;

public class ApiExample : MonoBehaviour
{
    [SerializeField] private string apiKey = "your-api-key-here";
    
    private HttpClient httpClient;
    
    private void Awake()
    {
        // Initialize the HTTP client with the API key and this MonoBehaviour for coroutine support
        httpClient = new HttpClient(apiKey, this);
    }
    
    public void FetchData()
    {
        // Build a GET request with API key authentication
        IHttpRequest request = HttpRequestBuilder.CreateGet("https://api.example.com/data")
            .WithApiKeyAuth(apiKey)
            .WithQueryParam("limit", "10")
            .Build();
            
        // Send the request and handle the response
        httpClient.Send(request, response =>
        {
            if (response.IsSuccess)
            {
                Debug.Log("Response: " + response.Content);
                // Process the data...
            }
            else
            {
                Debug.LogError("Error: " + response.Error);
            }
        });
    }
    
    // Async example using Task-based API
    public async void FetchDataAsync()
    {
        IHttpRequest request = HttpRequestBuilder.CreateGet("https://api.example.com/data")
            .WithApiKeyAuth(apiKey)
            .WithQueryParam("limit", "10")
            .Build();
            
        HttpResponse response = await httpClient.SendAsync(request);
        
        if (response.IsSuccess)
        {
            Debug.Log("Response: " + response.Content);
            // Process the data...
        }
        else
        {
            Debug.LogError("Error: " + response.Error);
        }
    }
}
```

### POST Request with JSON Body

```csharp
// Create a model for your data
[System.Serializable]
public class CreateUserRequest
{
    public string username;
    public string email;
    public int age;
}

// Build the request
CreateUserRequest userRequest = new CreateUserRequest
{
    username = "john_doe",
    email = "john@example.com",
    age = 30
};

IHttpRequest request = HttpRequestBuilder.CreatePost("https://api.example.com/users")
    .WithApiKeyAuth(apiKey)
    .WithBody(userRequest)
    .Build();
    
httpClient.Send(request, response => 
{
    // Handle response
});
```

### File Upload

```csharp
byte[] fileData = System.IO.File.ReadAllBytes("path/to/file.jpg");

IHttpRequest request = HttpRequestBuilder.CreatePost("https://api.example.com/upload")
    .WithApiKeyAuth(apiKey)
    .WithFile("file", fileData, "image.jpg", "image/jpeg")
    .WithFormData(new Dictionary<string, string>
    {
        { "description", "My awesome image" }
    })
    .Build();
    
httpClient.Send(request, response => 
{
    // Handle response
});
```

### Using Path Parameters

```csharp
IHttpRequest request = HttpRequestBuilder.CreateGet("https://api.example.com/users/{userId}")
    .WithApiKeyAuth(apiKey)
    .WithPathParam("userId", "123")
    .Build();
```

## Key Features

- Fluent API pattern for building requests
- Support for all common HTTP methods (GET, POST, PUT, DELETE, PATCH)
- JSON serialization using Unity's JsonUtility
- Path parameter substitution
- Query parameter support
- File uploads
- Form data
- API key authentication via X-API-Key header
- Async/await support
- Coroutine-based API

## API Key Authentication

The HTTP client supports using an API key for authentication. It can be provided in several ways:

1. When creating the HttpClient:
   ```csharp
   var client = new HttpClient("your-api-key");
   ```

2. Using the WithApiKeyAuth method on the request builder:
   ```csharp
   var request = HttpRequestBuilder.CreateGet("https://api.example.com")
       .WithApiKeyAuth("your-api-key")
       .Build();
   ```

3. Setting it later on the client:
   ```csharp
   client.SetApiKey("your-api-key");
   ```

The API key is sent as an `X-API-Key` header with each request. 