# Manual Collider Adjustment

The bg2 room now uses ordinary editable `BoxCollider2D` objects instead of the old hidden polygon clamp.

## Where To Edit

Build-safe prefab:

- `Assets/Prefabs/Crypt/BG2ManualWalkColliders.prefab`

Current scene instance:

- `Crypt Prototype/Isometric Crypt Room/Walk Boundary Colliders/BG2 Manual Walk Barrier Colliders`

If you only need an immediate scene tweak, move the scene instance children. If you want the change to survive `Dracula/Build Crypt Prototype`, edit or apply overrides to the prefab.

Player contact collider:

- `Crypt Prototype/Dracula/Ground Contact Collider - edit size/position here`

This is a child `BoxCollider2D` placed at Dracula's feet. It replaced the old body-centered contact shape so wall and floor blockers respond at his footprint instead of waiting until his torso reaches them. If you need to hand-tune movement feel in the scene, move this child object or adjust its `BoxCollider2D.size`; if you want the change to survive a rebuild, update the default values in `Assets/Editor/CryptPrototypeBuilder.cs`.

## Collider Objects

- `01 Back Wall Barrier - move down/up`
  - Stops Dracula from walking into the back wall. Move this down if the far wall blocks too soon; move it up if he can walk into the wall art.
- `02 Right Wall Barrier - move right/left`
  - Stops walking into the right wall. Move this right if it blocks too soon; move it left if he clips into the wall.
- `03 Lower Rail Barrier - move up/down`
  - Stops walking off the bottom of the room. Move this up if his feet still leave the floor; move it down if it blocks too early.
- `04 Lower Left Corner Barrier`
  - Covers the lower-left slanted edge.
- `05 Lower Right Corner Barrier`
  - Covers the lower-right slanted edge.
- `06 Door Left Post Barrier`
  - Left side of the arch/door opening.
- `07 Door Right Post Barrier`
  - Right side of the arch/door opening.
- `08 Left Wall Barrier`
  - Left slanted wall/edge.

## Notes

- The old `DraculaWalker.walkBoundary` polygon is no longer assigned for bg2.
- The broad rectangular `minBounds` / `maxBounds` on `DraculaWalker` are now only a large safety clamp; the room shape is controlled by the named colliders.
- Dracula's player collision is controlled by the named child foot collider, not by a capsule or body-centered collider.
- Verification screenshot with temporary collider outlines: `Assets/Screenshots/codex_dracula_idle_collider_pass/manual_collider_overlay_from_actual_boxes.png`
- Latest verification screenshot with Dracula's red foot collider: `Assets/Screenshots/codex_original_still_side_idle_fix/side_idle_original_still_collider_overlay.png`
