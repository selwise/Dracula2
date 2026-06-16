# Retro Pixel Filter — Design Spec

Date: 2026-06-15
Status: Approved (verbal), implementing

## Goal

A toggleable, real-time post-processing filter that makes the live game render
look like the reference mockup: a clean DOS/EGA pixel-art adventure look —
chunky pixels, a tight-but-roomy limited palette, and ordered dithering in
gradients. **Primary quality bar: clean, not muddy, and no crushed blacks.**

The prior attempt (a posterization variant) crushed blacks and looked muddy.
This design specifically avoids the three failure modes that cause that.

## Anti-failure measures (the whole point)

1. **No channel posterization.** We do *nearest-color matching* against a
   designed palette, not `floor(color * levels)`. Truncation pushes darks to 0;
   nearest-match snaps to the closest *designed* tone and never biases downward.
2. **Generous dark ramp.** The palette includes several distinct near-black
   tones (blue/brown/teal/grey), so shadow detail survives instead of collapsing
   to pure black.
3. **Perceptual (gamma-space) matching.** Project is Linear color space. Doing
   the quantize/dither in linear clumps dark values together (crushed blacks).
   We convert the sampled color linear->gamma, dither + match in gamma space
   (where human vision spreads out darks), then convert the matched color back
   to linear before writing. This is the single biggest crush-proofing step.
4. **Crisp, not smeared.** Point sampling for pixelation; restrained, tunable
   ordered dithering so gradients break cleanly instead of turning to noise.

## Components

1. **`Assets/Rendering/RetroFilter/RetroFilter.shader`** (`Hidden/RetroFilter`)
   Fullscreen URP blit shader. Per fragment:
   - Pixelate: snap UV to a virtual grid (`_PixelHeight` tall, aspect-correct),
     point-sample.
   - linear -> gamma (sRGB) conversion.
   - 8x8 Bayer ordered dither offset (per virtual-pixel), scaled by
     `_DitherStrength` (subtle by default).
   - Nearest-color match against `_PaletteTex` (N colors) using a redmean
     perceptual distance.
   - gamma -> linear conversion, output.

2. **`RetroFilterRendererFeature.cs`** — `ScriptableRendererFeature` + RenderGraph
   `ScriptableRenderPass` (URP 17). Blits camera color through the material at
   `AfterRenderingPostProcessing`. Builds the palette `Texture2D` (RGBA32, point,
   linear flag so authored gamma values pass through untouched) from a serialized
   `Color[]`. Exposes `_PixelHeight`, `_DitherStrength`, palette. Holds
   `public static bool Enabled` — when false, no pass is enqueued (true bypass,
   zero cost; not a passthrough blit).

3. **`RetroFilterController.cs`** — auto-instantiated via
   `[RuntimeInitializeOnLoadMethod]` (no scene edits needed). Reads **F** (new
   Input System, matching `DraculaWalker`) to flip `RetroFilterRendererFeature.Enabled`.
   Persists state in `PlayerPrefs`. Default OFF.

4. **`Editor/RetroFilterInstaller.cs`** — `[MenuItem]` that adds the feature to
   `PC_Renderer` and `Mobile_Renderer` UniversalRendererData (idempotent), so
   wiring is reproducible and versioned rather than hand-edited YAML.

## Palette (~48 colors)

Reference mood, grouped into ramps with a deliberately deep dark end:
- black -> brown -> amber -> gold -> pale-yellow (dominant wood/line tones)
- red ramp (cape, wax seals)
- cyan/teal ramp (crew figures, water highlights)
- blue night ramp (sea, sky)
- neutrals (several distinct darks, greys, white)

Authored as sRGB/gamma hex values. Tuned live against the Crypt scene via
Unity MCP screenshots.

## Settings / defaults

- Hotkey: **F**
- `_PixelHeight`: default ~240 (tunable; user noted may go higher). 16:9 -> 426x240.
- `_DitherStrength`: subtle default, exposed 0..1.
- Injection: `AfterRenderingPostProcessing`.
- UI: screen-space-overlay UGUI is drawn after URP, so the HUD stays sharp
  (intended). Pixelating the UI too would be a separate, more invasive change.

## Render target

Universal Renderer (`PC_Renderer`, Forward+), NOT the 2D Renderer — so a standard
fullscreen ScriptableRendererFeature is the correct attachment point (an SSAO
feature already lives there).

## Verification

Tune and verify in-engine via MCP: set `Enabled` true/false, screenshot the
camera, compare. Confirm: no crushed blacks (shadow detail visible), crisp pixel
edges, gradients dither cleanly, palette reads as the reference mood.
