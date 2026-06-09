# DumDum App Structure

## Goal

This app is a guided worry-thinking loop, not a simple note board.
Code should make that loop obvious:

1. Name the worry.
2. Estimate the felt probability.
3. Write a worst-case coping plan.
4. Choose one controllable action.
5. Check the current emotion.
6. Save the card to the evidence board.
7. Later, tag what actually happened.

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

## EventBus Scope

Allowed app-level events:

- `WorryCardCreatedEvent`
- `WorryCardSelectedEvent`
- `WorryOutcomeTaggedEvent`

Everything else should usually be a direct Presenter-to-View call.
