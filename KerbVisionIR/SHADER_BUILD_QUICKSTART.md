# 🚀 QUICK START: Budowanie Shaderów (45 min)

Jeśli nie masz czasu czytać całego przewodnika - oto wersja TL;DR!

## ⚡ 5 Kroków

### 1️⃣ Zainstaluj Unity (25 min)
```
1. Pobierz Unity Hub: https://unity.com/download
2. Instaluj Unity 2019.2.21f1 (Archive → 2019.x)
3. Czekaj...
```

### 2️⃣ Nowy Projekt (3 min)
```
Unity Hub → New Project:
- Template: 3D
- Name: KerbVisionIR-Shaders
- Create
```

### 3️⃣ Dodaj PostProcessing (5 min)
```
Window → Package Manager
[+] → Add from git URL
Wklej: com.unity.postprocessing
Add
```

### 4️⃣ Zbuduj Bundle (5 min)
**A) Stwórz skrypt:**
```
Assets → Create → Folder → "Editor"
Editor → Create → C# Script → "PostProcessingBundleBuilder"
```

**B) Wklej kod:**
Otwórz `PostProcessingBundleBuilder.cs` i wklej KOD Z `BUILD_SHADERS_GUIDE.md` (sekcja 4.3)

**C) Build:**
```
Menu: Assets → Build PostProcessing Bundle
```

Okno Explorer otworzy folder z `kerbvision-pp.ssf` (100-300 KB)

### 5️⃣ Skopiuj do KSP (2 min)
**PowerShell:**
```powershell
cd "C:\Users\grzeg\Desktop\...\KerbVisionIR"
.\copy-shader-bundle.ps1
```

LUB ręcznie skopiuj `kerbvision-pp.ssf` do:
- `KerbVisionIR/GameData/KerbVisionIR/Shaders/`
- `C:/Program Files/.../GameData/KerbVisionIR/Shaders/`

## ✅ Test (5 min)

1. **Zrestartuj KSP** (WAŻNE!)
2. Flight → Alt + F8
3. Sprawdź: `"FULL MODE"`
4. Test: Vignette Intensity = 0.8 → ciemne rogi ✨

## 🎉 Done!

Masz pełną wersję KerbVisionIR z wszystkimi efektami!

---

## ⚠️ Problemy?

### "No shaders found"
→ Sprawdź: `Packages/Post Processing/.../ Shaders` istnieje w Project

### Bundle 0 KB
→ Sprawdź Console w Unity na błędy

### Nadal FALLBACK MODE
→ Sprawdź logi KSP:
```powershell
Get-Content KSP.log | Select-String "shader"
```

---

## 📚 Pełny Przewodnik
Zobacz: `BUILD_SHADERS_GUIDE.md` (szczegółowe instrukcje)

## 📋 Checklist
Zobacz: `SHADER_BUILD_CHECKLIST.md` (do wydruku)

## 🔧 Skrypt
Użyj: `copy-shader-bundle.ps1` (automatyczne kopiowanie)
