# KerbVisionIR

KerbVisionIR is a night-vision mod for Kerbal Space Program (KSP 1.12.x).

It provides a monochrome-first visual pipeline with stable green output, plus optional vignette, grain, and scanlines.

## Features

- Monochrome-first night vision (no original scene color bleed)
- Stable green tint output through color grading mixer
- Brightness multiplier
- Optional vignette, grain, and scanlines
- Scanlines rendered below UI and under vignette ordering
- Smooth enable/disable transition
- Simple on-screen status messages: `Night Vision ON` / `Night Vision OFF`
- Toolbar integration and configurable hotkey
- Disabled automatically in Map View

## Requirements

- KSP `1.12.3+`
- `ClickThroughBlocker`
- `ToolbarController`

## Installation (manual)

1. Delete old `GameData/KerbVisionIR` folder.
2. Extract this mod to your KSP root.
3. Verify files exist:
   - `GameData/KerbVisionIR/Plugins/KerbVisionIR.dll`
   - `GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf`
   - `GameData/KerbVisionIR/Sounds/NVon.wav`

## Usage

### Default controls

- Toggle effect: `Alt + \``
- Open window: toolbar button

### In-flight settings

- Enable/Disable Night Vision
- Brightness
- Vignette ON/OFF + strength
- Grain ON/OFF + strength (smoothed slider response)
- Scanlines ON/OFF + strength
- Green Tint strength
- Key binding + Alt requirement

## Release Package Layout

```text
GameData/
  KerbVisionIR/
    Assets/
    Plugins/KerbVisionIR.dll
    Shaders/kerbvision-pp.ssf
    Sounds/NVon.wav
    KerbVisionIR.version
```

## Troubleshooting

- If effect does not appear: verify shader file exists in `GameData/KerbVisionIR/Shaders/`.
- If update behaves oddly: remove old mod folder and reinstall clean.
- If keybind seems unresponsive: check key binding and Alt modifier setting in GUI.

## License

Distributed under `GPL-3.0-or-later`. See `LICENSE`.
