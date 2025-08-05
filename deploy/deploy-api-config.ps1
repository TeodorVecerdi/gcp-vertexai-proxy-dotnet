param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [string]$OpenApiSpec = "./openapi.yaml",
    [string]$Project = "vecerdi",
    [string]$ApiName = "mdl-vertex-proxy-api",
    [string]$ServiceAccount = "mdl-vertex-proxy-sa@vecerdi.iam.gserviceaccount.com"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Deploying API Gateway Config" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "OpenAPI Spec: $OpenApiSpec" -ForegroundColor Yellow
Write-Host "Project: $Project" -ForegroundColor Yellow

# Validate OpenAPI file exists
if (-not (Test-Path $OpenApiSpec)) {
    Write-Error "OpenAPI spec file not found: $OpenApiSpec"
    exit 1
}

$configName = "mdl-vertex-proxy-function-config-$Version"

Write-Host "Creating API config: $configName..." -ForegroundColor Green

try {
    gcloud api-gateway api-configs create $configName `
        --api=$ApiName `
        --openapi-spec=$OpenApiSpec `
        --project=$Project `
        --backend-auth-service-account=$ServiceAccount

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ API config created successfully!" -ForegroundColor Green
        Write-Host "Config name: $configName" -ForegroundColor White
        Write-Host ""
        Write-Host "Next step: Update the gateway to use this config with:" -ForegroundColor Cyan
        Write-Host "  .\update-gateway-config.ps1 -Version $Version" -ForegroundColor Yellow
    } else {
        Write-Error "Failed to create API config"
        exit 1
    }
} catch {
    Write-Error "Error creating API config: $_"
    exit 1
}