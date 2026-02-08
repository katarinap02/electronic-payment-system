# PowerShell skripta za generisanje SSL sertifikata za sve servise

Write-Host "===== Generisanje SSL sertifikata za sve servise =====" -ForegroundColor Green

$password = "dev-cert-2024"
$pfxPassword = ConvertTo-SecureString -String $password -Force -AsPlainText

# Funkcija za kreiranje i export sertifikata
function New-ServiceCertificate {
    param (
        [string]$ServiceName,
        [string]$DnsName
    )
    
    Write-Host "`n----- $ServiceName -----" -ForegroundColor Cyan
    
    $cert = New-SelfSignedCertificate `
        -Subject "CN=$DnsName" `
        -DnsName $DnsName, "localhost", "127.0.0.1" `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -NotBefore (Get-Date) `
        -NotAfter (Get-Date).AddYears(2) `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -FriendlyName "$ServiceName Development Certificate" `
        -HashAlgorithm SHA256 `
        -KeyUsage DigitalSignature, KeyEncipherment, DataEncipherment `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")

    Write-Host "Thumbprint: $($cert.Thumbprint)" -ForegroundColor Yellow

    # Export PFX
    $pfxPath = Join-Path $PSScriptRoot "$DnsName.pfx"
    Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $pfxPassword | Out-Null
    Write-Host "PFX: $pfxPath" -ForegroundColor Green

    # Export CER
    $cerPath = Join-Path $PSScriptRoot "$DnsName.cer"
    Export-Certificate -Cert $cert -FilePath $cerPath -Type CERT | Out-Null
    Write-Host "CER: $cerPath" -ForegroundColor Green
    
    # Ukloni iz store
    Remove-Item -Path "Cert:\CurrentUser\My\$($cert.Thumbprint)" -Force
}

# Generiši sertifikate za sve servise
Write-Host "`n===== Backend API Sertifikati =====" -ForegroundColor Magenta
New-ServiceCertificate -ServiceName "PSP API" -DnsName "psp-api"
New-ServiceCertificate -ServiceName "WebShop API" -DnsName "webshop-api"
New-ServiceCertificate -ServiceName "Bank API" -DnsName "bank-api"
New-ServiceCertificate -ServiceName "PayPal API" -DnsName "paypal-api"
New-ServiceCertificate -ServiceName "Crypto API" -DnsName "crypto-api"

Write-Host "`n===== Frontend Sertifikati =====" -ForegroundColor Magenta
New-ServiceCertificate -ServiceName "WebShop Frontend" -DnsName "frontend-webshop"
New-ServiceCertificate -ServiceName "PSP Frontend" -DnsName "frontend-psp"
New-ServiceCertificate -ServiceName "Bank Frontend" -DnsName "frontend-bank"

Write-Host "`n===== Localhost Sertifikat =====" -ForegroundColor Magenta

# Proveri da li već postoji localhost sertifikat
$localhostPfxPath = Join-Path $PSScriptRoot "localhost+2.pfx"
$localhostCerPath = Join-Path $PSScriptRoot "localhost+2.pem"

if (-not (Test-Path $localhostPfxPath)) {
    Write-Host "`n----- Localhost -----" -ForegroundColor Cyan
    
    $localhostCert = New-SelfSignedCertificate `
        -Subject "CN=localhost" `
        -DnsName "localhost", "127.0.0.1" `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -NotBefore (Get-Date) `
        -NotAfter (Get-Date).AddYears(2) `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -FriendlyName "Localhost Development Certificate" `
        -HashAlgorithm SHA256 `
        -KeyUsage DigitalSignature, KeyEncipherment, DataEncipherment `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")
    
    # Export PFX
    Export-PfxCertificate -Cert $localhostCert -FilePath $localhostPfxPath -Password $pfxPassword | Out-Null
    Write-Host "PFX: $localhostPfxPath" -ForegroundColor Green
    
    # Export PEM (CER format)
    Export-Certificate -Cert $localhostCert -FilePath $localhostCerPath -Type CERT | Out-Null
    Write-Host "PEM: $localhostCerPath" -ForegroundColor Green
    
    # Ukloni iz store
    Remove-Item -Path "Cert:\CurrentUser\My\$($localhostCert.Thumbprint)" -Force
} else {
    Write-Host "Localhost sertifikat već postoji, preskačem..." -ForegroundColor Yellow
}

Write-Host "`n===== Generisanje zavrseno =====" -ForegroundColor Green
Write-Host "Lozinka za sve PFX fajlove: $password" -ForegroundColor Magenta
Write-Host "`nGenerirani sertifikati:" -ForegroundColor White
Write-Host "Backend API:" -ForegroundColor Yellow
Write-Host "  - psp-api.pfx / psp-api.cer" -ForegroundColor White
Write-Host "  - webshop-api.pfx / webshop-api.cer" -ForegroundColor White
Write-Host "  - bank-api.pfx / bank-api.cer" -ForegroundColor White
Write-Host "  - paypal-api.pfx / paypal-api.cer" -ForegroundColor White
Write-Host "  - crypto-api.pfx / crypto-api.cer" -ForegroundColor White
Write-Host "Frontend:" -ForegroundColor Yellow
Write-Host "  - frontend-webshop.pfx / frontend-webshop.cer" -ForegroundColor White
Write-Host "  - frontend-psp.pfx / frontend-psp.cer" -ForegroundColor White
Write-Host "  - frontend-bank.pfx / frontend-bank.cer" -ForegroundColor White
Write-Host "Localhost:" -ForegroundColor Yellow
Write-Host "  - localhost+2.pfx / localhost+2.pem" -ForegroundColor White
