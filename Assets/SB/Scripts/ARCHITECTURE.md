# DumDum App Structure

## Goal

This app is a guided thinking practice loop, not a simple note board or reassurance tool.
Code should make that loop obvious:

1. Name the worry.
2. Separate facts from assumptions.
3. Check the automatic thought with evidence, counter-evidence, and another interpretation.
4. Choose one controllable action.
5. Save a one-line takeaway.
6. Check the current emotion.
7. Save the card to the evidence board.
8. Later, tag what actually happened.

## Folders

- `Domain`: Pure app concepts such as `WorryCard`, `WorryDraft`, emotions, outcome tags, and flow steps.
- `Application`: Presenters, repository, and flow content. This is where UX flow decisions live.
- `Views`: Thin Unity UI components. They show data and emit user intent.
- `Events`: App-level bus events only.

## Rules

- Do not use EventBus for UI commands like show panel, hide panel, or disable button.
- Use `WorryFlowPresenter` for step flow and validation.
- Use `WhiteboardPresenter` for card list, card detail, and outcome tagging.
- Put copy and prompt changes in `WorryFlowContent` when possible.
- Keep views passive. A view should not decide what screen comes next.
- Keep character copy in question-guide mode. It should prompt thinking, not judge or solve.

## EventBus Scope

Allowed app-level events:

- `WorryCardCreatedEvent`
- `WorryCardSelectedEvent`
- `WorryOutcomeTaggedEvent`

Everything else should usually be a direct Presenter-to-View call.
