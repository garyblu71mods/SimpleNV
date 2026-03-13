# Required External Files

This mod requires external files that cannot be included in the source repository:

## 1. TUFX Shader Bundle

**Required File:** `kerbvision-pp.ssf`  
**Location:** `GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf`  
**Source:** Copy `tufx-universal.ssf` from an existing TUFX installation  
**Size:** ~200KB  

### Where to get it:

1. Install TUFX from CKAN or manually
2. Find `GameData/TUFX/Shaders/tufx-universal.ssf` in your KSP installation
3. Copy it to `GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf`

OR keep the original name and update `PostProcessResources.cs` line 27:
```csharp
string shaderPath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/KerbVisionIR/Shaders/tufx-universal.ssf");
```

## 2. Activation Sound (Optional)

**Required File:** `NVon.wav`  
**Location:** `GameData/KerbVisionIR/Sounds/NVon.wav`  
**Format:** WAV audio file, mono or stereo, 44.1kHz recommended  
**Size:** < 100KB recommended  

### Creating your own sound:

1. Record or find a short "activation" sound effect (0.5-1.0 seconds)
2. Convert to WAV format
3. Place in `GameData/KerbVisionIR/Sounds/NVon.wav`

**Note:** The mod will work without this file, but you'll see a warning in KSP.log

## Directory Structure

```
GameData/KerbVisionIR/
├── Plugins/
│   └── KerbVisionIR.dll
├── Shaders/
│   └── kerbvision-pp.ssf          ← COPY FROM TUFX
├── Sounds/
│   └── NVon.wav                   ← OPTIONAL
├── PluginData/
│   └── settings.cfg               ← AUTO-GENERATED
└── KerbVisionIR.version
```

## Testing Shader Loading

After copying the shader bundle, launch KSP and check `KSP.log` for:

```
[KerbVisionIR] Uber shader loaded: Hidden/PostProcessing/Uber, Supported=True
[KerbVisionIR] Copy shader loaded: Hidden/PostProcessing/Copy, Supported=True
```

If you see `Supported=False` or errors, the shader bundle is incorrect.

## Intel GPU Compatibility

The TUFX shader bundle is verified to work on Intel GPUs without spam.
**DO NOT** use custom shaders or Unity's standard post-processing shaders - they will cause "unsupported" spam.
