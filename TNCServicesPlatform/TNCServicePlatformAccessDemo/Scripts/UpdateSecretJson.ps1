# read secret.json
$tncSubscriptionId = '753a7f10-1bc9-4ac4-bdb2-fac72e44d41d'
Login-AzureRmAccount
Set-AzureRmContext -SubscriptionId $tncSubscriptionId
$SecretJsonContent = [IO.File]::ReadAllText("..\secret.json")
$secretvalue = ConvertTo-SecureString $SecretJsonContent -AsPlainText -Force
$secret = Set-AzureKeyVaultSecret -VaultName 'TNCServicesKeyVault' -Name 'TNCServicesPlatformAPIHostAuth' -SecretValue $secretvalue
