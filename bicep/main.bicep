targetScope = 'subscription'

param location string = 'northeurope'

resource resource_group 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-masstransit-test'
  location: location
}

module service_bus 'servicebus.bicep' = {
  name: 'service-bus-module'
  scope: resource_group
}

output sb_connection_string string = service_bus.outputs.connection_string
