# Castle Gallery Floor Placement Note - 2026-07-01

Scene: `Assets/Scenes/CastleProperEastGalleryPrototype.unity`

The current floor solution is intentionally a bit jerry-rigged. Do not "fix" it by resetting the transform or replacing it with generic tiled bathroom-style stones. The visible floor perspective is being carried by the authored floor image plus Unity transform placement.

## Current Floor Object

The active visual floor is:

`Hallway Deep Background`

Despite the name, this is currently the floor plate in the gallery scene.
It now owns a `CastleGalleryFloorTiler` component in the Inspector. Use that component to change the number of continuation plates instead of hand-duplicating or scaling floor objects.

- Parent: `Painted Corridor Blockout`
- Sprite: `Assets/Art/Castle/Tiles/Gemini_Generated_Image_d1dr12d1dr12d1dr.jpg`
- Draw mode: `Simple`
- Tile mode: `Continuous`
- Renderer size: `(1.00, 1.00)`
- Sorting: `Default / -50`
- Color: `(1, 1, 1, 1)`

Current transform:

- Local position: `(-13.78, -3.61, 0.00)`
- Local rotation: `(354.74, 0.00, 0.00)`
- Local scale: `(20.70, 7.95, 1.00)`
- World bounds min: `(-18.68, -5.56, -0.61)`
- World bounds max: `(-7.42, -1.24, -0.03)`

Sprite/import facts observed live:

- Texture size: `2048 x 2048`
- Sprite rect: `(0, 0, 2048, 2048)`
- Pixels per unit: `3500`
- Sprite bounds: `(0.59, 0.59, 0.20)`

## Placement Rule

The floor should keep the current authored angle relationship. If more floor coverage is needed toward the right/end-room area, do not enlarge individual stones or change the existing placed object just to fill space.

Preferred next steps:

1. Select `Hallway Deep Background`.
2. In `CastleGalleryFloorTiler`, set `Tiles Right` / `Tiles Left`.
3. Leave `Live Update In Edit Mode` enabled when you want generated copies to follow the source tile as it moves, rotates, scales, or changes renderer settings.
4. Press `Sync / Rebuild Tiles` only when you want to force a refresh.
5. Disable `Live Update In Edit Mode` only if you intentionally want to hand-adjust a generated tile, then use `Capture Step From R01` to store that custom offset.
6. Do not change the existing source plate's size, scale, rotation, sprite, or draw mode just to increase coverage.
7. Keep the existing floor object as the reference sample until the replacement is visibly better.
8. Avoid uniform square tile grids; the floor needs irregular medieval/gothic stone slabs matching the wall detail level.

## Current Continuation Tile

`Hallway Deep Background Tile R01` and `Hallway Deep Background Tile R02` were added as separate right-side continuation objects. They duplicate the current floor plate and are placed by one source-tile local width per step, not by stretching the original.

Inspector component:

- Component: `CastleGalleryFloorTiler`
- Attached to: `Hallway Deep Background`
- Source Renderer: `Hallway Deep Background` SpriteRenderer
- Tiles Left: `0`
- Tiles Right: `2`
- Tile Name Prefix: `Hallway Deep Background Tile`
- Effective Step: `(12.11, 0.00, 0.00)`
- Step override: `(0.00, 0.00, 0.00)` means derive the step from the source sprite width and source local scale.
- Live Update In Edit Mode: enabled. Generated tiles are inspector-managed and should follow the source tile automatically in edit mode.

- Source object: `Hallway Deep Background`
- Continuation object: `Hallway Deep Background Tile R01`
- Second continuation object: `Hallway Deep Background Tile R02`
- Source local position: `(-13.78, -3.61, 0.00)`
- R01 local position: `(-1.67, -3.61, 0.00)`
- R02 local position: `(10.44, -3.61, 0.00)`
- Shared local rotation: `(354.74, 0.00, 0.00)`
- Shared local scale: `(20.70, 7.95, 1.00)`
- Offset rule: next tile local X = previous tile local X + `12.1125`
- Source world X bounds: `-18.68` to `-7.42`
- R01 world X bounds: `-7.42` to `3.84`
- R02 world X bounds: `3.84` to `15.09`

## Nearby Anchors

Use these live objects as visual alignment anchors when checking a new floor plate:

- Back wall: `Hallway Castle Gray Block Wall Tile`
  - Local position: `(-5.11, 1.24, 0.00)`
  - Local scale: `(1.00, 1.00, 1.00)`
  - Sprite: `Assets/Art/Castle/Tiles/wall_castle_gray_block_tile.png`
  - Draw mode: `Tiled`
  - Renderer size: `(26.40, 4.93)`

- Oblique doorway wall: `Open Arch Oblique Doorway Candidate`
  - Local position: `(-5.18, -0.33, 5.73)`
  - Local rotation: `(0.00, 304.92, 0.00)`
  - Local scale: `(1.32, 1.00, 1.13)`
  - Sprite: `Assets/Art/Castle/Prototype/castle_gallery_oblique_doorwall_user_exact.png`

- End-room/right-side blockout: `East End Dark Wall Face`
  - Local position: `(7.42, 0.02, 0.00)`
  - Local scale: `(1.02, 6.80, 1.00)`

- Character scale check: `Renfield Placeholder`
  - Local position: `(4.90, -1.93, 0.00)`
  - Local scale: `(1.00, 1.00, 1.00)`

## Caution

The existing setup may look odd numerically because the source sprite has a very high PPU and the scene uses scale/rotation to place it. Preserve the visual result over tidy transform values unless the user explicitly asks for a rebuild.
