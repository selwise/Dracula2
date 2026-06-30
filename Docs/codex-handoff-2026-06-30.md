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

## Later 2026-06-30 Pass - Corridor Art, Renfield Scale, Wall Sconces

This later pass continued the castle-proper east gallery/right-end corridor work. The main goal shifted from rough blockout toward something that can support a pitch screenshot without pretending the whole art pipeline is solved.

### Current Scene And Builder

Primary scene:

- `Assets/Scenes/CastleProperEastGalleryPrototype.unity`

Primary builder:

- `Assets/Editor/CastleHorizontalRowPrototypeBuilder.cs`
- Menu item: `Dracula/Build Castle East Gallery Prototype`

Important scene grammar decision:

- Do not treat the right-end bay as a separate gameplay room.
- It is a visual segment inside one long horizontal corridor.
- Do not add fade transitions, door triggers, or invisible room-change triggers between horizontal segments.
- Segment breaks should be visual composition only: walls, arches, returns, pillars, props, lighting, and camera framing.

The current prototype is still generated by the editor builder. Manual edits to generated scene objects will be lost when the menu item is run again.

### Deprecated Spectrum Prototype Asset Warning

The working tree currently shows many deleted files under `Assets/Art/SpectrumPrototype/`. This was part of cleaning away old/wrong reference art. Do not restore them blindly.

However, `Assets/Editor/CryptPrototypeBuilder.cs` still references several paths under `Assets/Art/SpectrumPrototype/`, including the old room and statue-door assets. If those files remain deleted, rebuilding `CryptPrototype` from the old builder can fail until the builder is updated to the current intended crypt/background asset set.

Future cleanup target:

- Update `CryptPrototypeBuilder.cs` to stop requiring deleted SpectrumPrototype art.
- Keep only the intended current crypt background/source assets.
- Avoid reintroducing deprecated scene/background variants just to satisfy old builder paths.

### Renfield Baseline Scale

Renfield was too large and made every prop/collider/door scale judgement misleading.

Source sprite:

- `Assets/Art/Characters/Renfield/Renfield.png`
- Source size observed: `405x741`

Current import setting in the castle gallery builder:

- `RenfieldPpu = 220f`
- This matches the manually approved Renfield scale in the current saved sprite importer.
- Previous generated value was `380f`, which made Renfield too small after the manual scale review.

Implementation:

- `Assets/Editor/CastleHorizontalRowPrototypeBuilder.cs`
- `LoadRenfieldSprite()` imports with pivot `(0.5, 0)` and `RenfieldPpu`.
- Renfield transform scale remains `Vector3.one`; scale is controlled through import PPU rather than object scale.

Current screenshot for judgement:

- `Assets/Screenshots/codex_study_castle_gallery_current.png`

Current caveat:

- Renfield still uses one static sprite as all idle/walk frames.
- This is intentional placeholder behavior until real Renfield animation exists.

### Starting Character Toggle

Added/kept a simple start-character option:

- `Assets/Scripts/GameOptions.cs`

Current behavior:

- `GameOptions.startAsRenfield = true` by default.
- `StartingCharacter` returns `Renfield` or `Dracula` based on that toggle.
- The castle gallery builder creates a `GameOptions` object and wires `AdventureLoopController.gameOptions`.

Design note:

- Starting character should remain a toggle, not inferred from room/location. The user explicitly wants this because future overlap between Dracula/Renfield areas is possible.

### Collision And Transition State

The gallery prototype should not contain visual-segment transition triggers.

Current intended state:

- No doorway transition for the open arch/segment break.
- No fade transition between the right-end visual bay and the rest of the corridor.
- No up exits in the current right-end area.
- Environment colliders should stay minimal until the visible art and actor scale are settled.

Actor state:

- Renfield currently has a small child `Ground Contact Collider`, matching the temporary actor setup pattern.
- The environment itself should not be blocked up with speculative wall colliders until the layout is stable.

### Castle Wall Tile

User supplied:

- `C:\Users\selwi\Downloads\CastleWall.png`

It appeared to be a 2x preview sheet. A repeat unit was cropped and saved as:

- `Assets/Art/Castle/Tiles/wall_castle_gray_block_tile.png`

Reference/proof:

- `Docs/ArtReferences/wall_castle_gray_block_tile_unit_2x2_proof.png`

Current import/usage:

- `CastleWallTilePpu = 240f`
- Texture wrap mode: `Repeat`
- Filter mode: `Point`
- Compression: uncompressed
- The corridor builder uses `SpriteRenderer.drawMode = SpriteDrawMode.Tiled` for the wall span.

Current builder constants:

- `CastleWallTilePath = "Assets/Art/Castle/Tiles/wall_castle_gray_block_tile.png"`
- `CastleWallTilePpu = 240f`

Design caveat:

- The wall tile is a practical placeholder that makes the scene show progress.
- It is not the final castle style bible.
- The repeated brick texture reads better than the plain blockout, but it may still need a final authored wall/floor/module system.

### AI Tile/Wall Generation Lessons

Several AI tileability attempts were underwhelming. The important workflow conclusions:

- AI struggled with truly tileable walls, especially when asked to produce the full wall/floor room in one pass.
- Generated floors tended to become too uniform or structurally wrong: bathroom-like grids, unstaggered slabs, long mosaic strips, or tile lines converging incorrectly.
- Splitting floor, wall, arch modules, endcaps, and props is probably the better path.
- Use simple clean blockouts for AI inputs; noisy blockouts and random black shapes confuse generation.
- Do not expect a prompt alone to preserve the crypt/castle visual language. Use real reference imagery or existing project art as the anchor.
- ComfyUI/Schnell may still be useful for crawler/room exploration, but do not trust it to solve tileability without manual verification and 2x/3x seam proofs.

Reference/blockout files created during this exploration live under:

- `Docs/ArtReferences/`
- `Docs/ArtReferences/Candidates/`
- `Docs/ArtReferences/Deprecated/`

### Wall Sconce Candle Asset

Created a reusable wall sconce/candle prop to replace the earlier flame-only placeholder.

Final active game asset:

- `Assets/Art/Castle/Props/wall_sconce_candle.png`

Prefab:

- `Assets/Prefabs/Castle/WallSconceCandle.prefab`

Prefab builder:

- `Assets/Editor/CastleWallSconcePrefabBuilder.cs`
- Menu item: `Dracula/Create Castle Wall Sconce Prefab`

Current authored-art path:

- `SconceArtPath = "Assets/Art/Castle/Props/wall_sconce_candle.png"`

The prefab builder behavior:

- Uses the authored sconce sprite if present.
- Falls back to a simple constructed placeholder if the authored PNG is missing.
- Nests the existing `Assets/Prefabs/CandleFlame.prefab` inside the sconce prefab.
- Keeps the flame/light runtime behavior in `CandleFlame` instead of baking glow into the PNG.

Current sconce import settings:

- Sprite single
- Point filter
- Uncompressed
- Alpha transparency enabled
- `spritePixelsPerUnit = 220f`

Current nested flame settings in the builder:

- Nested flame local position: `(0, 0.56, 0)`
- Nested flame local scale: `0.2`
- `CandleFlame.flameSize = 0.62`
- `lightIntensity = 1.85`
- `lightRange = 2.15`
- `lightFlickerIntensity = 0.28`
- `animationFramerate = 5.2`
- `flameOffset = Vector2.zero`
- `positionJitter = 0.012`
- `hideBaseSpriteAtRuntime = true`

Important correction:

- Do not add a baked square/rectangular glow to the sconce prefab. It looked visibly bad against the wall.
- Let the flame prefab/light create illumination.

### Sconce Art Iterations

Generated a front-facing gothic wall sconce/candle on chroma background and cut it to alpha.

Problems found:

- The first top fleur-de-lis ornament crowded the flame.
- The candle and holder were too chunky relative to the flame.
- Increasing the whole prefab by 30 percent overshot once Renfield scale was reconsidered.

Fixes applied:

- Removed the top fleur-de-lis from the final active PNG.
- Slimmed the whole sconce/candle/backplate art horizontally to about 80 percent of its original width.
- Kept the flame itself the same size so it reads stronger relative to the prop.

Archived generation/source variants:

- `Docs/ArtReferences/SconceGeneration/wall_sconce_candle_chroma_source.png`
- `Docs/ArtReferences/SconceGeneration/wall_sconce_candle_alpha_full.png`
- `Docs/ArtReferences/SconceGeneration/wall_sconce_candle_with_fleur.png`
- `Docs/ArtReferences/SconceGeneration/wall_sconce_candle_before_slim_20260630_112208.png`
- `Docs/ArtReferences/SconceGeneration/wall_sconce_candle_before_whole_slim_20260630_112344.png`

### Sconce Placement

Current placement is generated in `BuildWallSconceRun()`:

- `sconceY = 1.12f`
- `scale = 0.94f`
- Positions:
  - `(-12.2, 1.12, 0)`
  - `(-6.25, 1.12, 0)`
  - `(-0.3, 1.12, 0)`
  - `(3.55, 1.12, 0)`

The `0.94` scale was selected after:

- `0.78` felt too small.
- `1.02` was roughly +30 percent and felt overshot.
- `0.94` is roughly +20 percent from the old `0.78`, but still should be judged against the corrected Renfield scale.

Current useful screenshots:

- `Assets/Screenshots/castle_gallery_wall_sconces_cleaned_no_square_glow.png`
- `Assets/Screenshots/castle_gallery_wall_sconces_flame_y050.png`
- `Assets/Screenshots/castle_gallery_wall_sconces_80pct_width.png`
- `Assets/Screenshots/castle_gallery_wall_sconces_scale_102.png`
- `Assets/Screenshots/codex_study_castle_gallery_current.png`

### Normal Map Direction

No normal maps were wired in this pass.

Recommendation:

- Use normals selectively, not everywhere.
- Good candidates: final stone wall tile, metal sconce/backplate, maybe final floor slabs.
- Avoid broad aggressive normal maps on pixel/painted sprites; they can make the art look rubbery or clash with the retro/painted look.
- A safe next test would be one subtle wall normal map assigned through Sprite secondary textures, then compare screenshots with candle lighting.

### Current Verification

Unity MCP was used throughout this late pass.

Verified:

- Active scene: `Assets/Scenes/CastleProperEastGalleryPrototype.unity`
- Unity version observed in this pass: `6000.5.0f1`
- Builder menu items ran:
  - `Dracula/Create Castle Wall Sconce Prefab`
  - `Dracula/Build Castle East Gallery Prototype`
- Scene contains 4 `Prototype Wall Sconce` instances.
- Final screenshot captured:
  - `Assets/Screenshots/codex_study_castle_gallery_current.png`
- Unity console reported no compile/game errors after the final rebuild.

Recurring non-game error observed in one earlier check:

- `MCP-FOR-UNITY: Client handler error: Cannot access a disposed object.`

This appeared to be a Unity MCP transport/client issue, not a project compile error.

### Current Next Best Steps

Recommended next order:

1. Treat `RenfieldPpu = 220f` as the manually approved actor scale baseline.
2. Tune sconce scale/height one final time against that actor scale.
3. Decide if the wall brick tile should stay as a pitch placeholder or be replaced by a more coherent authored wall/floor module set.
4. Replace the remaining plain floor/arch/endcap blockout pieces with authored modules one at a time.
5. Update `CryptPrototypeBuilder.cs` so it no longer depends on deleted/deprecated SpectrumPrototype assets.
6. Keep all future generated/source variants under `Docs/ArtReferences/...` and keep only final active runtime art under `Assets/Art/...`.

## Later 2026-06-30 Pass - Oblique Return Angle Correction

The first AI room-candidate attempt drifted into the wrong full-room/elevation angle. Selene clarified the intended angle with a cropped reference: a black void beside a warm, oblique stone wall return with vertical gothic columns.

Reference/check files:

- User-provided angle reference: `C:/Users/selwi/AppData/Local/Temp/codex-clipboard-42de059c-57f6-4d4b-9119-c183d8b6ca97.png`
- Previous tile-backed blockout screenshot: `Assets/Screenshots/castle_gallery_integrated_return_walltile.png`
- Current doorway/floor verification screenshot: `Assets/Screenshots/castle_gallery_doorwall_ratio_tile_match_test.png`
- Previous full-height doorwall screenshot: `Assets/Screenshots/castle_gallery_try_user_doorwall.png`
- Y-only squash comparison screenshot: `Assets/Screenshots/castle_gallery_doorwall_y78_tile_match_test.png`
- Previous top-aligned doorway screenshot: `Assets/Screenshots/castle_gallery_oblique_return_top_aligned.png`
- Previous floor-edge verification screenshot: `Assets/Screenshots/castle_gallery_floor_reaches_screen_edge.png`
- Prior doorway-only verification screenshots:
  - `Assets/Screenshots/castle_gallery_oblique_doorway_user_transparency_integrated.png`
  - `Assets/Screenshots/castle_gallery_oblique_doorway_user_transparency_scaled_integrated.png`
- Active runtime doorway art: `Assets/Art/Castle/Prototype/castle_gallery_oblique_doorway_candidate.png`
- User-supplied full rectangular wall/floor source copy: `Docs/ArtReferences/Candidates/castle_gallery_oblique_doorway_user_doorwall_20260630.png`
- User-supplied doorway source copy: `Docs/ArtReferences/Candidates/castle_gallery_oblique_doorway_user_transparency_20260630.png`
- Generated source/candidate sheets:
  - `Docs/ArtReferences/Candidates/castle_oblique_wall_doorway_sheet_20260630.png`
  - `Docs/ArtReferences/Candidates/castle_wall_doorway_sheet_20260630.png`
- Cropped reference candidates kept out of runtime:
  - `Docs/ArtReferences/Candidates/castle_gallery_oblique_doorway_tight_candidate_20260630.png`
  - `Docs/ArtReferences/Candidates/castle_gallery_oblique_doorway_regen_full_20260630.png`
  - `Docs/ArtReferences/Candidates/castle_gallery_oblique_doorway_regen_normalized_20260630.png`
  - `Docs/ArtReferences/Candidates/castle_gallery_oblique_doorway_regen_below_door_alpha_20260630.png`
  - `Docs/ArtReferences/Candidates/castle_gallery_oblique_wall_return_candidate_20260630.png`
  - `Docs/ArtReferences/Candidates/castle_gallery_wall_continuation_candidate_20260630.png`
  - `Docs/ArtReferences/Candidates/castle_gallery_doorway_candidate_20260630.png`

Builder change:

- `Assets/Editor/CastleHorizontalRowPrototypeBuilder.cs`
- `BuildSegmentBreak()` now calls `BuildObliqueGalleryReturn()` for the left visual segment/opening.
- The return now uses Selene's supplied `doorwall.png` full rectangular wall/floor panel, copied into the project as `castle_gallery_oblique_doorway_candidate.png`.
- This newest panel is opaque RGB, not an alpha cutout. It should be treated as a full rectangular art module for the current test.
- The doorway is imported as a point-filtered, uncompressed sprite at `267` PPU with `alphaIsTransparency = true`.
- The earlier straight-on doorway, plain wall continuation, transparent doorway test, and failed generated/cutout candidates were saved only under `Docs/ArtReferences/Candidates`; the builder currently uses the user-supplied opaque `doorwall.png` panel.
- The latest floor correction extends the visual floor to the bottom edge of the game view. In the current generated scene, the camera bottom is `y = -4.36`, `Hallway Floor Base` reaches `y = -4.34`, and `Hallway Front Floor Lip` reaches `y = -4.36`.
- The latest oblique return test uses non-uniform scale so the panel reads less vertically stretched and better approaches the front-facing wall tile course ratio. Current module position is `(-3.5, -0.42, 0)` and local scale is `(1.12, 0.92, 1)`.
- Current measured bounds: facing wall tile top `y = 2.940`, panel top `y = 2.943`, panel bottom `y = -3.783`, and panel size `9.002 x 6.726` world units. The right edge is kept near `x = 1.001` for the join into the existing front-facing wall.

Important direction:

- Use this oblique wall-return angle for arch/endcap/architectural slice candidates.
- Do not introduce standalone generated-image cutouts for this pass; any generated wall or doorway must be derived from and visually matched to the same gray block tile/material language as the back wall.
- Do not alpha-mask the wall/doorway module into a silhouette. Keep the upper-left/side area filled with matching masonry; alpha belongs only below/through the doorway where the opening continues.
- Do not ask imagegen for full head-on room elevations or full-room corridor concepts when the target is this kind of return detail.
- Continue one module at a time; the current oblique return should be judged before applying the same language to the east end stop or floor.

