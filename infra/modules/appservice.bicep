@description('Location for App Service.')
param location string = resourceGroup().location

@description('Name of the App Service.')
param appName string

@description('App Service Plan name.')
param appServicePlanName string

@description('App Service Plan SKU name.')
param skuName string = 'B1'

@description('App Service Plan SKU tier.')
param skuTier string = 'Basic'

@description('Application Insights connection string.')
param appInsightsConnectionString string

@description('PostgreSQL connection string.')
@secure()
param defaultConnectionString string

@description('Entra Tenant ID.')
param entraTenantId string

@description('Entra API Client ID.')
param entraApiClientId string

@description('Entra Web Client ID.')
param entraWebClientId string

@description('Entra MAUI Client ID.')
param entraMauiClientId string

@description('Entra Server Client ID (confidential client for Interactive Server).')
param entraServerClientId string = ''

@description('Entra Server Client Secret (confidential client for Interactive Server).')
@secure()
param entraServerClientSecret string = ''

@description('Web interactivity mode: WebAssembly or Server.')
@allowed([
  'WebAssembly'
  'Server'
])
param webInteractivity string = 'WebAssembly'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: skuName
    tier: skuTier
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: skuTier != 'Free' && skuTier != 'Shared'
      http20Enabled: true
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'AllowedHosts'
          value: '${appName}.azurewebsites.net'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: defaultConnectionString
        }
        {
          name: 'Database__Provider'
          value: 'PostgreSQL'
        }
        {
          name: 'Web__Interactivity'
          value: webInteractivity
        }
        {
          name: 'Authentication__Entra__TenantId'
          value: entraTenantId
        }
        {
          name: 'Authentication__Entra__ApiClientId'
          value: entraApiClientId
        }
        {
          name: 'Authentication__Entra__WebClientId'
          value: entraWebClientId
        }
        {
          name: 'Authentication__Entra__MauiClientId'
          value: entraMauiClientId
        }
        {
          name: 'Authentication__Entra__ServerClientId'
          value: entraServerClientId
        }
        {
          name: 'Authentication__Entra__ServerClientSecret'
          value: entraServerClientSecret
        }
        {
          name: 'Authentication__Entra__ApiScopeName'
          value: 'Tasks.ReadWrite'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
      ]
    }
  }
}

output appServiceUrl string = 'https://${webApp.properties.defaultHostName}'
output appServiceName string = webApp.name
output principalId string = webApp.identity.principalId
