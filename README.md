# Dracula2

Isometric Dracula crypt prototype / vertical slice.

## Project

- Unity: 6000.4.10f1
- Main scene: `Assets/Scenes/CryptPrototype.unity`
- Input: WASD movement through `Assets/Scripts/DraculaWalker.cs`
- Art: generated low-resolution crypt sprites under `Assets/Art/Crypt`

## Git Notes

This repo uses Git LFS for binary art/media files. After cloning on a new machine, run:

```powershell
git lfs install
git lfs pull
```

Unity-generated folders such as `Library`, `Temp`, `Logs`, and `UserSettings` are intentionally ignored.
