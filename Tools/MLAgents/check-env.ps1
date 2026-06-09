$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$Python = Join-Path $ProjectRoot ".venv\Scripts\python.exe"

if (-not (Test-Path $Python)) {
    throw "ML-Agents venv not found: $Python"
}

& $Python --version
& $Python -m pip show mlagents mlagents-envs torch
& $Python -m mlagents.trainers.learn --help
