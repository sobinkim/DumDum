# DumDum ML-Agents

## Current Setup

- Unity package: `com.unity.ml-agents`
- Python: project venv at `.venv`
- Trainer package: `mlagents==1.1.0`
- Default behavior name: `DumDumTest`
- Default config: `Assets/SB/ML/Configs/DumDumTest.yaml`
- Training output: `MLRuns/`

## Train From The Unity Editor

Run:

```powershell
.\Tools\MLAgents\train-editor.ps1 -RunId dumdum-test -Force
```

When the trainer waits for Unity, press Play in the Unity Editor.

The agent GameObject must have:

- `Behavior Parameters`
- `Decision Requester`
- an `Agent` script
- Behavior Name: `DumDumTest`

## TensorBoard

Run:

```powershell
.\Tools\MLAgents\tensorboard.ps1
```

Then open:

```text
http://localhost:6006
```
