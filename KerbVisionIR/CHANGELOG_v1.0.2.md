# KerbVisionIR v1.0.2 - Hotfix

## 🐛 NAPRAWIONE BŁĘDY

### 1. Skybox/Gwiazdy nie zmieniały koloru
**Problem:** Mnożenie czarnego tła (0,0,0) przez zielony tint dawało czarny (0,0,0)
**Rozwiązanie:** Dla ciemnego tła używamy bezpośrednio koloru tint, nie mnożenia

```csharp
// PRZED (błędne):
adjustedColor = originalBackgroundColor * tintColor * brightness;
// (0,0,0) * (0.2, 1.0, 0.2) = (0,0,0) - czarny!

// PO (poprawne):
if (background is black) {
    adjustedColor = tintColor * brightness * 0.3f;
    // (0.2, 1.0, 0.2) * 1.5 * 0.3 = zielonkawy!
}
```

### 2. GUI pokazywało nie działające slidery
**Problem:** W FALLBACK mode (bez shadera) vignette/saturation/contrast/grain nie działają, ale slidery były aktywne

**Rozwiązanie:** 
- Oznaczono "(DISABLED)" przy każdym sliderze
- Dodano separator z ostrzeżeniem
- `GUI.enabled = false` dla nie działających kontrolek
- Wyraźna informacja o potrzebie TUFX shadera

## ✅ CO TERAZ DZIAŁA

### FALLBACK MODE (bez TUFX shader):
✅ **Brightness** - jasność sceny 0-2.0x  
✅ **Color Tint** - zielony/amber/mono odcień  
✅ **Skybox Tint** - gwiazdy/tło zmienia kolor! 🌟  
✅ **Toolbar Button** - zielona ikona  
✅ **Hotkey** - Alt + ` toggle  
✅ **GUI** - Alt + F8  

### WYŁĄCZONE (wymaga TUFX):
❌ **Vignette** - ciemne rogi (disabled w GUI)  
❌ **Saturation** - zmiana nasycenia (disabled)  
❌ **Contrast** - kontrast (disabled)  
❌ **Grain** - filmowe ziarno (disabled)  

## 🎯 JAK TO WYGLĄDA W GUI

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━
⚠ Following require TUFX shader:
(Install TUFX for these effects)

Vignette Intensity: 0.40 (DISABLED)
[░░░░░░░░░░] <- slider wyszarzony

Saturation: -0.50 (DISABLED)
[░░░░░░░░░░] <- slider wyszarzony

Contrast: 0.20 (DISABLED)
[░░░░░░░░░░] <- slider wyszarzony

☐ Grain/Noise Effect (DISABLED) <- checkbox wyłączony
```

## 🔧 CO SIĘ ZMIENIŁO W KODZIE

### `SimpleColorCorrection.cs`
- Wykrywa czy tło jest czarne (przestrzeń)
- Dla czarnego tła: używa `tintColor * brightness * 0.3f`
- Dla jasnego tła: mnoży `originalColor * tintColor * brightness`
- Dodane logi z rzeczywistymi wartościami

### `VisionSettingsWindow.cs`
- Dodano `GUI.enabled = !isFallbackMode` dla wyłączonych kontrolek
- Każdy wyłączony slider ma `(DISABLED)` w labelu
- Separator z ostrzeżeniem przed wyłączonymi funkcjami
- Grain checkbox nie może być włączony w FALLBACK mode

## 📊 TESTY

### Test 1: Skybox w przestrzeni
1. Uruchom w Flight (orbit/space)
2. Alt + ` aby włączyć
3. **Oczekiwane:** Gwiazdy/tło mają zielonkawy odcień
4. **Wynik:** ✅ Działa - log pokazuje `RGBA(0.09, 0.45, 0.09, 1.0)` zamiast czarnego

### Test 2: GUI w FALLBACK mode  
1. Alt + F8 (otwórz GUI)
2. **Oczekiwane:** Slidery dla vignette/saturation/contrast są wyszarzone z "(DISABLED)"
3. **Wynik:** ✅ Działa - slidery nie działają, jasne oznaczenie

### Test 3: Zmiana brightness
1. W GUI zmień brightness 0.5 → 2.0
2. **Oczekiwane:** Skybox/gwiazdy jaśnieją
3. **Wynik:** ✅ Działa - kolor zmienia się dynamicznie

## 💡 DLA UŻYTKOWNIKÓW

**Chcesz pełne efekty (vignette, saturation, contrast, grain)?**

1. Zainstaluj TUFX z CKAN:
   ```
   CKAN > Search "TUFX" > Install
   ```

2. Skopiuj shader:
   ```powershell
   Copy-Item "GameData\TUFX\Shaders\tufx-universal.ssf" `
             "GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
   ```

3. Zrestartuj KSP

4. Mod automatycznie wykryje shader i przełączy na **FULL MODE** ✨

## 📝 ZNANE OGRANICZENIA

- Skybox tint działa tylko dla `Camera.backgroundColor` + właściwości shadera skybox
- Niektóre modyfikacje skybox mogą nie wspierać `_Tint` property
- W atmosferze efekt może być mniej widoczny (niebo jest jasne)

## 🚀 NASTĘPNA WERSJA

Planowane:
- [ ] Automatyczna detekcja czy jesteś w przestrzeni vs atmosferze
- [ ] Różne wartości brightness dla przestrzeni/atmosfery
- [ ] Więcej trybów kolorów (IR thermal, UV, etc.)

---

**v1.0.2 - Wszystko naprawione dla FALLBACK mode!** ✅
