param fromEmail string = 'pamelding@bardarswingclub.no'
param adminEmails string = 'gundersen@gmail.com, erikly@gmail.com'
param websiteName string = 'bsc-medlemmer'
param slotName string = 'test'
param vaultName string = 'bsc-medlemmer-keyvault'

resource testSlot 'Microsoft.Web/sites/slots@2021-03-01' existing = {
  name: '${websiteName}/${slotName}'
}

module testAppSettings './appsettings.bicep' = {
  name: 'appsettings'
  params: {
    parentName: testSlot.name
    currentAppSettings: list('${testSlot.id}/config/appsettings', '2020-12-01').properties
    appSettings: {
      'Stripe:PublicKey': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Stripe-PublicKey)'
      'Stripe:SecretKey': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Stripe-SecretKey)'
      'Email:From': fromEmail
      'ConnectionStrings:DefaultConnection': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=DatabaseConnectionString)'
      'AdminEmails': adminEmails
      'Email:SendGridApiKey': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=SendGrid-ApiKey)'
      'Authentication:Microsoft:ClientId': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Microsoft-ClientId)'
      'Authentication:Microsoft:ClientSecret': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Microsoft-ClientSecret)'
      'Authentication:Facebook:AppId': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Facebook-AppId)'
      'Authentication:Facebook:AppSecret': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Facebook-AppSecret)'
      'Authentication:Vipps:ClientId': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Vipps-ClientId)'
      'Authentication:Vipps:ClientSecret': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Vipps-ClientSecret)'
      'Vipps:SubscriptionKey': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Vipps-SubscriptionKey)'
      'Vipps:MerchantSerialNumber': '@Microsoft.KeyVault(VaultName=${vaultName};SecretName=Vipps-MerchantSerialNumber)'
      'Vipps:CallbackPrefix': 'https://bsc-medlemmer.azurewebsites.net/vipps/callback'
    }
  }
}
