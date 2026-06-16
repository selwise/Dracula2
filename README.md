# Dracula2

Isometric Dracula crypt prototype / vertical slice.

## Project

- Unity: 6000.4.10f1
- Main scene: `Assets/Scenes/CryptPrototype.unity`
- Input: WASD movement through `Assets/Scripts/DraculaWalker.cs`
- Art: generated low-resolution crypt sprites under `Assets/Art/Crypt`

## AI Workflow Guardrail

When iterating on the prototype, work in this order unless Selene explicitly changes it:

1. Room backdrop, camera framing, and walkable space.
2. Character placement, scale, and currently approved character art.
3. Boundary colliders and movement feel.

Do not rush ahead into coffin props, candle edits, filters, alpha cleanup, palette conversion, or other side fixes unless specifically requested. If Selene says an asset already has proper alpha or is hand-cleaned, use it directly and do not process it first.

## Git Notes

This repo uses Git LFS for binary art/media files. After cloning on a new machine, run:

```powershell
git lfs install
git lfs pull
```

Unity-generated folders such as `Library`, `Temp`, `Logs`, and `UserSettings` are intentionally ignored.
