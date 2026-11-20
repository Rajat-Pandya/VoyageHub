param(
    [string]$Server = ".\SQLEXPRESS06",
    [string]$Database = "AngularAuthDb"
)

$ErrorActionPreference = "Stop"

Write-Host "Applying database schema to server: $Server, database: $Database using Windows Authentication..."
$scriptPath = Join-Path $PSScriptRoot "VoyageHub_Schema.sql"

if (-not (Test-Path $scriptPath)) {
    Write-Error "Schema script not found at $scriptPath"
    exit 1
}

# Run using sqlcmd with Windows Authentication (-E)
sqlcmd -S $Server -d master -E -i $scriptPath
Write-Host "Database migration completed successfully."
