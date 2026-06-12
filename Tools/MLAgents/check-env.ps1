$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$PythonCandidates = @(
    (Join-Path $ProjectRoot ".venv\Scripts\python.exe"),
    (Join-Path $ProjectRoot ".python310\python.exe")
)

$Python = $null
foreach ($Candidate in $PythonCandidates) {
    if (-not (Test-Path $Candidate)) {
        continue
    }

    & $Candidate --version *> $null
    if ($LASTEXITCODE -eq 0) {
        $Python = $Candidate
        break
    }
}

if ([string]::IsNullOrWhiteSpace($Python)) {
    throw "No working Python found. Tried: $($PythonCandidates -join ', ')"
}

& $Python --version
& $Python -c "import importlib.metadata as m; print('mlagents', m.version('mlagents')); print('mlagents-envs', m.version('mlagents-envs')); print('torch', m.version('torch'))"
& $Python -m mlagents.trainers.learn --help *> $null
if ($LASTEXITCODE -ne 0) {
    throw "mlagents trainer CLI failed to start."
}

Write-Host "mlagents trainer CLI ok"
