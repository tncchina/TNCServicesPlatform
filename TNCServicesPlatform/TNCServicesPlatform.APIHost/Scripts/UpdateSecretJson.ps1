# read secret.json
$tncSubscriptionId = '771e833f-9316-45e5-94b7-8d32d5ec312b'
Login-AzureRmAccount
Set-AzureRmContext -SubscriptionId $tncSubscriptionId
$SecretJsonContent = [IO.File]::ReadAllText("..\secret.json")
$secretvalue = ConvertTo-SecureString $SecretJsonContent -AsPlainText -Force
$secret = Set-AzureKeyVaultSecret -VaultName 'TNCServicesKeyVault' -Name 'secretjsonvalue' -SecretValue $secretvalue
