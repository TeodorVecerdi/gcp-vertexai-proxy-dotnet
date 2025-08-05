param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [string]$Project = "vecerdi",
    [string]$Location = "us-central1",
    [string]$ApiName = "mdl-vertex-proxy-api",
    [string]$GatewayName = "mdl-vertex-proxy-gateway"
)

$ErrorActionPreference = "Stop"

Write-Host "🔄 Updating API Gateway" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Gateway: $GatewayName" -ForegroundColor Yellow
Write-Host "Location: $Location" -ForegroundColor Yellow
Write-Host "Project: $Project" -ForegroundColor Yellow

$configName = "mdl-vertex-proxy-function-config-$Version"

Write-Host "Updating gateway to use config: $configName..." -ForegroundColor Green

try {
    # Check if config exists first
    Write-Host "Verifying config exists..." -ForegroundColor Blue
    $configExists = gcloud api-gateway api-configs describe $configName --api=$ApiName --project=$Project --format="value(name)" 2>$null

    if (-not $configExists) {
        Write-Error "API config '$configName' not found. Please create it first with: .\deploy-api-config.ps1 -Version $Version"
        exit 1
    }

    Write-Host "Config found. Updating gateway..." -ForegroundColor Blue
    gcloud api-gateway gateways update $GatewayName `
        --api=$ApiName `
        --api-config=$configName `
        --location=$Location `
        --project=$Project

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Gateway updated successfully!" -ForegroundColor Green
        Write-Host "Gateway: $GatewayName" -ForegroundColor White
        Write-Host "Config: $configName" -ForegroundColor White

        # Get gateway URL
        Write-Host "Getting gateway URL..." -ForegroundColor Blue
        $gatewayUrl = gcloud api-gateway gateways describe $GatewayName --location=$Location --project=$Project --format="value(defaultHostname)" 2>$null
        if ($gatewayUrl) {
            Write-Host "Gateway URL: https://$gatewayUrl" -ForegroundColor Cyan
        }
    } else {
        Write-Error "Failed to update gateway"
        exit 1
    }
} catch {
    Write-Error "Error updating gateway: $_"
    exit 1
}