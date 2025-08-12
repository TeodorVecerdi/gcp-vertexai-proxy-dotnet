param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [string]$OpenApiTemplate = "./openapi.template.yaml",
    [string]$Project = "vecerdi",
    [string]$ApiName = "mdl-vertex-proxy-api",
    [string]$FunctionName = "mdl-vertex-proxy-function",
    [string]$Region = "us-central1",
    [string]$ServiceAccount = "mdl-vertex-proxy-sa@vecerdi.iam.gserviceaccount.com"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Deploying API Gateway Config" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "OpenAPI Template: $OpenApiTemplate" -ForegroundColor Yellow
Write-Host "Project: $Project" -ForegroundColor Yellow

# Validate OpenAPI template file exists
if (-not (Test-Path $OpenApiTemplate)) {
    Write-Error "OpenAPI template file not found: $OpenApiTemplate"
    exit 1
}

# Get the Cloud Function URL
Write-Host "Getting Cloud Function URL..." -ForegroundColor Green
try {
    $functionUrl = gcloud functions describe $FunctionName `
        --region=$Region `
        --project=$Project `
        --format="value(serviceConfig.uri)"
    
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($functionUrl)) {
        Write-Error "Failed to get function URL or function not found"
        exit 1
    }
    
    Write-Host "Function URL: $functionUrl" -ForegroundColor Yellow
} catch {
    Write-Error "Error getting function URL: $_"
    exit 1
}

# Create temporary OpenAPI spec from template
$tempOpenApiSpec = "./openapi-$Version.yaml"
Write-Host "Creating OpenAPI spec from template..." -ForegroundColor Green

try {
    $templateContent = Get-Content $OpenApiTemplate -Raw
    $specContent = $templateContent -replace '\{\{FUNCTION_URL\}\}', $functionUrl
    $specContent | Out-File -FilePath $tempOpenApiSpec -Encoding UTF8
    
    Write-Host "Temporary OpenAPI spec created: $tempOpenApiSpec" -ForegroundColor Yellow
} catch {
    Write-Error "Error creating OpenAPI spec from template: $_"
    exit 1
}

$configName = "mdl-vertex-proxy-function-config-$Version"

Write-Host "Creating API config: $configName..." -ForegroundColor Green

try {
    gcloud api-gateway api-configs create $configName `
        --api=$ApiName `
        --openapi-spec=$tempOpenApiSpec `
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
        throw "API config creation failed"
    }
} catch {
    Write-Error "Error creating API config: $_"
    throw
} finally {
    # Ensure temporary file is removed
    Write-Host "Cleaning up temporary file..." -ForegroundColor Green
    if (Test-Path $tempOpenApiSpec) {
        Remove-Item $tempOpenApiSpec -ErrorAction SilentlyContinue
    }
}