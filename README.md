# .NET 8 ASP.NET Core Web API Sample

This project demonstrates a sample API and a background service that receives messages from Azure Service Bus.

## Features
- ASP.NET Core Web API targeting .NET 8
- Background service for receiving messages from Azure Service Bus
- Sample API endpoint

## Azure Service Bus Setup
Replace the placeholders in `appsettings.json` with your actual Azure Service Bus connection string and queue name.

```
"AzureServiceBus": {
  "ConnectionString": "<YOUR_SERVICE_BUS_CONNECTION_STRING>",
  "QueueName": "<YOUR_QUEUE_NAME>"
}
```

## How to Run
1. Update `appsettings.json` with your Azure Service Bus details.
2. Run the project:
   ```powershell
   dotnet run
   ```
3. The API will be available at `http://localhost:5000` (or the port specified in launchSettings.json).

## Endpoints
- `GET /weatherforecast` - Sample endpoint

## Notes
- The background service will log received messages to the console.
- This is a sample setup. Add error handling and business logic as needed.
