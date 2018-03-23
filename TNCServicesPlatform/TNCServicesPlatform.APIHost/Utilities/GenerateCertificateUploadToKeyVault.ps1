####################################
$subscriptionId = "753a7f10-1bc9-4ac4-bdb2-fac72e44d41d"
$vaultName = 'tnckv4test'
$certificateName = 'TncSPKeyEncryptionCert'
####################################
$cert = New-SelfSignedCertificate -DnsName "www.tncai.org" -CertStoreLocation "cert:\LocalMachine\My" -Type DocumentEncryptionCertLegacyCsp
$securepfxpwd = ConvertTo-SecureString ¨CString 'tncai2018' ¨CAsPlainText ¨CForce
$certTP = $cert.Thumbprint
$certPath = 'cert:\LocalMachine\My\'+$certTP
Export-PfxCertificate -Cert $certPath -FilePath c:\clientcert.pfx -ChainOption EndEntityCertOnly -NoProperties -Password $securepfxpwd

Login-AzureRmAccount
Get-AzureRmSubscription
Set-AzureRmContext -SubscriptionId $subscriptionId
$cer = Import-AzureKeyVaultCertificate -VaultName $vaultName -Name $certificateName -FilePath 'c:\clientcert.pfx' -Password $securepfxpwd


#$policy = New-AzureKeyVaultCertificatePolicy -SubjectName "CN=www.tncai.org" -Type DocumentEncryptionCertLegacyCsp -IssuerName Self -ValidityInMonths 120
#Add-AzureKeyVaultCertificate -VaultName $vaultName -Name $certificateName -CertificatePolicy $policy 

# Open https://portal.azure.com/#@tncaioutlook.onmicrosoft.com/asset/Microsoft_Azure_KeyVault/Certificate/https://tnckv4test.vault.azure.net/certificates/TncSPKeyEncryptionCert/
# to download and install CER file