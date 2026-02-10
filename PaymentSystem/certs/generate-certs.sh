#!/bin/bash
# Bash skripta za generisanje SSL sertifikata za sve servise

echo "===== Generisanje SSL sertifikata za sve servise ====="

PASSWORD="dev-cert-2024"
DAYS=730  # 2 godine

# Funkcija za kreiranje sertifikata
generate_cert() {
    SERVICE_NAME=$1
    DNS_NAME=$2
    
    echo ""
    echo "----- $SERVICE_NAME -----"
    
    # Generiši private key
    openssl genrsa -out "${DNS_NAME}.key" 2048 2>/dev/null
    
    # Generiši CSR sa SAN (Subject Alternative Names)
    openssl req -new -key "${DNS_NAME}.key" -out "${DNS_NAME}.csr" \
        -subj "/CN=${DNS_NAME}" \
        -addext "subjectAltName=DNS:${DNS_NAME},DNS:localhost,IP:127.0.0.1" 2>/dev/null
    
    # Generiši self-signed sertifikat
    openssl x509 -req -in "${DNS_NAME}.csr" -signkey "${DNS_NAME}.key" \
        -out "${DNS_NAME}.crt" -days ${DAYS} \
        -extfile <(printf "subjectAltName=DNS:${DNS_NAME},DNS:localhost,IP:127.0.0.1\nkeyUsage=digitalSignature,keyEncipherment,dataEncipherment\nextendedKeyUsage=serverAuth") 2>/dev/null
    
    # Generiši PFX fajl sa modernim algoritmima (AES-256 umesto RC2)
    openssl pkcs12 -export -out "${DNS_NAME}.pfx" \
        -inkey "${DNS_NAME}.key" -in "${DNS_NAME}.crt" \
        -keypbe AES-256-CBC -certpbe AES-256-CBC -macalg SHA256 \
        -password "pass:${PASSWORD}" 2>/dev/null
    
    # Cleanup privremenih fajlova
    rm "${DNS_NAME}.csr"
    
    echo "✓ ${DNS_NAME}.pfx"
    echo "✓ ${DNS_NAME}.crt"
    echo "✓ ${DNS_NAME}.key"
}

cd "$(dirname "$0")"

# Backend API Sertifikati
echo ""
echo "===== Backend API Sertifikati ====="
generate_cert "PSP API" "psp-api"
generate_cert "WebShop API" "webshop-api"
generate_cert "Bank API" "bank-api"
generate_cert "PayPal API" "paypal-api"
generate_cert "Crypto API" "crypto-api"

# Frontend Sertifikati
echo ""
echo "===== Frontend Sertifikati ====="
generate_cert "WebShop Frontend" "frontend-webshop"
generate_cert "PSP Frontend" "frontend-psp"
generate_cert "Bank Frontend" "frontend-bank"

# Generiši localhost sertifikat (ako ga nemaš)
if [ ! -f "localhost+2.pem" ]; then
    echo ""
    echo "----- Localhost sertifikat -----"
    
    openssl req -x509 -newkey rsa:2048 -nodes \
        -keyout "localhost+2-key.pem" \
        -out "localhost+2.pem" \
        -days ${DAYS} \
        -subj "/CN=localhost" \
        -addext "subjectAltName=DNS:localhost,DNS:127.0.0.1,IP:127.0.0.1" 2>/dev/null
    
    echo "✓ localhost+2.pem"
    echo "✓ localhost+2-key.pem"
fi

echo ""
echo "===== Generisanje završeno ====="
echo "Lozinka za sve PFX fajlove: ${PASSWORD}"
echo ""
echo "Generirani sertifikati:"
echo "Backend API:"
echo "  - psp-api.pfx / psp-api.crt / psp-api.key"
echo "  - webshop-api.pfx / webshop-api.crt / webshop-api.key"
echo "  - bank-api.pfx / bank-api.crt / bank-api.key"
echo "  - paypal-api.pfx / paypal-api.crt / paypal-api.key"
echo "  - crypto-api.pfx / crypto-api.crt / crypto-api.key"
echo "Frontend:"
echo "  - frontend-webshop.pfx / frontend-webshop.crt / frontend-webshop.key"
echo "  - frontend-psp.pfx / frontend-psp.crt / frontend-psp.key"
echo "  - frontend-bank.pfx / frontend-bank.crt / frontend-bank.key"
echo "Localhost:"
echo "  - localhost+2.pem / localhost+2-key.pem"
