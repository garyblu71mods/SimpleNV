# 🎉 SHADER ZAINSTALOWANY - TEST GUIDE

## ✅ Co zostało zrobione:

Skopiowano `tufx-universal.ssf` (517 KB) do:
1. `KerbVisionIR/GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf`
2. `KerbVisionIR/Release/GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf`
3. `C:\Program Files\Epic Games\KerbalSpaceProgram\English\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf`

## 🚀 JAK PRZETESTOWAĆ FULL MODE

### Krok 1: Zrestartuj KSP
**WAŻNE:** Shader ładuje się tylko przy starcie!

### Krok 2: Sprawdź logi
Uruchom KSP i sprawdź `KSP.log`:

**✅ SUKCES (FULL MODE):**
```
[KerbVisionIR] Awake - Initializing...
[KerbVisionIR] Uber shader loaded: Hidden/PostProcessing/Uber, Supported=True
[KerbVisionIR] Copy shader loaded: Hidden/PostProcessing/Copy, Supported=True
[KerbVisionIR] PostProcessLayer added to camera
[KerbVisionIR] Post-processing initialized (FULL MODE with effects)
[KerbVisionIR] Skybox correction initialized
[KerbVisionIR] Initialization complete
```

**❌ FALLBACK (jeśli coś nie działa):**
```
[KerbVisionIR] TUFX shader not found - running in FALLBACK MODE
```

### Krok 3: Wejdź w Flight i przetestuj

**Test 1: GUI pokazuje FULL MODE**
```
1. Wejdź w Flight
2. Alt + F8 (otwórz GUI)
3. Sprawdź nagłówek: "Version 1.0.0 - FULL MODE"
4. Slidery dla Vignette/Saturation/Contrast powinny być AKTYWNE (nie DISABLED)
```

**Test 2: Vignette (ciemne rogi)**
```
1. Alt + ` (włącz efekt)
2. W GUI: Vignette Intensity = 0.8
3. Sprawdź: Rogi ekranu powinny być ciemniejsze!
```

**Test 3: Saturation (zmiana nasycenia)**
```
1. W GUI: Saturation = -1.0 (minimum)
2. Sprawdź: Obraz powinien być czarno-biały!
3. Ustaw: Saturation = 1.0 (maximum)
4. Sprawdź: Kolory bardzo intensywne
```

**Test 4: Contrast (kontrast)**
```
1. W GUI: Contrast = 0.5
2. Sprawdź: Obraz bardziej kontrastowy
3. Ustaw: Contrast = -0.5
4. Sprawdź: Obraz płaski, mniej kontrastu
```

**Test 5: Grain (ziarno filmowe)**
```
1. W GUI: zaznacz "Grain/Noise Effect"
2. Grain Intensity = 0.5
3. Sprawdź: Widoczne ziarno/szum na obrazie
```

## 📊 Porównanie PRZED i PO

### PRZED (FALLBACK MODE):
```
✅ Brightness - działa
✅ Color Tint - działa
✅ Skybox - działa
❌ Vignette - DISABLED
❌ Saturation - DISABLED
❌ Contrast - DISABLED
❌ Grain - DISABLED
```

### PO (FULL MODE z shaderem):
```
✅ Brightness - działa
✅ Color Tint - działa
✅ Skybox - działa
✅ Vignette - DZIAŁA! 🎉
✅ Saturation - DZIAŁA! 🎉
✅ Contrast - DZIAŁA! 🎉
✅ Grain - DZIAŁA! 🎉
```

## 🐛 Troubleshooting

### Problem: Nadal pokazuje FALLBACK MODE
**Przyczyna:** Shader nie załadował się

**Rozwiązanie:**
1. Sprawdź czy plik istnieje:
```powershell
Test-Path "C:\Program Files\Epic Games\KerbalSpaceProgram\English\GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
```

2. Sprawdź rozmiar (powinien być ~517 KB):
```powershell
Get-Item "...\kerbvision-pp.ssf" | Select Length
```

3. Sprawdź KSP.log na błędy:
```
Get-Content KSP.log | Select-String "KerbVisionIR.*shader"
```

### Problem: "Shader is unsupported"
**To jest OK!** Unity może pokazać warning ale shader dalej działa.

### Problem: Vignette nie widać
**Rozwiązanie:**
- Zwiększ Vignette Intensity do 0.8-1.0
- Sprawdź Vignette Smoothness (ustaw 0.2-0.3)
- Upewnij się że efekt jest włączony (Alt + `)

### Problem: Grain nie widać
**Rozwiązanie:**
- Zwiększ Grain Intensity do 0.7-1.0
- Grain jest subtelny, najlepiej widoczny na ciemnych obszarach

## 🎯 Checklist - Wszystko działa?

- [ ] KSP zrestartowany
- [ ] Log pokazuje "FULL MODE with effects"
- [ ] GUI pokazuje "Version 1.0.0 - FULL MODE"
- [ ] Slidery vignette/saturation/contrast/grain są aktywne
- [ ] Vignette ciemni rogi (przy intensity > 0.5)
- [ ] Saturation zmienia kolory (-1.0 = czarno-biały)
- [ ] Contrast zmienia kontrast
- [ ] Grain dodaje szum (przy intensity > 0.5)
- [ ] Wszystkie efekty działają razem! 🎉

## 🌟 Gratulacje!

Jeśli wszystkie testy przeszły - masz **PEŁNĄ WERSJĘ** KerbVisionIR z wszystkimi efektami!

Teraz możesz:
- ✨ Używać vignette dla efektu tunelowego
- 🎨 Bawiąc się saturation/contrast dla różnych looków
- 🎞️ Dodawać grain dla filmowego efektu
- 🟢 Wszystko w połączeniu z green/amber/mono tint!

**Miłych nocnych lotów!** 🌙✨🚀
