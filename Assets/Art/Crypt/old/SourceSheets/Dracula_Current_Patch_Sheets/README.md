# Dracula Current Patch Sheets

This folder contains the current editable Dracula spritesheets requested for manual alpha/paint patching.

Edit the transparent `*_sheet.png` files. The `*_preview.png` files have checkerboards and labels for review only. The `*_map.json` files record frame order, cell size, and source runtime frame paths.

## Sheets

- `01_down_walk_sheet.png`
  - Runtime use: `DraculaWalker.walkDown`
  - Frames: 12
  - Cell size: `1280x720`

- `02_down_to_idle_sheet.png`
  - Runtime use: `DraculaWalker.toIdleDown`
  - Frames: 24
  - Cell size: `1280x720`

- `03_down_idle_sheet.png`
  - Runtime use: `DraculaWalker.idleDown`
  - Frames: 24
  - Cell size: `1280x720`

- `04_side_walk_sheet.png`
  - Runtime use: `DraculaWalker.walkSide`
  - Frames: 24
  - Cell size: `720x1280`
  - Left walk mirrors this sheet with `SpriteRenderer.flipX`.

- `05_up_walk_sheet.png`
  - Runtime use: `DraculaWalker.walkUp`
  - Frames: 24
  - Cell size: `1280x720`

- `06_up_idle_sheet.png`
  - Runtime use: `DraculaWalker.idleUp`
  - Frames: 12
  - Cell size: `1280x720`

## Side Idle Note

Side idle is intentionally not included here for now.

Current side-idle placeholder source:

- Ready source: `D:/dracula-right.png`
- Runtime canvas copy: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_on_walk_canvas.png`
- Runtime frames folder: `Assets/Art/Crypt/DraculaIdleRightAlpha12/`

The current side-idle placeholder may have a slight white rim, and that is accepted for now.

## Side Walk Start Image

The active side-walk start frame is:

- `Assets/Art/Crypt/DraculaWalkRightAlpha24/dracula_right_alpha24_00.png`

Use this when you need the first frame of the current side-walk cycle.

## Rules

- Do not run automated alpha cleanup or black/dark keying unless Selene explicitly asks.
- Do not crop the sheets or alter their cell sizes.
- Do not edit preview sheets and expect changes to go into runtime.
- Keep side-left derived from side-right mirroring unless the runtime code changes.
