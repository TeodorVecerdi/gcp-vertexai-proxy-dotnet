param(
    [string]$Project = "vecerdi",
    [string]$Region = "us-central1",
    [string]$FunctionName = "mdl-vertex-proxy-function",
    [string]$ServiceAccount = "mdl-vertex-proxy-sa@vecerdi.iam.gserviceaccount.com",
    [string]$EntryPoint = "Vecerdi.VertexAIProxy.Service.Function",
    [string]$SourcePath = "."
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Deploying Cloud Function" -ForegroundColor Cyan
Write-Host "Function: $FunctionName" -ForegroundColor Yellow
Write-Host "Region: $Region" -ForegroundColor Yellow
Write-Host "Project: $Project" -ForegroundColor Yellow
Write-Host "Source: $SourcePath" -ForegroundColor Yellow

# Validate source path
if (-not (Test-Path $SourcePath)) {
    Write-Error "Source path not found: $SourcePath"
    exit 1
}

Write-Host "Deploying function..." -ForegroundColor Green

try {
    $deployArgs = @(
        "functions", "deploy", $FunctionName,
        "--gen2",
        "--runtime", "dotnet8",
        "--region", $Region,
        "--source", $SourcePath,
        "--trigger-http",
        "--service-account", $ServiceAccount,
        "--project", $Project,
        "--entry-point", $EntryPoint,
        "--no-allow-unauthenticated",
        "--set-build-env-vars=GOOGLE_BUILDABLE=Service"
    )
    & gcloud @deployArgs

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Function deployed successfully!" -ForegroundColor Green
    } else {
        Write-Error "Function deployment failed"
        exit 1
    }
} catch {
    Write-Error "Error deploying function: $_"
    exit 1
}