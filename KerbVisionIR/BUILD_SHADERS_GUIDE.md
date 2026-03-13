# 🎨 PRZEWODNIK: Budowanie PostProcessing Shaderów

## 📋 Wymagania

- **Unity Hub** (najnowszy)
- **Unity 2019.2.21f1** (dokładnie ta wersja - zgodna z KSP 1.8+)
- **~2GB** wolnego miejsca
- **~4-8h** czasu (w tym nauka Unity)

## 🚀 KROK 1: Instalacja Unity

### 1.1 Pobierz Unity Hub
```
https://unity.com/download
```

### 1.2 Zainstaluj Unity 2019.2.21f1
1. Otwórz Unity Hub
2. **Installs** → **Add**
3. **Archive** → **download archive**
4. Wybierz **Unity 2019.x** → **2019.2.21f1**
5. Zaznacz komponenty:
   - ☑ **Windows Build Support**
   - ☐ Android/iOS/Mac (niepotrzebne)

**Czas:** ~20-30 min (pobieranie + instalacja)

## 🎯 KROK 2: Stwórz Projekt Unity

### 2.1 Nowy projekt
1. Unity Hub → **Projects** → **New**
2. **Template:** 3D (Built-in Render Pipeline)
3. **Project name:** `KerbVisionIR-Shaders`
4. **Location:** gdziekolwiek (np. Desktop)
5. **Create**

### 2.2 Czekaj na inicjalizację
- Unity otworzy projekt (~2-3 min)
- Zobaczysz Scene view, Hierarchy, Project

## 📦 KROK 3: Zainstaluj PostProcessing Stack V2

### 3.1 Metoda A: Package Manager (zalecane)
```
1. Menu: Window → Package Manager
2. Kliknij [+] w lewym górnym rogu
3. Add package from git URL...
4. Wklej: com.unity.postprocessing
5. Add
```

### 3.2 Metoda B: Asset Store (alternatywa)
```
1. Menu: Window → Asset Store
2. Wyszukaj: "Post Processing Stack V2"
3. Download → Import
```

### 3.3 Weryfikacja
W **Project** window powinieneś zobaczyć:
```
Assets/
└── PostProcessing/
    ├── Editor/
    ├── Resources/
    ├── Runtime/
    └── Shaders/  ← TO NAS INTERESUJE!
```

**Czas:** ~5-10 min

## 🔨 KROK 4: Stwórz AssetBundle Builder

### 4.1 Utwórz folder dla skryptów
W Project window:
```
Assets → Create → Folder
Nazwa: "Editor"
```

### 4.2 Stwórz skrypt buildera
```
Assets/Editor → Create → C# Script
Nazwa: "PostProcessingBundleBuilder"
```

### 4.3 Kod skryptu
Kliknij dwukrotnie na `PostProcessingBundleBuilder.cs` i wklej:

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

public class PostProcessingBundleBuilder
{
    [MenuItem("Assets/Build PostProcessing Bundle")]
    static void BuildBundle()
    {
        // Find all PostProcessing shaders
        string[] shaderGuids = AssetDatabase.FindAssets("t:Shader", new[] { "Packages/com.unity.postprocessing/PostProcessing/Shaders" });
        
        if (shaderGuids.Length == 0)
        {
            Debug.LogError("No PostProcessing shaders found!");
            return;
        }

        AssetBundleBuild[] builds = new AssetBundleBuild[1];
        builds[0].assetBundleName = "kerbvision-pp.ssf";
        
        // Collect shader paths
        string[] shaderPaths = new string[shaderGuids.Length];
        for (int i = 0; i < shaderGuids.Length; i++)
        {
            shaderPaths[i] = AssetDatabase.GUIDToAssetPath(shaderGuids[i]);
            Debug.Log($"Adding shader: {shaderPaths[i]}");
        }
        
        builds[0].assetNames = shaderPaths;

        // Create output directory
        string outputPath = Application.dataPath + "/../AssetBundles";
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // Build for Windows (StandaloneWindows64)
        BuildPipeline.BuildAssetBundles(
            outputPath,
            builds,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );

        Debug.Log($"✓ Bundle built successfully!");
        Debug.Log($"Location: {outputPath}/kerbvision-pp.ssf");
        
        // Reveal in Explorer
        EditorUtility.RevealInFinder(outputPath);
    }
}
```

### 4.4 Zapisz i zamknij Visual Studio/Rider

**Czas:** ~5 min

## 🏗️ KROK 5: Zbuduj Bundle

### 5.1 Uruchom builder
W Unity:
```
Menu: Assets → Build PostProcessing Bundle
```

### 5.2 Sprawdź Console
Powinieneś zobaczyć:
```
Adding shader: Packages/.../Uber.shader
Adding shader: Packages/.../Copy.shader
Adding shader: ...
✓ Bundle built successfully!
Location: C:/.../AssetBundles/kerbvision-pp.ssf
```

### 5.3 Okno Explorer się otworzy
Zobaczysz:
```
AssetBundles/
├── kerbvision-pp.ssf         ← TEGO POTRZEBUJEMY!
├── kerbvision-pp.ssf.manifest
└── AssetBundles
```

**Czas:** ~1 min

## 📤 KROK 6: Skopiuj Bundle do KerbVisionIR

### 6.1 Skopiuj plik
PowerShell:
```powershell
$source = "C:\Users\TWOJA_NAZWA\Desktop\KerbVisionIR-Shaders\AssetBundles\kerbvision-pp.ssf"
$dest1 = "C:\Users\grzeg\Desktop\KerbCalcAndNote\TUFXnightVision\TUFX-main\KerbVisionIR\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
$dest2 = "C:\Users\grzeg\Desktop\KerbCalcAndNote\TUFXnightVision\TUFX-main\KerbVisionIR\Release\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
$dest3 = "C:\Program Files\Epic Games\KerbalSpaceProgram\English\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"

Copy-Item $source $dest1 -Force
Copy-Item $source $dest2 -Force
Copy-Item $source $dest3 -Force

Write-Host "✅ Shader bundle skopiowany do wszystkich lokalizacji!" -ForegroundColor Green
```

### 6.2 Weryfikuj rozmiar
```powershell
Get-Item $dest1 | Select Name, Length
```

Oczekiwany rozmiar: **~100-300 KB**

**Czas:** ~1 min

## 🎮 KROK 7: Test w KSP

### 7.1 Zrestartuj KSP
**WAŻNE:** Shader ładuje się tylko przy starcie!

### 7.2 Sprawdź logi
```powershell
Get-Content "C:\Program Files\Epic Games\KerbalSpaceProgram\English\KSP.log" | Select-String "KerbVisionIR.*shader" | Select-Object -Last 10
```

**✅ SUKCES - powinieneś zobaczyć:**
```
[LOG] [KerbVisionIR] Found 2 shaders in bundle:
[LOG] [KerbVisionIR]   Shader: Hidden/PostProcessing/Uber
[LOG] [KerbVisionIR]   Shader: Hidden/PostProcessing/Copy
[LOG] [KerbVisionIR] Uber shader loaded: Hidden/PostProcessing/Uber, Supported=True
[LOG] [KerbVisionIR] Copy shader loaded: Hidden/PostProcessing/Copy, Supported=True
[LOG] [KerbVisionIR] Post-processing initialized (FULL MODE with effects)
```

### 7.3 Test w grze
1. Wejdź w Flight
2. Alt + F8 (GUI)
3. Sprawdź: **"Version 1.0.0 - FULL MODE"**
4. Slidery Vignette/Saturation/Contrast powinny być **AKTYWNE**!

### 7.4 Test efektów
```
✓ Vignette Intensity 0.8 → ciemne rogi
✓ Saturation -1.0 → czarno-biały
✓ Contrast 0.5 → większy kontrast
✓ Grain Intensity 0.5 → widoczne ziarno
```

**Czas:** ~5 min

## ⚠️ Troubleshooting

### Problem: "No PostProcessing shaders found!"
**Rozwiązanie:**
```
1. Sprawdź czy PostProcessing został zainstalowany:
   Package Manager → In Project → Post Processing

2. Sprawdź folder:
   Project → Packages → Post Processing → PostProcessing → Shaders
   
3. Jeśli brak, przeinstaluj package
```

### Problem: Bundle ma 0 KB
**Rozwiązanie:**
```
1. Sprawdź Console w Unity na błędy buildu
2. Upewnij się że używasz Unity 2019.2.x
3. Spróbuj: Edit → Preferences → Asset Pipeline → Clear Cache
4. Zbuduj ponownie
```

### Problem: "Shader unsupported" w KSP
**Rozwiązanie:**
```
To jest OK! Unity może pokazać warning ale shader dalej działa.
Sprawdź czy efekty działają w grze (vignette, saturation).
```

### Problem: Nadal FALLBACK MODE
**Rozwiązanie:**
```
1. Sprawdź czy plik istnieje:
   ls "C:\Program Files\...\KerbVisionIR\Shaders\kerbvision-pp.ssf"

2. Sprawdź rozmiar (powinien być 100-300 KB, nie 517 KB jak TUFX)

3. Sprawdź logi - szukaj linii z nazwami shaderów

4. Jeśli nic nie działa, podeślij mi KSP.log
```

## 📊 Podsumowanie

### Co zrobiłeś:
1. ✅ Zainstalowałeś Unity 2019.2.21f1
2. ✅ Stwórz projekt Unity
3. ✅ Zainstalowałeś PostProcessing Stack V2
4. ✅ Zbudowałeś AssetBundle z shaderami
5. ✅ Skopiowałeś do KerbVisionIR
6. ✅ Przetestowałeś w KSP

### Co teraz działa:
✅ **Wszystkie efekty w FULL MODE!**
- Brightness
- Color Tint
- Skybox
- **Vignette** 🎉
- **Saturation** 🎉
- **Contrast** 🎉
- **Grain** 🎉

### Czas wykonania:
- Unity instalacja: 20-30 min
- Projekt setup: 5-10 min
- Skrypt + build: 5-10 min
- Test: 5 min
- **TOTAL: ~45-60 min** (znacznie mniej niż 4-8h!)

## 🎊 Gratulacje!

Masz teraz **pełną wersję KerbVisionIR** z wszystkimi efektami post-processingu!

Możesz:
- 🎨 Używać wszystkich efektów
- 📦 Dystrybuować pełny mod z shaderami
- 🚀 Publikować na SpaceDock/CKAN
- ✨ Dodawać nowe efekty w przyszłości

**Wesołych lotów nocnych!** 🌙🚀✨

---

## 📚 Dodatkowe Zasoby

### Jeśli chcesz dodać więcej efektów:
- Blur
- Depth of Field  
- Motion Blur
- Chromatic Aberration

Wszystkie są w PostProcessing Stack V2!

### Jeśli chcesz customizować shadery:
```
Packages/PostProcessing/PostProcessing/Shaders/
Edytuj .shader pliki
Zbuduj ponownie bundle
```

### Backup projektu Unity:
```
Folder: KerbVisionIR-Shaders/
Możesz go użyć ponownie do budowania w przyszłości!
```
