# ============================================
# CRYPTO TRANSACTION MONITOR
# ============================================
# Prati blockchain i detektuje incoming TX
# ============================================

param(
    [string]$TxHash = ""
)

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  BLOCKCHAIN TRANSACTION MONITOR" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$merchantWallet = "0x3b7dc23209CfB02bCA82e56c8c93D370B1f0b19b"

if ($TxHash) {
    # === MODE 1: Validate specific transaction ===
    Write-Host "Checking transaction: $TxHash" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "Opening Sepolia Etherscan..." -ForegroundColor Cyan
    Start-Process "https://sepolia.etherscan.io/tx/$TxHash"
    
    Write-Host ""
    Write-Host "Etherscan will show:" -ForegroundColor White
    Write-Host "  • Status (Success/Pending)" -ForegroundColor Gray
    Write-Host "  • Block confirmations" -ForegroundColor Gray
    Write-Host "  • From/To addresses" -ForegroundColor Gray
    Write-Host "  • Amount sent" -ForegroundColor Gray
    Write-Host ""
    
} else {
    # === MODE 2: Monitor recent transactions ===
    Write-Host "Monitoring recent transactions to merchant wallet..." -ForegroundColor Yellow
    Write-Host "Wallet: $merchantWallet" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "Opening Sepolia Etherscan..." -ForegroundColor Cyan
    Start-Process "https://sepolia.etherscan.io/address/$merchantWallet"
    
    Write-Host ""
    Write-Host "On Etherscan, you should see:" -ForegroundColor White
    Write-Host "  1. Your initial faucet transaction (0.05 ETH)" -ForegroundColor Gray
    Write-Host "  2. Any new incoming transactions you send" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  HOW TO SEND TEST TRANSACTION" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Open MetaMask extension" -ForegroundColor White
Write-Host "2. Make sure you're on Sepolia Testnet" -ForegroundColor White
Write-Host "3. Click 'Send'" -ForegroundColor White
Write-Host "4. Paste merchant address:" -ForegroundColor White
Write-Host "   " -NoNewline
Write-Host "$merchantWallet" -ForegroundColor Cyan
Write-Host "5. Enter amount: 0.001 ETH (small test)" -ForegroundColor White
Write-Host "6. Click 'Confirm'" -ForegroundColor White
Write-Host "7. Copy transaction hash from MetaMask" -ForegroundColor White
Write-Host "8. Run this script again:" -ForegroundColor White
Write-Host "   " -NoNewline
Write-Host ".\monitor-transaction.ps1 -TxHash 'YOUR_TX_HASH'" -ForegroundColor Yellow
Write-Host ""

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  WHAT HAPPENS NEXT" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "• BlockchainMonitorService polls every 10 seconds" -ForegroundColor White
Write-Host "• It scans last 5 blocks for incoming TX" -ForegroundColor White
Write-Host "• When found, it validates amount & recipient" -ForegroundColor White
Write-Host "• Waits for 1+ confirmations" -ForegroundColor White
Write-Host "• Updates status: PENDING -> CONFIRMING -> CAPTURED" -ForegroundColor White
Write-Host ""
Write-Host "Check API logs to see when TX is detected!" -ForegroundColor Cyan
Write-Host ""
