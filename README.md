# Create the Service Bus
Create a service bus (if needed) using a bicep deployment

`az deployment sub create --location northeurope -f ./bicep/main.bicep --name masstransit-test`

The output will contain the `connectionstring` to the new service bus instance

# Start the test application
`cd .\WorkerService\`

`$env:ASB_CONNECTION_STRING = '<connection_string_here>'`

`dotnet run`