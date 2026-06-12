param(
    [string] $RunId = "dumdum-support-intervention",
    [switch] $Force,
    [int] $TimeScale = 20,
    [string] $PythonPath = $env:MLAGENTS_PYTHON
)

$ScriptPath = Join-Path $PSScriptRoot "train-editor.ps1"
& $ScriptPath `
    -Config "Assets/SB/ML/Configs/DumDumSupportIntervention.yaml" `
    -RunId $RunId `
    -TimeScale $TimeScale `
    -PythonPath $PythonPath `
    -Force:$Force
