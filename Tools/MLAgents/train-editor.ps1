param(
    [string] $Config = "Assets/SB/ML/Configs/DumDumTest.yaml",
    [string] $RunId = "dumdum-test",
    [switch] $Force,
    [int] $TimeScale = 20,
    [string] $PythonPath = $env:MLAGENTS_PYTHON
)

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$PythonCandidates = @()
if (-not [string]::IsNullOrWhiteSpace($PythonPath)) {
    $PythonCandidates += $PythonPath
} else {
    $PythonCandidates += Join-Path $ProjectRoot ".venv\Scripts\python.exe"
    $PythonCandidates += Join-Path $ProjectRoot ".python310\python.exe"
}

$Python = $null
$PythonErrors = @()
foreach ($Candidate in $PythonCandidates) {
    if (-not (Test-Path $Candidate)) {
        $PythonErrors += "Missing: $Candidate"
        continue
    }

    $PythonCheck = & $Candidate --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        $Python = $Candidate
        break
    }

    $PythonErrors += "Failed: $Candidate`n$PythonCheck"
}

$ConfigPath = Join-Path $ProjectRoot $Config
$ResultsPath = Join-Path $ProjectRoot "MLRuns"
$TrainerLogPath = Join-Path $ResultsPath "_trainer_logs"

if ([string]::IsNullOrWhiteSpace($Python)) {
    throw "No working Python for ML-Agents was found.`n$($PythonErrors -join "`n")`nInstall Python 3.10, recreate .venv, or pass -PythonPath / MLAGENTS_PYTHON."
}

if (-not (Test-Path $ConfigPath)) {
    throw "Trainer config not found: $ConfigPath"
}

$ArgsList = @(
    "-m", "mlagents.trainers.learn",
    $ConfigPath,
    "--run-id", $RunId,
    "--results-dir", $ResultsPath,
    "--time-scale", $TimeScale
)

if ($Force) {
    $ArgsList += "--force"
}

Write-Host "Starting ML-Agents trainer..."
Write-Host "When the trainer says it is listening, press Play in Unity."
New-Item -ItemType Directory -Path $TrainerLogPath -Force | Out-Null
$SafeRunId = $RunId -replace '[^a-zA-Z0-9_.-]', '_'
$LogPath = Join-Path $TrainerLogPath "$SafeRunId-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
Write-Host "Trainer log: $LogPath"

& $Python @ArgsList 2>&1 | Tee-Object -FilePath $LogPath
$TrainerExitCode = $LASTEXITCODE
Write-Host "ML-Agents trainer exited with code $TrainerExitCode"
exit $TrainerExitCode
