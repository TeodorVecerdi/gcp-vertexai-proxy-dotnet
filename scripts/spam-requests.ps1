# spam-requests.ps1
param(
    [int]$ConcurrentRequests = 10,
    [int]$TotalRequests = 100,
    [string]$BaseUrl = "http://127.0.0.1:8080",
    [string]$Model = "gemini-2.5-flash"
)

$url = "$BaseUrl/v1/projects/vecerdi/locations/us-central1/publishers/google/models/${Model}:generateContent"
$body = @{
    model = $Model
    contents = @(
        @{
            role = "user"
            parts = @(
                @{ text = "Hello there!" }
            )
        }
    )
    generationConfig = @{
        thinkingConfig = @{
            thinkingBudget = 0
        }
    }
} | ConvertTo-Json -Depth 10

$headers = @{
    "Content-Type" = "application/json"
}

Write-Host "Starting $TotalRequests requests with $ConcurrentRequests concurrent to $url..."
Write-Host "Payload: $body"
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

$jobs = @()
$requestCount = 0
$results = @()

while ($requestCount -lt $TotalRequests) {
    # Limit concurrent jobs
    while ($jobs.Count -ge $ConcurrentRequests) {
        $completed = $jobs | Where-Object { $_.State -eq 'Completed' -or $_.State -eq 'Failed' }
        foreach ($job in $completed) {
            $result = Receive-Job -Job $job
            $results += $result
            if ($result.StatusCode -eq 200) {
                Write-Host "✓ Request $($result.Id) completed: HTTP $($result.StatusCode) in $($result.ElapsedMs)ms" -ForegroundColor Green
            } else {
                Write-Host "✗ Request $($result.Id) failed: HTTP $($result.StatusCode) in $($result.ElapsedMs)ms" -ForegroundColor Red
            }
            Remove-Job -Job $job
        }
        $jobs = $jobs | Where-Object { $_.State -eq 'Running' }
        Start-Sleep -Milliseconds 10
    }

    # Start new request
    $job = Start-Job -ScriptBlock {
        param($url, $body, $headers, $requestId)
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        try {
            $response = Invoke-RestMethod -Uri $url -Method POST -Body $body -Headers $headers -ErrorAction Stop
            $sw.Stop()
            return @{
                Id = $requestId
                StatusCode = 200
                ElapsedMs = $sw.ElapsedMilliseconds
                Success = $true
            }
        } catch {
            $sw.Stop()
            $statusCode = if ($_.Exception.Response) {
                [int]$_.Exception.Response.StatusCode
            } else {
                500
            }
            return @{
                Id = $requestId
                StatusCode = $statusCode
                ElapsedMs = $sw.ElapsedMilliseconds
                Success = $false
                Error = $_.Exception.Message
            }
        }
    } -ArgumentList $url, $body, $headers, ($requestCount + 1)

    $jobs += $job
    $requestCount++
    Write-Progress -Activity "Sending requests" -Status "$requestCount/$TotalRequests sent" -PercentComplete (($requestCount / $TotalRequests) * 100)
}

# Wait for remaining jobs
Write-Host "Waiting for remaining requests to complete..."
while ($jobs.Count -gt 0) {
    $completed = $jobs | Where-Object { $_.State -eq 'Completed' -or $_.State -eq 'Failed' }
    foreach ($job in $completed) {
        $result = Receive-Job -Job $job
        $results += $result
        if ($result.StatusCode -eq 200) {
            Write-Host "✓ Request $($result.Id) completed: HTTP $($result.StatusCode) in $($result.ElapsedMs)ms" -ForegroundColor Green
        } else {
            Write-Host "✗ Request $($result.Id) failed: HTTP $($result.StatusCode) in $($result.ElapsedMs)ms" -ForegroundColor Red
        }
        Remove-Job -Job $job
    }
    $jobs = $jobs | Where-Object { $_.State -eq 'Running' }
    Start-Sleep -Milliseconds 100
}

$stopwatch.Stop()

# Calculate statistics
$successful = ($results | Where-Object { $_.Success }).Count
$failed = $results.Count - $successful
$avgResponseTime = ($results | Measure-Object -Property ElapsedMs -Average).Average
$minResponseTime = ($results | Measure-Object -Property ElapsedMs -Minimum).Minimum
$maxResponseTime = ($results | Measure-Object -Property ElapsedMs -Maximum).Maximum
$totalTime = $stopwatch.ElapsedMilliseconds
$requestsPerSecond = [math]::Round(($TotalRequests / ($totalTime / 1000)), 2)

Write-Host "`n=== RESULTS ===" -ForegroundColor Cyan
Write-Host "Total requests: $TotalRequests"
Write-Host "Successful: $successful" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if($failed -gt 0) { "Red" } else { "Green" })
Write-Host "Total time: $totalTime ms"
Write-Host "Requests/second: $requestsPerSecond"
Write-Host "Response times:"
Write-Host "  Average: $([math]::Round($avgResponseTime, 2)) ms"
Write-Host "  Minimum: $minResponseTime ms"
Write-Host "  Maximum: $maxResponseTime ms"

if ($failed -gt 0) {
    Write-Host "`nFailures:" -ForegroundColor Red
    $results | Where-Object { -not $_.Success } | ForEach-Object {
        Write-Host "  Request $($_.Id): HTTP $($_.StatusCode) - $($_.Error)" -ForegroundColor Red
    }
}