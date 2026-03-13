# ?? W?ASNE SHADERY - Kompletny Przewodnik

## ? Co masz teraz:

Stworzy?em dla Ciebie **5 shaderów HLSL**:

1. **UberShader.shader** - Master shader (vignette + color + grain)
2. **CopyShader.shader** - Simple blit/copy
3. **VignetteShader.shader** - Tylko vignette (opcjonalny)
4. **ColorGradingShader.shader** - Tylko color grading (opcjonalny)
5. **GrainShader.shader** - Tylko grain (opcjonalny)

Plus:
- **KerbVisionShaderBundleBuilder.cs** - Automatyczny builder

## ?? Jak u?y? (30 min):

### KROK 1: Setup Unity Projekt (5 min)

1. Otwórz Unity Hub
2. **Installs** ? Dodaj Unity 2019.2.21f1 (je?li nie masz)
3. **New Project**:
   - Template: **3D**
   - Name: `KerbVisionIR-Shaders`
   - Create

### KROK 2: Dodaj shadery (3 min)

W Unity Project window:

```
1. Assets ? Create ? Folder ? "Shaders"
2. Assets ? Create ? Folder ? "Editor"
3. Skopiuj pliki:
   - UberShader.shader ? Assets/Shaders/
   - CopyShader.shader ? Assets/Shaders/
   - VignetteShader.shader ? Assets/Shaders/ (opcjonalny)
   - ColorGradingShader.shader ? Assets/Shaders/ (opcjonalny)
   - GrainShader.shader ? Assets/Shaders/ (opcjonalny)
   - KerbVisionShaderBundleBuilder.cs ? Assets/Editor/
```

**JAK SKOPIOWA?:**
```powershell
# Z folderu KerbVisionIR/Shaders do Unity Assets/Shaders
$source = "C:\Users\grzeg\Desktop\...\KerbVisionIR\Shaders"
$unityProject = "C:\Users\grzeg\Desktop\KerbVisionIR-Shaders\Assets"

Copy-Item "$source\*.shader" "$unityProject\Shaders\" -Force
Copy-Item "$source\KerbVisionShaderBundleBuilder.cs" "$unityProject\Editor\" -Force
```

### KROK 3: Weryfikacja (1 min)

W Unity Project window sprawd?:

```
Assets/
??? Shaders/
?   ??? UberShader.shader ?
?   ??? CopyShader.shader ?
?   ??? ... (inne opcjonalne)
??? Editor/
    ??? KerbVisionShaderBundleBuilder.cs ?
```

Kliknij na ka?dy shader - w Inspector powinno by?:
- **Shader:** Hidden/KerbVision/Uber (lub Copy)
- **Properties:** (lista parametrów)

### KROK 4: Zbuduj Bundle (1 min)

W Unity:
```
Menu: Assets ? Build KerbVision Shader Bundle
```

**Console powinno pokaza?:**
```
=== Building KerbVision Shader Bundle ===
? Adding shader: Hidden/KerbVision/Uber (Assets/Shaders/UberShader.shader)
? Adding shader: Hidden/KerbVision/Copy (Assets/Shaders/CopyShader.shader)
Found 2 shaders to bundle
Building to: C:/.../AssetBundles
? Bundle built successfully!
   Location: C:/.../AssetBundles/kerbvision-pp.ssf
   Size: XXX KB
   Shaders: 2
```

Folder Explorer otworzy si? automatycznie z `kerbvision-pp.ssf`!

### KROK 5: Test Bundle (opcjonalny, 1 min)

Przed kopiowaniem do KSP, przetestuj:

```
Menu: Assets ? Test Load KerbVision Bundle
```

**Console powinno pokaza?:**
```
=== Testing Bundle Load ===
? Bundle loaded successfully
? Uber shader loaded: Hidden/KerbVision/Uber
  Supported: True
? Copy shader loaded: Hidden/KerbVision/Copy
  Supported: True
```

### KROK 6: Skopiuj do KerbVisionIR (2 min)

**PowerShell:**
```powershell
cd "C:\Users\grzeg\Desktop\...\KerbVisionIR"
.\copy-shader-bundle.ps1
```

LUB r?cznie:
```powershell
$bundle = "C:\Users\grzeg\Desktop\KerbVisionIR-Shaders\AssetBundles\kerbvision-pp.ssf"
$dest1 = "C:\Users\grzeg\Desktop\...\KerbVisionIR\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
$dest2 = "C:\Program Files\Epic Games\KerbalSpaceProgram\English\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"

Copy-Item $bundle $dest1 -Force
Copy-Item $bundle $dest2 -Force
```

### KROK 7: Napraw kod KerbVisionIR (5 min)

Musimy zmieni? nazwy shaderów w kodzie!

Otwórz `PostProcessResources.cs` i zmie?:

```csharp
// STARE (NIE ISTNIEJE):
uberShader = bundle.LoadAsset<Shader>("Hidden/PostProcessing/Uber");
copyShader = bundle.LoadAsset<Shader>("Hidden/PostProcessing/Copy");

// NOWE (NASZE):
uberShader = bundle.LoadAsset<Shader>("Hidden/KerbVision/Uber");
copyShader = bundle.LoadAsset<Shader>("Hidden/KerbVision/Copy");
```

### KROK 8: Przebuduj KerbVisionIR (2 min)

```powershell
cd "C:\Users\grzeg\Desktop\...\KerbVisionIR"
$env:KSPRoot = "C:\Program Files\Epic Games\KerbalSpaceProgram\English"
dotnet build KerbVisionIR.csproj -c Release
Copy-Item "bin\Release\net4.8\KerbVisionIR.dll" "C:\Program Files\...\GameData\KerbVisionIR\Plugins\" -Force
```

### KROK 9: Test w KSP! (5 min)

1. **ZRESTARTUJ KSP** (wa?ne!)
2. Flight mode
3. Alt + F8 (GUI)

**Sprawd? logi:**
```powershell
Get-Content "C:\Program Files\...\KSP.log" | Select-String "KerbVision"
```

**Oczekiwane:**
```
[LOG] [KerbVisionIR] Found 2 shaders in bundle:
[LOG] [KerbVisionIR]   Shader: Hidden/KerbVision/Uber
[LOG] [KerbVisionIR]   Shader: Hidden/KerbVision/Copy
[LOG] [KerbVisionIR] Uber shader loaded: Hidden/KerbVision/Uber, Supported=True
[LOG] [KerbVisionIR] Post-processing initialized (FULL MODE with effects)
```

**W GUI:**
- ? "Version 1.0.0 - FULL MODE"
- ? Slidery aktywne (nie DISABLED)

**Test efektów:**
- ? Vignette Intensity 0.8 ? ciemne rogi
- ? Saturation -1.0 ? czarno-bia?y
- ? Contrast 0.5 ? wi?cej kontrastu
- ? Grain Intensity 0.5 ? widoczne ziarno

## ?? GRATULACJE!

Masz **w?asne shadery** dzia?aj?ce w KerbVisionIR!

## ?? Customizacja

### Zmiana parametrów shadera:

Edytuj `UberShader.shader`:

```hlsl
// Zmie? domy?lne warto?ci:
_VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.6  // by?o 0.4
_VignetteSmoothness ("Vignette Smoothness", Range(0.01, 1)) = 0.2  // by?o 0.3
```

Zbuduj ponownie:
```
Assets ? Build KerbVision Shader Bundle
```

Skopiuj i test!

### Dodanie nowego efektu:

1. Dodaj property w shader:
```hlsl
_Glow ("Glow Amount", Range(0, 1)) = 0
```

2. Dodaj w fragment shader:
```hlsl
// Apply glow
col.rgb += _Glow * 0.1;
```

3. Rebuild bundle
4. Dodaj slider w C# (`VisionSettings.cs`)
5. Testuj!

## ?? Ró?nice mi?dzy shaderami:

| Plik | Co robi | Kiedy u?y? |
|------|---------|------------|
| **UberShader.shader** | Wszystko razem | Produkcja (szybsze) |
| **VignetteShader.shader** | Tylko vignette | Debugowanie |
| **ColorGradingShader.shader** | Tylko color | Debugowanie |
| **GrainShader.shader** | Tylko grain | Debugowanie |
| **CopyShader.shader** | Tylko blit | Pomocniczy |

## ?? Troubleshooting

### Shader "unsupported" w KSP
**Sprawd?:**
- Target: StandaloneWindows64 (nie WebGL, Android!)
- Unity 2019.2.x (nie 2020+)
- Shader compiles bez b??dów w Unity

### Bundle 0 KB
**Fix:**
- Sprawd? Console w Unity na b??dy
- Shadery musz? by? w `Assets/Shaders/`
- Nazwy: `Hidden/KerbVision/...`

### Nadal FALLBACK MODE
**Fix:**
- Sprawd? nazw? w `LoadAsset<Shader>()`
- Musi by? `"Hidden/KerbVision/Uber"` nie `"Hidden/PostProcessing/Uber"`
- Zrestartuj KSP

### Efekty nie widoczne
**Fix:**
- Zwi?ksz parametry (Intensity > 0.5)
- Sprawd? czy shader jest `Supported=True` w logu
- Test na czystej scenie (bez innych modów)

## ?? Nast?pne kroki

Chcesz doda?:
- **Blur?** Dodaj multi-pass z offsetami
- **Chromatic Aberration?** Offset R/G/B channels
- **Depth of Field?** Potrzebujesz depth texture
- **Motion Blur?** Potrzebujesz previous frame

Mog? pomóc z ka?dym z tych efektów!

## ?? Zasoby do nauki

- **Unity Shader Basics:** https://docs.unity3d.com/Manual/SL-Reference.html
- **HLSL Reference:** https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/
- **Cg Programming:** http://developer.download.nvidia.com/CgTutorial/cg_tutorial_chapter01.html

---

**Twoje shadery, Twoja kontrola!** ???
