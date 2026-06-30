# Codex Handoff - Dracula2 Lighting And Castle Row Pass - 2026-06-30

This handoff records the current state after the crypt lighting pass, Dracula readability pass, and first castle-proper horizontal row blockout.

## Read This First

- Keep all project notes, handoffs, planning docs, and durable workflow notes under `Docs/`.
- Work one visible target at a time. Do not drift from lighting to props to character art to filters unless Selene explicitly redirects.
- Do not revert user-placed scene edits. Several candle flame placements in the crypt were manually nudged by Selene and should be treated as intentional.
- Never call visual work done without a fresh Unity screenshot and console check.
- The project is intentionally dirty. Do not run destructive git cleanup or reset commands.

## Unity MCP State

Unity MCP was visible and usable this session.

- Project: `C:\Gamedev\Dracula2`
- Unity version observed: `6000.5.0f1`
- Connected instance observed: `Dracula2@7ca0cf98bcf49805`
- Current active scene after this pass: `Assets/Scenes/CastleProperEastGalleryPrototype.unity`
- Play mode was stopped before handoff.

Useful MCP habits:

- Check `mcpforunity://custom-tools` first.
- Check `mcpforunity://editor/state` before scene work.
- After script changes, refresh/compile and check console.
- Use `manage_camera` screenshots for visual verification.

## Documentation Convention

Selene prefers handoffs and durable planning/workflow docs in a project `Docs/` folder. This is now documented in:

- `Docs/README.md`
- `README.md`

Use dated handoff files such as `Docs/codex-handoff-YYYY-MM-DD.md` instead of scattering notes beside assets or in root-level scratch files.

## Scene Naming

The old scene with the awkward prototype name has been renamed in practice:

- Current crypt scene: `Assets/Scenes/CryptPrototype.unity`
- Crypt builder: `Assets/Editor/CryptPrototypeBuilder.cs`
- Crypt rebuild menu item: `Dracula/Build Crypt Prototype`

## Dust Fix

The crypt room dust had been visible in dark/black areas but not reliably over the room art. It was adjusted so dust renders over the room graphics:

- Script: `Assets/Scripts/RoomDustParticles.cs`
- Scene: `Assets/Scenes/CryptPrototype.unity`
- Current dust rendering uses sorting layer `Default`, order `15`.

Useful proof screenshots:

- `Assets/Screenshots/codex_dust_diagnostic/dust_before_fix.png`
- `Assets/Screenshots/codex_dust_diagnostic/dust_after_fix.png`
- `Assets/Screenshots/codex_dust_diagnostic/dust_after_mcp_restart.png`

## 2D Renderer And Crypt Lighting

The project is now using a real URP 2D renderer setup for the pixel/isometric scene lighting.

Added/changed:

- `Assets/Settings/PC_2D_Renderer.asset`
- `Assets/Settings/PC_RPAsset.asset`
- `Assets/Scenes/CryptPrototype.unity`

Current rendering setup:

- `PC_RPAsset` has a 2D renderer added to the renderer list.
- Default renderer index is set to the 2D renderer.
- Room and Dracula use `Sprite-Lit-Default`.
- Dust, candle flames, glows, and the ambient veil remain unlit.
- The old `Directional Light` in the crypt is disabled.
- `Global Low Ambient 2D Light` provides the low disembodied base visibility.

Added crypt lighting assets:

- `Assets/Art/Effects/Lighting/ambient_veil_pixel.png`
- `Assets/Art/Effects/Lighting/candle_warm_radial_glow.png`

Added scene lighting rig:

- `Candle Lighting Rig`
- `Low Ambient Veil`
- `Warm Local Glow Pools`
- `CryptCandleLightFlicker`

Important taste note:

- Avoid broad uniform lighting that makes the room look like a lit poster.
- Keep global/ambient light very low.
- Let candles/torches carry most perceived light.

## Candle Flame State

Selene manually placed the candle flame prefab instances in the crypt and then nudged them into place. Do not rebuild or overwrite those positions casually.

Current candle count/state:

- There are 17 candle flame prefab instances under `Candle Flames`.
- Selene removed the two side-door entries formerly called `10a` and `10b`; that is intentional.

Changed files:

- `Assets/Scripts/CandleFlame.cs`
- `Assets/Prefabs/CandleFlame.prefab`

Current `CandleFlame` defaults:

- `lightIntensity = 1.25`
- `lightRange = 1.35`
- Light2D type lookup checks `Unity.RenderPipelines.Universal.2D.Runtime` first.
- Light2D uses point-light radius properties:
  - `pointLightInnerRadius`
  - `pointLightOuterRadius`
- It sets:
  - `lightType = Point`
  - `falloffIntensity = 0.9`
  - `shadowIntensity = 0`
  - `shadowsEnabled = false`

Rejected/regressed path:

- A character-only Light2D sorting-layer experiment made candle flames render black and did not meaningfully help Dracula. It was fully reverted.
- Do not reintroduce `Characters` / `ForegroundEffects` sorting layer targeting for this purpose without a very controlled test.

Useful proof screenshots:

- `Assets/Screenshots/codex_candle_placement/sixteen_candle_flames_first_pass.png`
- `Assets/Screenshots/codex_candle_lighting/candle_lighting_first_pass.png`
- `Assets/Screenshots/codex_candle_lighting/candle_lighting_subtle_pass.png`

## Dracula Readability

Problem:

- The post-processing and low light made Dracula read too dark even when the sprite itself was already bright.
- Brightening all source frames is possible but painful and not scalable, especially because the lighting rig should not force all character art into over-bright source sprites.

Chosen current solution:

- `Assets/Scripts/CharacterReadabilityOverlay.cs`

It creates a child `Readability Overlay` sprite that tracks the source sprite, flip state, sorting layer, and sorting order. It uses a subtle unlit tinted overlay rather than a visible character light.

Current crypt scene values:

- Attached to `Spectrum Dracula Placeholder`.
- Overlay material: `Sprite-Unlit-Default`.
- Alpha: `0.17`
- Tint: approximately `0.84, 0.88, 0.96, 1`
- Sorting order offset: `+1`

Verified in Play:

- Overlay active and sprite-matched.
- Candle flames did not turn black.
- Runtime console clean.

Useful proof screenshot:

- `Assets/Screenshots/codex_candle_lighting/character_readability_overlay_final_alpha017.png`

## Castle Proper Horizontal Row Prototype

New rightmost castle-proper row blockout:

- Scene: `Assets/Scenes/CastleProperEastGalleryPrototype.unity`
- Builder: `Assets/Editor/CastleHorizontalRowPrototypeBuilder.cs`
- Rebuild menu item: `Dracula/Build Castle East Gallery Prototype`
- Blockout pixel asset: `Assets/Art/Castle/Prototype/castle_blockout_pixel.png`

New supporting scripts:

- `Assets/Scripts/HorizontalRowCameraFollow.cs`
- `Assets/Scripts/HorizontalParallaxLayer.cs`

New layout constants in:

- `Assets/Scripts/CastleRoomLayout.cs`

Constants added:

- `CastleProperEastGalleryRoomName`
- `CastleProperEastGalleryMinBounds`
- `CastleProperEastGalleryMaxBounds`
- `CastleProperEastGalleryCameraMin`
- `CastleProperEastGalleryCameraMax`
- `CastleProperEastGalleryStart`

Current design intent implemented:

- It is a horizontal row, not a single isolated room.
- The rightmost space is the only area with meaningful detail for now.
- The left side has a visible arch/opening into the continuous row.
- The right side is a hard blocking wall/end of row.
- Normal horizontal traversal does not fade or teleport.
- The camera follows horizontally and clamps across the row.
- Foreground pillars are in their own parallax layer for front occlusion tests.
- There is a disabled placeholder marker named `Future Upper Row Transition Slot`; it is not active gameplay yet.

Runtime caveat:

- This checkout only currently has `Assets/Art/Characters/Dracula/Sheets/dracula_spectrum_walk_down_sheet.png`.
- The castle row prototype uses that sheet for the temporary scale/play actor. Directional walking art still needs to be restored or replaced later.

Verification:

- Builder ran successfully.
- Scene saved and active.
- Play mode started/stopped cleanly.
- Console clean during runtime check.
- Dracula was moved left in Play mode; the camera followed/clamped instead of doing a room transition.

Useful screenshots:

- `Assets/Screenshots/castle_horizontal_row/castle_east_gallery_blockout_final.png`
- `Assets/Screenshots/castle_horizontal_row/castle_east_gallery_left_follow_playcheck.png`

## Row And Portal Design Notes

The intended castle-proper room grammar:

- Most castle rooms should be long horizontal rows, closer to `Spellbound` style.
- A row can be visually segmented with arches, pillars, and room-like bays.
- Moving left/right across a row should be seamless with camera follow.
- Camera fades/snaps should be reserved for explicit upper/lower row exits.
- Up/down exits should be visually legible through missing obstacles, thresholds, or clear openings, similar to later LucasArts adventure layout language.
- Sister locations should use explicit row-transition hotspots/portals, not invisible transitions between horizontal bays.

Existing useful portal script:

- `Assets/Scripts/CastleRoomPortal.cs`

It already supports:

- destination transform
- destination room name
- destination min/max bounds
- active actor requirement
- optional camera snap
- portal cooldown

Recommendation:

- Use `CastleRoomPortal` or a thin wrapper around it for future up/down row changes.
- Do not use it for ordinary left/right row continuation.

## Current Git/Workspace State

Known modified files from this work/session include:

- `Assets/Prefabs/CandleFlame.prefab`
- `Assets/Scenes/CryptPrototype.unity`
- `Assets/Scripts/CandleFlame.cs`
- `Assets/Scripts/CastleRoomLayout.cs`
- `Assets/Settings/PC_RPAsset.asset`

Known new/untracked paths from this work/session include:

- `Assets/Art/Castle/Prototype/`
- `Assets/Art/Effects/Lighting/`
- `Assets/Editor/CastleHorizontalRowPrototypeBuilder.cs`
- `Assets/Scenes/CastleProperEastGalleryPrototype.unity`
- `Assets/Screenshots/castle_horizontal_row/`
- `Assets/Screenshots/codex_candle_lighting/`
- `Assets/Scripts/CharacterReadabilityOverlay.cs`
- `Assets/Scripts/HorizontalParallaxLayer.cs`
- `Assets/Scripts/HorizontalRowCameraFollow.cs`
- `Assets/Settings/PC_2D_Renderer.asset`

`ProjectSettings/EditorBuildSettings.asset` was briefly changed when the new prototype scene was added to build settings, then reverted back to only the crypt scene. The castle row scene is a prototype/editor scene, not a build-settings scene.

Git status may print:

- `warning: unable to access 'C:\Users\selwi/.config/git/ignore': Permission denied`

This is unrelated to the Unity work.

## Known Console State

After the final Unity refresh, the only warning observed was the pre-existing obsolete importer warning in the old crypt builder:

- `Assets/Editor/CryptPrototypeBuilder.cs(394,9): TextureImporter.spritesheet is obsolete`

The new castle scene runtime check itself produced no console entries.

## Likely Next Work

Good next focused targets:

1. Review the castle row blockout in Unity and adjust proportions/feel.
2. Decide whether the castle row should keep the two temporary sconces or stay barer.
3. Add a proper up/down row transition prototype using `CastleRoomPortal`.
4. Restore/replace full Dracula directional animation assets for the castle row prototype.
5. Continue refining crypt lighting only after Selene reviews the current 2D renderer/candle/readability pass.

