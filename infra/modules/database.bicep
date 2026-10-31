@description('Location for PostgreSQL Flexible Server.')
param location string = resourceGroup().location

@description('Name of the PostgreSQL Flexible Server.')
param serverName string

@description('Administrator username for PostgreSQL.')
@secure()
param adminUser string

@description('Administrator password for PostgreSQL.')
@secure()
param adminPassword string

@description('Database name to create on server.')
param databaseName string = 'starter_db'

@description('Compute SKU name.')
param skuName string = 'Standard_B1ms'

@description('Compute SKU tier.')
param skuTier string = 'Burstable'

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: serverName
  location: location
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {
    version: '16'
    administratorLogin: adminUser
    administratorLoginPassword: adminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource allowAzureServicesFirewall 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = {
  parent: postgresServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

output serverFullyQualifiedDomainName string = postgresServer.properties.fullyQualifiedDomainName
output serverName string = postgresServer.name
output databaseName string = database.name
