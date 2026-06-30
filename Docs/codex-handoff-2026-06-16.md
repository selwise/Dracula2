# Codex Handoff - Dracula2 Crypt Prototype - 2026-06-16

> Newer handoff exists: `Docs/codex-handoff-2026-06-30.md`.

This is the current working handoff for the Dracula2 crypt prototype after the room, Dracula sprite, animation, and collider iteration pass.

## Read This First

- Selene has been explicit: work one target at a time. Do not drift from room to coffin to candles to UI to filters in one pass.
- Trust manual alpha assets. If Selene says a PNG has alpha or is cleaned, copy/use it directly unless she asks for processing.
- Do not key out black from Dracula. That destroyed cape/costume pixels in the rejected side-idle attempt.
- Do not crop tight around generated character frames. Keep enough cell width/height for the full cape.
- Never call a visual task done without a fresh Unity Game View screenshot.
- The coffin is not the active target in this handoff. Do not re-polish or re-invent it unless Selene asks.

## Current Unity Entry Points

- Project: `C:\Gamedev\Dracula2`
- Active scene: `Assets/Scenes/CryptPrototype.unity`
- Rebuild menu item: `Dracula/Build Crypt Prototype`
- Scene builder: `Assets/Editor/CryptPrototypeBuilder.cs`
- Movement/animation controller: `Assets/Scripts/DraculaWalker.cs`
- Sprite workflow notes: `Docs/dracula-video-sprite-workflow.md`
- Collider handoff notes: `Docs/manual-collider-adjustment.md`

After any asset/code change:

1. Refresh/compile Unity.
2. Run `Dracula/Build Crypt Prototype`.
3. Check console errors/warnings.
4. Capture and inspect a fresh Game View screenshot.

## Room State

- The prototype currently uses the bg2 path because `Assets/Art/Crypt/bg2_downscaled.png` exists.
- `CryptPrototypeBuilder.UsesBg2Room()` switches to bg2 when that file is present.
- Active bg2 room layer:
  - `Assets/Art/Crypt/bg2_downscaled.png`
  - Loaded at `Bg2BackgroundPpu = 124.2f`
  - Camera uses orthographic size `4.4` and position `(1.15, 0.75, -10)` for bg2.
- The old separate `bg2_coffin_occluder.png` is currently deleted/missing, so the builder will skip that occluder layer.
- The latest verified room screenshot with the current side idle placeholder is:
  - `Assets/Screenshots/codex_side_idle_ready_alpha/side_idle_ready_alpha_in_scene.png`

## Dracula Runtime Scale And Import Values

Current constants in `CryptPrototypeBuilder`:

- `DraculaReferenceScale = 1.55f`
- `DraculaDownAlpha12Ppu = 458f`
- `DraculaRightAlphaPpu = 568f`
- `DraculaWalkFrameTime = 0.16f`
- `DraculaSideWalkFrameTime = 0.08f`
- `DraculaUpWalkFrameTime = 0.108f`
- `DraculaIdleFrameTime = 0.18f`
- `DraculaToIdleFrameTime = 0.055f`

Do not resize Dracula casually. His apparent size has been tuned against this new room, and scale changes will affect collider feel and floor fit.

## Dracula Sprite And Animation State

Runtime folders:

- Down walk: `Assets/Art/Crypt/DraculaWalkDownAlpha12/`
- Down idle: `Assets/Art/Crypt/DraculaIdleDownAlpha24/`
- Down ToIdle: `Assets/Art/Crypt/DraculaToIdleDownAlpha24/`
- Right walk: `Assets/Art/Crypt/DraculaWalkRightAlpha24/`
- Right idle: `Assets/Art/Crypt/DraculaIdleRightAlpha12/`
- Up walk: `Assets/Art/Crypt/DraculaWalkUpAlpha24/`
- Up idle: `Assets/Art/Crypt/DraculaIdleUpAlpha12/`
- Source and review sheets: `Assets/Art/Crypt/SourceSheets/`

Current sources and choices:

- Down walk comes from Selene's cleaned alpha sheet:
  - `Assets/Art/Crypt/SourceSheets/sheet-alpha.png`
  - 12 output frames: `dracula_down_alpha12_00.png` through `_11.png`
- Down idle and down ToIdle come from `D:/draculaidlenew.mp4`.
  - 24 selected frames.
  - Same strip is used for idle and ToIdle for now.
  - Head/face/upper chest were locked from frame 0 to reduce mouth movement.
  - This may still need taste review, but it is the current runtime path.
- Right walk comes from `D:/right.mp4`.
  - 24 selected frames.
  - Left walk is mirrored with `SpriteRenderer.flipX`.
  - Treat this as separate from the rejected right-idle video. It may still need visual review before final approval.
- Right idle no longer comes from video.
  - Current source is Selene's ready-made placeholder still: `D:/dracula-right.png`.
  - Source already has RGBA alpha.
  - The source was scaled and padded onto the existing right-walk `720x1280` canvas so the runtime frame size and foot alignment match the walk cycle.
  - No alpha keying, no matte removal, no black removal.
  - A slight white rim is accepted as placeholder quality for now; do not spend time on rim cleanup unless Selene asks.
  - Source copy: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_source.png`
  - Runtime canvas copy: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_on_walk_canvas.png`
  - Comparison preview: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_compare_preview.png`
  - Repeated-frame preview: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_repeated_preview_2row.png`
  - Meta: `Assets/Art/Crypt/SourceSheets/dracula_idle_right_ready_alpha_20260616_meta.json`
  - Runtime output: `Assets/Art/Crypt/DraculaIdleRightAlpha12/dracula_idle_right_alpha12_00.png` through `_11.png`
- Left idle is the right idle mirrored with `SpriteRenderer.flipX`.
- Up walk comes from `D:/dracula_up.mp4`.
  - 24 selected frames.
- Up idle comes from `C:/Users/Selene/Downloads/Subtle_idle_animation_freeze_camera_202606152258.mp4`.
  - 12 selected frames.
  - Upper cape/head area is locked from frame 0 so the idle reads as a quiet hold.

## DraculaWalker Behavior

`DraculaWalker` now supports separate walk/idle arrays:

- `walkDown`, `walkUp`, `walkSide`
- `idleDown`, `idleUp`, `idleSide`
- `toIdleDown`
- `sideFrameTime`, `upFrameTime`, `idleFrameTime`, `toIdleFrameTime`

Current behavior:

- Moving down/up/side uses direction-specific walk strips.
- Stopping from down/front movement plays `toIdleDown` once, then loops `idleDown`.
- Stopping from side movement goes directly to `idleSide`.
- Stopping from up movement loops `idleUp`.
- Left movement/idle uses the right-facing sprites mirrored with `flipX`.
- The old `[RequireComponent(typeof(CapsuleCollider2D))]` was removed.

## Player Collider State

The old body-centered/capsule-style collision was the reason wall blockers only triggered when Dracula's torso reached them.

Current generated player collider:

- Scene object: `Crypt Prototype/Dracula/Ground Contact Collider - edit size/position here`
- Type: child `BoxCollider2D`
- Generated in `CryptPrototypeBuilder`
- Local position: `(0, -1.08 / DraculaReferenceScale, 0)`
- Local size: `(0.72 / DraculaReferenceScale, 0.22 / DraculaReferenceScale)`
- Verified live after rebuild:
  - World center: `(0.420, -2.820, 0.000)`
  - World min: `(0.060, -2.930, 0.000)`
  - World max: `(0.780, -2.710, 0.000)`

Latest proof screenshot:

- `Assets/Screenshots/codex_original_still_side_idle_fix/side_idle_original_still_collider_overlay.png`
- Red box is Dracula's foot-contact collider.
- Cyan boxes are room boundary colliders.

If the player contact still feels wrong, first adjust this child foot collider in scene for testing. For durable rebuild-safe changes, edit the values in `CryptPrototypeBuilder`.

## Room Boundary Colliders

The bg2 room now uses named, editable `BoxCollider2D` objects instead of the old hidden polygon clamp.

Build-safe prefab:

- `Assets/Prefabs/Crypt/BG2ManualWalkColliders.prefab`

Scene instance after build:

- `Crypt Prototype/Isometric Crypt Room/Walk Boundary Colliders/BG2 Manual Walk Barrier Colliders`

Children:

- `01 Back Wall Barrier - move down/up`
- `02 Right Wall Barrier - move right/left`
- `03 Lower Rail Barrier - move up/down`
- `04 Lower Left Corner Barrier`
- `05 Lower Right Corner Barrier`
- `06 Door Left Post Barrier`
- `07 Door Right Post Barrier`
- `08 Left Wall Barrier`

Important:

- `DraculaWalker.walkBoundary` is deliberately set to `null` for bg2.
- The broad `minBounds` / `maxBounds` are now just a large safety clamp.
- The actual room shape is handled by the named collider objects.
- If Selene wants manual collider tweaking, tell her these exact object names.

## Rejected Or Deprecated Paths

Do not restart these unless Selene explicitly asks:

- The failed retro/Spectrum filter. It blurred lines, muddied colors, and was rejected. It should remain disabled/removed from runtime.
- The right-idle video attempts from `D:/dracula-idle-right.mp4`. They produced nodding, foot drift, missing cape, and bad keying. The runtime side idle is now the ready-made still from `D:/dracula-right.png`.
- The `D:/right.png` repeated still attempt. It used a different raw canvas/scale and was not the requested walk-compatible placeholder.
- The neutral-walk-frame side-idle placeholder from `dracula_right_alpha24_00.png`. It matched size, but Selene provided a better ready still afterward.
- Any alpha cleanup on `D:/dracula-right.png` unless Selene explicitly asks. It already has alpha and the slight white rim is accepted for placeholder use.
- Any black/dark keying on Dracula frames.
- Any coffin polish or coffin collider work as a side quest.
- Any invisible/opaque polygon collider method for the bg2 walk area. Use the named box colliders.

## Verification Evidence From This Session

Most relevant current screenshots:

- `Assets/Screenshots/codex_side_idle_ready_alpha/side_idle_ready_alpha_in_scene.png`
- `Assets/Screenshots/codex_original_still_side_idle_fix/side_idle_original_still_collider_overlay.png`

Useful older screenshots, if tracking how we got here:

- `Assets/Screenshots/codex_dracula_idle_collider_pass/`
- `Assets/Screenshots/codex_dracula_autopilot_anim_pass/`
- `Assets/Screenshots/codex_dracula_collider_foot_fix/`

Latest Unity verification completed:

- Ran `Dracula/Build Crypt Prototype`.
- Checked Unity console: no errors/warnings.
- Captured Game View side-idle proof.
- Captured collider overlay proof.
- Removed temporary proof overlay.
- Restored default front-facing `dracula_down_alpha12_00` sprite.
- Saved `Assets/Scenes/CryptPrototype.unity`.

## Likely Next Work

If continuing visually, keep it narrow:

1. Build a subtle matching side-idle animation from the current `D:/dracula-right.png` pose, or another approved source, without changing scale/canvas unexpectedly.
2. Verify the current right walk in Game View. Do not confuse it with the rejected right idle.
3. If right walk is unacceptable, generate a new wider source or use a more controlled rig workflow. Do not crop or black-key it.
4. Let Selene hand-adjust the named room boundary colliders if needed, or adjust the prefab with exact screenshot proof.
5. Only revisit room art or coffin once Selene explicitly picks that target.

## Practical MCP Notes

Unity MCP was working in this session with the instance:

- `Dracula2@7ca0cf98bcf49805`

Useful MCP actions:

- `refresh_unity` with compile request.
- `execute_menu_item` using `Dracula/Build Crypt Prototype`.
- `execute_code` for object/collider inspection.
- `manage_camera` screenshot from `game_view`.
- custom tool `read_console` for console checks.

When using `execute_code`, remember it compiles as a method body. Do not include `using` statements at the top; fully qualify types if needed.

## Current Git/Workspace Note

The workspace is intentionally dirty with many generated/untracked art folders, screenshots, docs, prefab assets, and scene/code changes. Do not run destructive git cleanup. Do not revert user or generated assets unless Selene explicitly asks.
