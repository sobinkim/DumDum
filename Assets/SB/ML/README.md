# DumDum ML-Agents

## Current Setup

- Unity package: `com.unity.ml-agents`
- Python: project venv at `.venv`
- Fallback Python: project-local `.python310` runtime
- Trainer package: `mlagents==1.1.0`
- Support intervention behavior name: `DumDumSupportIntervention`
- Support intervention config: `Assets/SB/ML/Configs/DumDumSupportIntervention.yaml`
- Training output: `MLRuns/`

## Support Intervention Agent

`DumDumSupportAgent` learns which support notification is appropriate for a simulated user state.

Actions:

- `0`: no notification
- `1`: send a past takeaway reminder
- `2`: send an outcome statistics reminder
- `3`: suggest a daily closure reminder

Observations include card counts, tagged-result ratio, today's card count, takeaway count, latest probability, and latest emotion.

## Create Training Scene

In the Unity Editor, run:

```text
DumDum > ML > Rebuild Support Training Scene
```

Open:

```text
Assets/SB/ML/TrainingScenes/DumDumSupportTraining.unity
```

## Start 100k Training From Unity

Use this first. It starts the Python trainer, opens the training scene, then enters Play Mode automatically:

```text
DumDum > ML > Start Support Training 100k
```

You can also open the launcher window:

```text
DumDum > ML > Support Training Launcher
```

The launcher runs:

```powershell
Tools/MLAgents/train-support-intervention.ps1
```

The trainer config uses `max_steps: 100000`.

The in-scene `Start 100k Training` button is kept as a fallback, but Editor training is more reliable when the trainer starts before Play Mode.

## Train From The Unity Editor

Run:

```powershell
.\Tools\MLAgents\train-support-intervention.ps1 -RunId dumdum-support-intervention -Force
```

If PowerShell blocks local scripts, run:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\MLAgents\train-support-intervention.ps1 -RunId dumdum-support-intervention -Force
```

If `.venv` points to a missing Python install, the scripts automatically fall back to `.python310\python.exe`. You can still pass a custom Python path:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\MLAgents\train-support-intervention.ps1 -PythonPath "C:\Path\To\Python310\python.exe" -Force
```

When the trainer waits for Unity, press Play in the Unity Editor.

The agent GameObject must have:

- `Behavior Parameters`
- `Decision Requester`
- an `Agent` script
- Behavior Name: `DumDumSupportIntervention`

## TensorBoard

Run:

```powershell
.\Tools\MLAgents\tensorboard.ps1
```

Then open:

```text
http://localhost:6006
```
