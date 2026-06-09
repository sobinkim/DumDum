param(
    [string] $Config = "Assets/SB/ML/Configs/DumDumTest.yaml",
    [string] $RunId = "dumdum-test",
    [switch] $Force,
    [int] $TimeScale = 20
)

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$Python = Join-Path $ProjectRoot ".venv\Scripts\python.exe"
$ConfigPath = Join-Path $ProjectRoot $Config
$ResultsPath = Join-Path $ProjectRoot "MLRuns"

if (-not (Test-Path $Python)) {
    throw "ML-Agents venv not found: $Python"
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
& $Python @ArgsList
