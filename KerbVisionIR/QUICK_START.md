# KerbVisionIR - Quick Start Guide

## 🚀 Fast Track to Testing (5 minutes)

### Step 1: Get the TUFX Shader (Required!)

```powershell
# Find your KSP installation with TUFX installed
# Copy the shader bundle:
Copy-Item "C:\Path\To\KSP\GameData\TUFX\Shaders\tufx-universal.ssf" `
          ".\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
```

**Don't have TUFX?**
- Install it from CKAN first, or
- Download TUFX manually and extract the shader bundle

### Step 2: Build

```powershell
# Set your KSP path
$env:KSPRoot = "C:\Program Files\Epic Games\KerbalSpaceProgram\English"

# Build
.\build.ps1
```

### Step 3: Deploy to KSP

```powershell
# Create the folder structure in KSP
New-Item -ItemType Directory -Force -Path "$env:KSPRoot\GameData\KerbVisionIR\Plugins"
New-Item -ItemType Directory -Force -Path "$env:KSPRoot\GameData\KerbVisionIR\Shaders"

# Copy DLL
Copy-Item "bin\Release\net4.8\KerbVisionIR.dll" `
          "$env:KSPRoot\GameData\KerbVisionIR\Plugins\"

# Copy shader bundle
Copy-Item "GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf" `
          "$env:KSPRoot\GameData\KerbVisionIR\Shaders\"

# Copy version file
Copy-Item "GameData\KerbVisionIR\KerbVisionIR.version" `
          "$env:KSPRoot\GameData\KerbVisionIR\"
```

### Step 4: Test in KSP

1. **Launch KSP**
2. **Load any flight scene** (existing save or sandbox)
3. **Press Alt + `** (backtick key, usually next to '1')
4. **See the effect!** Green tint, dark corners, increased brightness

### Step 5: Verify Success

Open `KSP.log` and search for `[KerbVisionIR]`:

```
✅ [KerbVisionIR] Initializing...
✅ [KerbVisionIR] Settings loaded from...
✅ [KerbVisionIR] Uber shader loaded: Hidden/PostProcessing/Uber, Supported=True
✅ [KerbVisionIR] Copy shader loaded: Hidden/PostProcessing/Copy, Supported=True
✅ [KerbVisionIR] PostProcessLayer added to camera
✅ [KerbVisionIR] Post-processing initialized
✅ [KerbVisionIR] Effect enabled
```

**❌ If you see errors:**
- `Shader bundle not found` → Copy the shader file again
- `Failed to load Uber shader` → Wrong shader bundle (must be from TUFX)
- `ShaderProgram is unsupported` (spam) → You're using the wrong shader!

## ⌨️ Controls

| Key | Action |
|-----|--------|
| `Alt + `` | Toggle night vision on/off |
| `Alt + F8` | Open settings window |

## 🎨 Default Settings

- **Mode:** Green Night Vision
- **Brightness:** 1.5x
- **Vignette:** 0.4 intensity, 0.3 smoothness
- **Saturation:** -0.5 (desaturated)
- **Contrast:** +0.2
- **Grain:** Enabled, 0.3 intensity

## 🔧 Adjust Settings

Press `Alt + F8` to open the settings window:

1. **Toggle Effect** - On/off checkbox
2. **Cycle Vision Mode** - Click button (Monochrome → Green → Amber → repeat)
3. **Adjust Sliders** - Changes apply in real-time
4. **Reset to Defaults** - Restore original values
5. **Save & Close** - Persists settings to config file

## 🎯 Testing Checklist

- [ ] Shader loads without errors
- [ ] No "unsupported" spam in log
- [ ] Vignette visible (dark corners)
- [ ] Brightness increases scene lighting
- [ ] Saturation desaturates colors
- [ ] Green tint applied in GreenNV mode
- [ ] Hotkey toggles effect
- [ ] Settings window opens
- [ ] Settings persist across restarts
- [ ] Performance > 60 FPS

## 💡 Tips

### Best Settings for Different Scenes

**Dark Side of Mun:**
- Brightness: 2.0
- Mode: Green NV
- Vignette: 0.5

**Duna Twilight:**
- Brightness: 1.5
- Mode: Amber
- Vignette: 0.3

**Low Orbit (night):**
- Brightness: 1.2
- Mode: Monochrome
- Grain: Disabled (for cleaner view)

### Performance

If you experience FPS drops:
1. Disable Grain effect (saves render time)
2. Lower KSP graphics settings
3. Close other mods that use post-processing

## 🐛 Common Issues

### "Main camera not found"
- Load a flight scene first (VAB/SPH won't work)
- Make sure you're not in the main menu

### No visual effect
- Check that effect is enabled (Alt + `)
- Increase Vignette Intensity to 0.5+
- Set Saturation to -0.8 for more dramatic effect
- Try different vision modes

### Settings not saving
- Check that GameData/KerbVisionIR/PluginData/ folder exists
- Make sure KSP has write permissions
- Look for errors in KSP.log

## 📊 Expected Results

### Before (Night Scene):
- Dark terrain
- Hard to see details
- Black sky

### After (Green NV Active):
- Bright green-tinted scene
- Visible terrain details
- Dark vignette around edges
- Slight film grain texture

## 🎉 Success!

If you can see the green-tinted effect with dark corners and increased brightness, **congratulations!** The mod is working perfectly.

Now you can:
- Explore dark side of celestial bodies
- Night operations on Kerbin
- Low-light IVA views
- Atmospheric entry at night

Enjoy your night vision! 🌙✨

---

**Still having issues?** Check `PROJECT_SUMMARY.md` for troubleshooting or review `REQUIRED_FILES.md` for file requirements.
