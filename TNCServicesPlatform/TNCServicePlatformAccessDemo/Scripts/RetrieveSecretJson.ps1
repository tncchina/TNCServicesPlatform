# read secret.json
$tncSubscriptionId = '753a7f10-1bc9-4ac4-bdb2-fac72e44d41d'
Login-AzureRmAccount
Set-AzureRmContext -SubscriptionId $tncSubscriptionId
$secretjsoncontent = (get-azurekeyvaultsecret -vaultName "TNCServicesKeyVault" -name "TNCServicesPlatformAPIHostAuth").SecretValueText
$secretjsoncontent | Set-Content "..\secret.json"