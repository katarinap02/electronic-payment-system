#!/bin/sh
# Nginx entrypoint - automatski konvertuje PSP sertifikate

set -e

echo "===== Nginx PSP Load Balancer Entrypoint ====="

CERT_DIR="/etc/nginx/certs"
PFX_FILE="${CERT_DIR}/psp-api.pfx"
CRT_FILE="${CERT_DIR}/psp-api-nginx.crt"
KEY_FILE="${CERT_DIR}/psp-api-nginx.key"
PASSWORD="dev-cert-2024"

# Proveri da li PFX postoji
if [ ! -f "$PFX_FILE" ]; then
    echo "ERROR: PFX file not found: $PFX_FILE"
    exit 1
fi

# Konvertuj PFX u PEM format ako već ne postoje
if [ ! -f "$CRT_FILE" ] || [ ! -f "$KEY_FILE" ]; then
    echo "Converting PFX to PEM format..."
    
    # Extract certificate
    echo "Extracting certificate..."
    if ! openssl pkcs12 -in "$PFX_FILE" -clcerts -nokeys -out "$CRT_FILE" \
        -passin pass:$PASSWORD -passout pass: 2>&1; then
        echo "ERROR: Failed to extract certificate"
        exit 1
    fi
    
    # Extract private key (with password)
    echo "Extracting private key..."
    if ! openssl pkcs12 -in "$PFX_FILE" -nocerts -out "${CERT_DIR}/psp-api-nginx-temp.key" \
        -passin pass:$PASSWORD -passout pass:$PASSWORD 2>&1; then
        echo "ERROR: Failed to extract private key"
        exit 1
    fi
    
    # Remove password from private key
    echo "Removing password from private key..."
    if ! openssl rsa -in "${CERT_DIR}/psp-api-nginx-temp.key" -out "$KEY_FILE" \
        -passin pass:$PASSWORD 2>&1; then
        echo "ERROR: Failed to remove password from private key"
        exit 1
    fi
    
    # Cleanup
    rm -f "${CERT_DIR}/psp-api-nginx-temp.key"
    
    echo "✓ Certificates converted successfully"
else
    echo "✓ Certificates already exist"
fi

# Validate nginx config
echo "Testing nginx configuration..."
nginx -t

# Start nginx
echo "Starting nginx..."
exec nginx -g "daemon off;"
