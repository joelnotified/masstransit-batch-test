targetScope = 'resourceGroup'

resource service_bus 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' = {
  name: 'sb-masstransit-test'
  location: resourceGroup().location
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  tags: {
    environment: 'test'
  }
}

resource access_policy 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2021-06-01-preview' = {
  name: 'access-key'
  parent: service_bus
  properties: {
    rights: [
      'Manage'
      'Listen'
      'Send'
    ]
  }
}

var connection_string = listKeys(access_policy.id, access_policy.apiVersion).primaryConnectionString
output connection_string string = connection_string
