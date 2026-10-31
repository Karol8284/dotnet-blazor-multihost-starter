targetScope = 'resourceGroup'

@description('Environment name prefix (e.g. demo, showcase).')
@minLength(3)
@maxLength(16)
param environmentName string = 'showcase'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('PostgreSQL Administrator username.')
param dbAdminUser string = 'starter_admin'

@description('PostgreSQL Administrator password.')
@secure()
param dbAdminPassword string

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

@description('App Service Plan SKU.')
param appServiceSku string = 'B1'

@description('PostgreSQL SKU.')
param postgresSku string = 'Standard_B1ms'

var uniqueSuffix = uniqueString(resourceGroup().id)
var baseName = '${environmentName}-${uniqueSuffix}'
var postgresServerName = '${baseName}-pg'
var appServiceName = '${baseName}-app'
var appServicePlanName = '${baseName}-asp'

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoringDeployment'
  params: {
    location: location
    baseName: baseName
  }
}

module database 'modules/database.bicep' = {
  name: 'databaseDeployment'
  params: {
    location: location
    serverName: postgresServerName
    adminUser: dbAdminUser
    adminPassword: dbAdminPassword
    skuName: postgresSku
  }
}

var connectionString = 'Host=${database.outputs.serverFullyQualifiedDomainName};Port=5432;Database=${database.outputs.databaseName};Username=${dbAdminUser};Password=${dbAdminPassword};SSL Mode=Require;Trust Server Certificate=true'

module appService 'modules/appservice.bicep' = {
  name: 'appServiceDeployment'
  params: {
    location: location
    appName: appServiceName
    appServicePlanName: appServicePlanName
    skuName: appServiceSku
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    defaultConnectionString: connectionString
    entraTenantId: entraTenantId
    entraApiClientId: entraApiClientId
    entraWebClientId: entraWebClientId
    entraMauiClientId: entraMauiClientId
    entraServerClientId: entraServerClientId
    entraServerClientSecret: entraServerClientSecret
    webInteractivity: webInteractivity
  }
}

output appServiceUrl string = appService.outputs.appServiceUrl
output appServiceName string = appService.outputs.appServiceName
output postgresServerFqdn string = database.outputs.serverFullyQualifiedDomainName
output logAnalyticsWorkspaceId string = monitoring.outputs.logAnalyticsWorkspaceId
