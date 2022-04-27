param parentName string
param currentAppSettings object
param appSettings object

resource siteSlotConfig 'Microsoft.Web/sites/slots/config@2021-02-01' = {
  name: '${parentName}/appsettings'
  properties: union(currentAppSettings, appSettings)
}
