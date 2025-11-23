$baseUrl = "http://localhost:5000/api"

# 1. Register/Login
$username = "vcstester"
$password = "Password123!"

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body (@{ username = $username; password = $password } | ConvertTo-Json) -ContentType "application/json"
    $token = $loginResponse.accessToken
    Write-Host "Logged in successfully."
} catch {
    Write-Host "Login failed, trying to register..."
    try {
        $registerResponse = Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method Post -Body (@{ username = $username; password = $password; displayName = "VCS Tester" } | ConvertTo-Json) -ContentType "application/json"
        $token = $registerResponse.accessToken
        Write-Host "Registered and logged in successfully."
    } catch {
        Write-Error "Registration failed: $_"
        exit 1
    }
}

$headers = @{ Authorization = "Bearer $token" }

# 2. Create Project
$projectName = "VCS Project " + (Get-Random)
try {
    $project = Invoke-RestMethod -Uri "$baseUrl/projects" -Method Post -Body (@{ name = $projectName; description = "Test Project" } | ConvertTo-Json) -Headers $headers -ContentType "application/json"
    Write-Host "Created project: $($project.id)"
} catch {
    Write-Error "Create project failed: $_"
    exit 1
}

# 3. Create Column
try {
    $column = Invoke-RestMethod -Uri "$baseUrl/projects/$($project.id)/columns" -Method Post -Body (@{ name = "To Do" } | ConvertTo-Json) -Headers $headers -ContentType "application/json"
    Write-Host "Created column: $($column.id)"
} catch {
    Write-Error "Create column failed: $_"
    exit 1
}

# 4. Create Task
try {
    $task = Invoke-RestMethod -Uri "$baseUrl/projects/$($project.id)/tasks" -Method Post -Body (@{ title = "Test Task"; columnId = $column.id } | ConvertTo-Json) -Headers $headers -ContentType "application/json"
    Write-Host "Created task: $($task.id) with Number: $($task.taskNumber)"
} catch {
    Write-Error "Create task failed: $_"
    exit 1
}

# 5. Add Repository
try {
    $repo = Invoke-RestMethod -Uri "$baseUrl/projects/$($project.id)/integrations/repositories" -Method Post -Body (@{ type = 0; url = "https://github.com/test/repo"; webhookSecret = "secret" } | ConvertTo-Json) -Headers $headers -ContentType "application/json"
    Write-Host "Added repository: $($repo.id)"
} catch {
    Write-Error "Add repository failed: $_"
    exit 1
}

# 6. Simulate Webhook
$payload = @{
    repository = @{
        html_url = "https://github.com/test/repo"
    }
    commits = @(
        @{
            id = "abc1234"
            message = "Fixes #$($task.taskNumber) - Fixed the bug"
            url = "https://github.com/test/repo/commit/abc1234"
            timestamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
            author = @{
                name = "DevUser"
            }
        }
    )
} | ConvertTo-Json -Depth 5

try {
    Invoke-RestMethod -Uri "$baseUrl/webhooks/github" -Method Post -Body $payload -ContentType "application/json" -Headers @{ "X-Hub-Signature-256" = "sha256=dummy" }
    Write-Host "Webhook sent successfully."
} catch {
    Write-Error "Webhook failed: $_"
    exit 1
}

# 7. Verify Commit Linked
Start-Sleep -Seconds 2
try {
    $updatedTask = Invoke-RestMethod -Uri "$baseUrl/projects/$($project.id)/tasks/$($task.id)" -Headers $headers
    if ($updatedTask.commits.Count -gt 0) {
        Write-Host "SUCCESS: Commit linked to task!"
        Write-Host "Commit Message: $($updatedTask.commits[0].message)"
    } else {
        Write-Error "FAILURE: No commits linked to task."
    }
} catch {
    Write-Error "Get task failed: $_"
    exit 1
}
