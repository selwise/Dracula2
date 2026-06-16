# Dracula Video Sprite Workflow

This notes the current process for turning Omni/Gemini-style character videos into in-game Dracula sprite strips.

## Core Rules

- Work one layer at a time: room first, character second, colliders third unless Selene explicitly changes priority.
- Do not rush into adjacent props, candles, coffin edits, UI, filters, or palette fixes while the current target is still being judged.
- Keep the source video and a raw magenta spritesheet before any cleanup.
- Do not crop tight around the figure. Keep the full video frame per cell so cape extremes are not clipped.
- If Selene provides a cleaned alpha sheet, use it directly. Do not re-key, matte-fix, palette-filter, resize, or otherwise process it unless she asks.
- If automated keying is used for an in-game test, keep that output separate from the raw sheet and document it as a keyed test asset.
- After Unity integration, rebuild with `Dracula/Build Crypt Prototype`, check the console, and capture a fresh Game View screenshot.

## Current Asset Layout

- Source/review sheets: `Assets/Art/Crypt/SourceSheets/`
- Current requested patch sheets: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/`
- Down walk alpha frames: `Assets/Art/Crypt/DraculaWalkDownAlpha12/`
- Down idle alpha frames: `Assets/Art/Crypt/DraculaIdleDownAlpha24/`
- Down ToIdle alpha frames: `Assets/Art/Crypt/DraculaToIdleDownAlpha24/`
- Right walk alpha frames: `Assets/Art/Crypt/DraculaWalkRightAlpha24/`
- Right idle alpha frames: `Assets/Art/Crypt/DraculaIdleRightAlpha12/`
- Up walk alpha frames: `Assets/Art/Crypt/DraculaWalkUpAlpha24/`
- Up idle alpha frames: `Assets/Art/Crypt/DraculaIdleUpAlpha12/`
- Runtime scene builder: `Assets/Editor/CryptPrototypeBuilder.cs`
- Runtime movement/animation controller: `Assets/Scripts/DraculaWalker.cs`

## Current Runtime Patch Sheets

The current requested patch sheets are in one clearly marked folder:

- Folder: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/`
- README: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/README.md`
- Manifest: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/00_MANIFEST.json`

Patch the transparent `*_sheet.png` files, not the checkerboard `*_preview.png` files. The JSON maps record the cell size, frame count, and source frame path for splitting edited sheets back into frames.

- Down walk: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/01_down_walk_sheet.png`
- Down ToIdle: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/02_down_to_idle_sheet.png`
- Down idle: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/03_down_idle_sheet.png`
- Side walk: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/04_side_walk_sheet.png`
- Up walk: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/05_up_walk_sheet.png`
- Up idle: `Assets/Art/Crypt/SourceSheets/Dracula_Current_Patch_Sheets/06_up_idle_sheet.png`

Side idle is intentionally excluded from this patch-sheet set for now. The current side-idle placeholder source is `D:/dracula-right.png`; its runtime canvas copy is `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_on_walk_canvas.png`. The active side-walk start frame is `Assets/Art/Crypt/DraculaWalkRightAlpha24/dracula_right_alpha24_00.png`.

## Down Walk

The down walk currently comes from Selene's cleaned alpha sheet.

- Input sheet: `Assets/Art/Crypt/SourceSheets/sheet-alpha.png`
- Output frames: `Assets/Art/Crypt/DraculaWalkDownAlpha12/dracula_down_alpha12_00.png` through `_11.png`
- Cell size: `1280x720`
- Pivot: bottom center
- Import scale in builder: `DraculaDownAlpha12Ppu = 458`
- Walk timing: `DraculaWalkFrameTime = 0.16`

## Down Idle And ToIdle

The current front-facing idle source is `D:/draculaidlenew.mp4`.

- Selected source frames: `0, 8, 16, 24, 32, 40, 48, 56, 64, 72, 80, 88, 96, 104, 112, 120, 128, 136, 144, 152, 160, 168, 176, 184`
- Raw review sheet: `Assets/Art/Crypt/SourceSheets/dracula_idle_down_new_video_24_raw_magenta_sheet_2row.png`
- Keyed review sheet: `Assets/Art/Crypt/SourceSheets/dracula_idle_down_new_video_24_keyed_alpha_sheet_2row.png`
- Keyed preview: `Assets/Art/Crypt/SourceSheets/dracula_idle_down_new_video_24_keyed_preview_2row.png`
- Idle output frames: `Assets/Art/Crypt/DraculaIdleDownAlpha24/dracula_idle_down_alpha24_00.png` through `_23.png`
- ToIdle output frames: `Assets/Art/Crypt/DraculaToIdleDownAlpha24/dracula_to_idle_down_alpha24_00.png` through `_23.png`
- Idle timing in builder: `DraculaIdleFrameTime = 0.18`
- ToIdle timing in builder: `DraculaToIdleFrameTime = 0.055`

The same 24 front-facing frames are used for idle and ToIdle for now. ToIdle plays quickly once after stopping, then the same quiet hold loops more slowly. This replaced the previous wrong-facing cape-settle ToIdle strip.

Latest cleanup: the head/face/upper chest region is locked from frame 0 so the idle no longer opens and closes Dracula's mouth.

- Static-head preview: `Assets/Art/Crypt/SourceSheets/dracula_idle_down_alpha24_subtle_static_head_preview_2row.png`

## Right And Left Walk

The right walk currently comes from `D:/right.mp4`.

- Selected source frames: `175, 178, 180, 183, 186, 188, 191, 194, 196, 199, 202, 204, 207, 210, 212, 215, 218, 220, 223, 226, 228, 231, 234, 236`
- Raw review sheet: `Assets/Art/Crypt/SourceSheets/dracula_walk_right_video_24_raw_magenta_sheet_4row.png`
- Keyed preview sheet: `Assets/Art/Crypt/SourceSheets/dracula_walk_right_video_24_keyed_preview_4row.png`
- Output frames: `Assets/Art/Crypt/DraculaWalkRightAlpha24/dracula_right_alpha24_00.png` through `_23.png`
- Cell size: `720x1280`
- Pivot: approximately bottom center, y pivot `0.144`
- Import scale in builder: `DraculaRightAlphaPpu = 568`
- Side timing in builder: `DraculaSideWalkFrameTime = 0.08`

The left walk is the right walk mirrored by `SpriteRenderer.flipX`.

## Right And Left Idle

The right idle currently uses Selene's ready-made placeholder still, repeated for all 12 frames. The rejected video-derived side idle is no longer the runtime source.

- Source still: `D:/dracula-right.png`
- Source copy: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_source.png`
- Runtime canvas copy: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_on_walk_canvas.png`
- Output frames: `Assets/Art/Crypt/DraculaIdleRightAlpha12/dracula_idle_right_alpha12_00.png` through `_11.png`
- Comparison preview: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_compare_preview.png`
- Repeated-frame preview: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_repeated_preview_2row.png`
- Meta note: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_20260616_meta.json`
- Runtime frame size: `720x1280`
- Alpha note: the source still is already RGBA. This pass did not key, matte-remove, or black-remove anything. It scaled and padded the ready still onto the existing side-walk canvas so runtime size and foot alignment match the walk cycle.
- Placeholder note: a slight white rim is acceptable for now; do not spend time on alpha cleanup unless Selene asks. The real next art task is a subtle matching side-idle animation.

The left idle is the right idle mirrored by `SpriteRenderer.flipX`. There is no side ToIdle strip; side movement stops directly into side idle.

## Up Walk

The up walk currently comes from `D:/dracula_up.mp4`.

- Selected source frames: `32, 37, 43, 48, 53, 59, 64, 69, 75, 80, 85, 91, 96, 101, 107, 112, 117, 123, 128, 133, 139, 144, 149, 155`
- Raw review sheet: `Assets/Art/Crypt/SourceSheets/dracula_walk_up_video_24_raw_magenta_sheet.png`
- Keyed preview: `Assets/Art/Crypt/SourceSheets/dracula_walk_up_video_24_keyed_preview.png`
- Output frames: `Assets/Art/Crypt/DraculaWalkUpAlpha24/dracula_up_alpha24_00.png` through `_23.png`
- Up timing in builder: `DraculaUpWalkFrameTime = 0.108`

## Up Idle

The up idle currently comes from `C:/Users/Selene/Downloads/Subtle_idle_animation_freeze_camera_202606152258.mp4`.

- Selected source frames: `0, 20, 40, 60, 80, 100, 120, 140, 160, 180, 200, 220`
- Raw review sheet: `Assets/Art/Crypt/SourceSheets/dracula_idle_up_video_12_raw_magenta_sheet_2row.png`
- Static-head preview: `Assets/Art/Crypt/SourceSheets/dracula_idle_up_alpha12_static_head_preview_2row.png`
- Output frames: `Assets/Art/Crypt/DraculaIdleUpAlpha12/dracula_idle_up_alpha12_00.png` through `_11.png`
- Cleanup note: upper cape/head area is locked from frame 0 so the idle reads as a quiet hold, not exaggerated breathing.

## Unity Integration

`CryptPrototypeBuilder` loads the active animation groups and assigns them to `DraculaWalker`.

`DraculaWalker` currently behaves like this:

- While moving, use direction-specific walk frames.
- When stopping from down/front movement, play `toIdleDown` once, then loop `idleDown`.
- When stopping from side movement, go directly into `idleSide`; left uses the mirrored right idle.
- When stopping from up movement, loop `idleUp`.

## Collider Handoff

If colliders need hand adjustment, the durable source is:

- Build-safe prefab: `Assets/Prefabs/Crypt/BG2ManualWalkColliders.prefab`
- Scene object after build: `Crypt Prototype/Isometric Crypt Room/Walk Boundary Colliders/BG2 Manual Walk Barrier Colliders`
- Player foot collider: `Crypt Prototype/Dracula/Ground Contact Collider - edit size/position here`
- Adjustment notes: `Docs/manual-collider-adjustment.md`

Scene-only room collider edits are overwritten by `Dracula/Build Crypt Prototype`; edit the prefab or apply scene overrides to the prefab for durable room-boundary changes. The player foot collider is generated by `CryptPrototypeBuilder`, so durable changes to that contact box go in the builder defaults. The old hidden `walkBoundary` polygon clamp is no longer assigned for bg2.

## Verification From Latest Pass

- Side-idle ready-alpha placeholder in scene: `Assets/Screenshots/codex_side_idle_ready_alpha/side_idle_ready_alpha_in_scene.png`
- Side-idle ready-alpha comparison preview: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_compare_preview.png`
- Earlier side-idle plus collider proof overlay: `Assets/Screenshots/codex_original_still_side_idle_fix/side_idle_original_still_collider_overlay.png`
- Unity console check after rebuild: no errors or warnings.
