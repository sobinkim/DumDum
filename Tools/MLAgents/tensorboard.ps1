param(
    [int] $Port = 6006
)

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$Python = Join-Path $ProjectRoot ".venv\Scripts\python.exe"
$ResultsPath = Join-Path $ProjectRoot "MLRuns"

if (-not (Test-Path $Python)) {
    throw "ML-Agents venv not found: $Python"
}

New-Item -ItemType Directory -Path $ResultsPath -Force | Out-Null
& $Python -m tensorboard.main --logdir $ResultsPath --port $Port
