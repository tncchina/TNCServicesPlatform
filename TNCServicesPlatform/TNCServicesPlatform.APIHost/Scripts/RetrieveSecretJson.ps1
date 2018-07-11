# read secret.json
$tncSubscriptionId = '771e833f-9316-45e5-94b7-8d32d5ec312b'
Login-AzureRmAccount
Set-AzureRmContext -SubscriptionId $tncSubscriptionId
$secretjsoncontent = (get-azurekeyvaultsecret -vaultName "TNCServicesKeyVault" -name "secretjsonvalue").SecretValueText
$secretjsoncontent | Set-Content "..\secret.json"