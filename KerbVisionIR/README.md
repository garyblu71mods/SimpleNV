# KerbVisionIR - Night Vision for KSP

A lightweight night vision mod for Kerbal Space Program 1.8+ based on TUFX PostProcessing Stack.

## Features

- **3 Vision Modes**: Monochrome, Green Night Vision, Amber/Warm
- **Brightness Control**: 0-2.0 range for lighting boost
- **Post-Processing Effects**: Vignette, Color Grading (saturation/contrast/tint), optional Grain
- **Simple GUI**: Settings window with real-time adjustments
- **Hotkey Toggle**: Alt + ` (backtick) to toggle effect
- **Audio Feedback**: Activation sound when toggling
- **Intel GPU Compatible**: Uses TUFX shaders (no "unsupported" spam)

## Installation

1. Extract the `GameData/KerbVisionIR` folder to your KSP installation's `GameData` directory
2. **IMPORTANT**: Copy `tufx-universal.ssf` from an existing TUFX installation to:
   ```
   GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf
   ```
   (You can rename it to `kerbvision-pp.ssf` or keep it as `tufx-universal.ssf`, just update the path in `PostProcessResources.cs`)

## Building from Source

### Prerequisites

- .NET Framework 4.8 SDK
- KSP 1.8+ installation
- Set the `KSPRoot` environment variable to your KSP installation path

### Build Steps

1. Set KSP path:
   ```powershell
   $env:KSPRoot = "C:\Program Files\Epic Games\KerbalSpaceProgram\English"
   ```

2. Build the project:
   ```powershell
   cd KerbVisionIR
   dotnet build -c Release
   ```

3. The compiled DLL will be in `bin/Release/net4.8/KerbVisionIR.dll`

4. Copy to KSP:
   ```powershell
   Copy-Item bin\Release\net4.8\KerbVisionIR.dll GameData\KerbVisionIR\Plugins\
   ```

## Usage

### In-Flight Controls

- **Alt + `** (backtick): Toggle night vision on/off
- **Alt + F8**: Open settings window

### Settings Window

- **Effect Enabled**: Master on/off toggle
- **Vision Mode**: Cycle through Monochrome/Green/Amber
- **Brightness**: Adjust scene lighting (0-2.0)
- **Vignette Intensity**: Dark corners effect (0-1.0)
- **Vignette Smoothness**: Softness of vignette (0.01-1.0)
- **Saturation**: Color saturation (-1 to 1)
- **Contrast**: Image contrast (-1 to 1)
- **Grain/Noise**: Optional film grain effect
- **Reset to Defaults**: Restore default settings
- **Save & Close**: Save settings and close window

## Configuration

Settings are saved automatically to:
```
GameData/KerbVisionIR/PluginData/settings.cfg
```

You can manually edit this file to change the hotkey or other settings.

## Troubleshooting

### Shader Not Loading

If you see errors like "Shader bundle not found":
1. Make sure `kerbvision-pp.ssf` exists in `GameData/KerbVisionIR/Shaders/`
2. Check that the file is from TUFX (it should be ~200KB)
3. Verify the path in the code matches your file name

### "ShaderProgram is unsupported" Spam

This should **NOT** happen if using TUFX shaders. If you see this:
1. You're not using the correct shader bundle
2. Check KSP.log for shader loading messages
3. Make sure you copied `tufx-universal.ssf` from a working TUFX installation

### No Visual Effect

1. Check that the effect is enabled (Alt + `)
2. Open the settings window (Alt + F8) and verify settings
3. Check KSP.log for error messages
4. Try adjusting Vignette Intensity and Saturation

### Performance Issues

- Disable Grain effect (it's optional)
- Lower image quality settings in KSP
- This mod is designed to be lightweight, but post-processing has some overhead

## Technical Details

### Architecture

- **Minimal TUFX Subset**: Only includes 13 core TUFX files (7 core, 3 effects, 3 utils)
- **Standalone**: No dependencies on full TUFX installation (though shader bundle is required)
- **Namespace**: All TUFX code is under `KerbVisionIR.PostProcessing` to avoid conflicts
- **Effects Used**: Vignette, ColorGrading, Grain (simplified versions)

### Files Included from TUFX

```
PostProcessing/
├── ParameterOverride.cs
├── PostProcessEffectSettings.cs
├── PostProcessEffectRenderer.cs
├── PostProcessRenderContext.cs
├── PostProcessProfile.cs
├── PostProcessResources.cs (modified for KerbVisionIR shader path)
├── PostProcessLayer.cs (simplified)
├── Effects/
│   ├── Vignette.cs
│   ├── ColorGrading.cs (simplified)
│   └── Grain.cs (simplified)
└── Utils/
    ├── PropertySheet.cs
    ├── PropertySheetFactory.cs
    └── RuntimeUtilities.cs (simplified)
```

### Shader Loading

The mod loads shaders from an AssetBundle at:
```
GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf
```

Required shaders:
- `Hidden/PostProcessing/Uber` - Main post-processing shader
- `Hidden/PostProcessing/Copy` - Copy/blit shader

## Known Issues

- Audio file (`NVon.wav`) is not included yet - you'll see a warning but the mod will work
- Grain effect uses a simplified implementation (no texture baker)
- Some advanced color grading features from TUFX are not included

## Credits

- Based on TUFX Post-Processing Stack by Shadowmage
- Inspired by various night vision mods

## License

This mod includes modified code from TUFX. Please respect the original TUFX license.

## Version History

### 1.0.0 (2024)
- Initial release
- 3 vision modes
- Brightness, vignette, color grading, grain effects
- Hotkey toggle and settings GUI
