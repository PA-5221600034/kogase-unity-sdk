## Installation

1. Import the Kogase Unity SDK package into your Unity project.
    - Open your Unity project.
    - In the Unity Editor, go to Package Manager.
    - Click the "+" button and select "Add package from git URL...".
    - Paste the following URL: `https://github.com/PA-5221600034/kogase-unity-sdk.git`
    - Click "Add".

2. Configure the KogaseSDK in the inspector.
    - From header menu, click `Kogase` > `Edit Settings`.
    - Input your API URL and API Key.
    - (optional) If you haven't created an API key yet, you can create one in the Kogase dashboard.
    - Test the connection by clicking the "Test Connection" button.
    - Click "Save".

## Usage

To use the Kogase Unity SDK, follow these steps:

### Begin SDK Session

```csharp
// Import Kogase namespace
using Kogase;

public class GameManager : MonoBehaviour  {
    private void Awake()
    {
        // Begin the SDK session
        KogaseSDK.BeginSession();
    }
}
```

### Example Basic Event Tracking

```csharp
// Import Kogase namespace
using Kogase;
using Kogase.Dtos;

// Record a simple event
KogaseSDK.Api.RecordEvent(
    new RecordEventRequest
    {
        EventType = "Test",
        EventName = "Test",
    }
);

// Record an event with parameters
var parameters = new Dictionary<string, object>
{
    { "level_id", "level_1" },
    { "difficulty", "hard" },
    { "player_health", 100 }
};
KogaseSDK.Api.RecordEvent(
    new RecordEventRequest
    {
        EventType = "level",
        EventName = "level_completed",
        Payloads = parameters,
    }
);
```

## Features

- Automatic event batching and sending
- Offline event queueing
- Automatic retry on failed network requests
- Device information tracking
- Installation tracking
- Cross-scene persistence

## Technical Details

- Events are stored in memory until flushed
- Failed event sends are automatically requeued
- The SDK is thread-safe and handles Unity's lifecycle events appropriately
- Installation events are automatically tracked on first launch

## Support

For issues, feature requests, or questions, please visit the [Kogase GitHub repository](https://github.com/PA-5221600034/kogase). 