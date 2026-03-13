# ✅ SHADER BUILDING CHECKLIST

## Przygotowanie (~30 min)

- [ ] Pobrano Unity Hub (https://unity.com/download)
- [ ] Zainstalowano Unity 2019.2.21f1 z Unity Hub
- [ ] Sprawdzono: ~2GB wolnego miejsca

## Unity Projekt (~10 min)

- [ ] Utworzono nowy projekt 3D w Unity Hub
  - Nazwa: `KerbVisionIR-Shaders`
  - Template: 3D Built-in RP
- [ ] Projekt się otworzył poprawnie
- [ ] Widoczny Scene view + Project window

## PostProcessing Package (~10 min)

- [ ] Otwarto Package Manager (Window → Package Manager)
- [ ] Dodano package: `com.unity.postprocessing`
- [ ] Package zainstalował się poprawnie
- [ ] Widoczny folder w Project: `Packages/Post Processing/.../ Shaders`

## Builder Script (~5 min)

- [ ] Utworzono folder `Assets/Editor`
- [ ] Utworzono skrypt `PostProcessingBundleBuilder.cs`
- [ ] Wklejono kod z przewodnika
- [ ] Zapisano plik (Ctrl+S)
- [ ] Unity skompilował skrypt (sprawdź Console - brak błędów)

## Build Bundle (~2 min)

- [ ] Uruchomiono: `Assets → Build PostProcessing Bundle`
- [ ] W Console: `✓ Bundle built successfully!`
- [ ] Otworzył się folder `AssetBundles/`
- [ ] Plik istnieje: `kerbvision-pp.ssf`
- [ ] Rozmiar: 100-300 KB (nie 0 KB!)

## Kopiowanie Bundle (~2 min)

- [ ] Uruchomiono `copy-shader-bundle.ps1` LUB ręcznie skopiowano do:
  - [ ] `KerbVisionIR/GameData/KerbVisionIR/Shaders/`
  - [ ] `KerbVisionIR/Release/GameData/KerbVisionIR/Shaders/`
  - [ ] `C:/Program Files/.../GameData/KerbVisionIR/Shaders/`
- [ ] Sprawdzono rozmiary (wszystkie ~100-300 KB)

## Test w KSP (~5 min)

- [ ] ZRESTARTOWANO KSP (kluczowe!)
- [ ] Wejście w Flight mode
- [ ] Sprawdzono logi:
  ```
  Get-Content KSP.log | Select-String "KerbVisionIR.*shader"
  ```
- [ ] Log zawiera:
  - [ ] `Found X shaders in bundle`
  - [ ] `Shader: Hidden/PostProcessing/Uber`
  - [ ] `Shader: Hidden/PostProcessing/Copy`
  - [ ] `Uber shader loaded: ... Supported=True`
  - [ ] `Post-processing initialized (FULL MODE)`

## Test GUI (~3 min)

- [ ] Naciśnięto Alt + F8
- [ ] GUI pokazuje: `Version 1.0.0 - FULL MODE`
- [ ] Slidery są AKTYWNE (nie DISABLED):
  - [ ] Vignette Intensity
  - [ ] Vignette Smoothness
  - [ ] Saturation
  - [ ] Contrast
  - [ ] Grain

## Test Efektów (~5 min)

### Vignette
- [ ] Ustawiono: Vignette Intensity = 0.8
- [ ] Widoczne: Ciemne rogi ekranu

### Saturation
- [ ] Ustawiono: Saturation = -1.0
- [ ] Widoczne: Obraz czarno-biały
- [ ] Ustawiono: Saturation = 1.0
- [ ] Widoczne: Intensywne kolory

### Contrast
- [ ] Ustawiono: Contrast = 0.5
- [ ] Widoczne: Większy kontrast
- [ ] Ustawiono: Contrast = -0.5
- [ ] Widoczne: Płaski obraz

### Grain
- [ ] Zaznaczono: "Grain/Noise Effect"
- [ ] Ustawiono: Grain Intensity = 0.5
- [ ] Widoczne: Ziarno filmowe na obrazie

## Finalizacja

- [ ] Wszystkie efekty działają poprawnie
- [ ] Zapisano ustawienia (Save & Close)
- [ ] Bundle backup (skopiowano na dysk)
- [ ] Projekt Unity backup (na przyszłość)

## ✨ SUKCES!

- [ ] Masz pełną wersję KerbVisionIR z shaderami!
- [ ] Wszystkie efekty post-processingu działają
- [ ] Mod gotowy do dystrybucji

---

## 📝 Notatki / Problemy:

________________________________

________________________________

________________________________

________________________________

---

## ⏱️ Czas Wykonania:

- Przygotowanie: _____ min
- Unity setup: _____ min
- Build: _____ min
- Test: _____ min
- **TOTAL: _____ min**

## 🎯 Status: 

- [ ] ✅ COMPLETED
- [ ] ⏸️ IN PROGRESS
- [ ] ❌ BLOCKED (opisz problem w notatkach)
