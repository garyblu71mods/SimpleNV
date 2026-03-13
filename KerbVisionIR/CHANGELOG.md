# KerbVisionIR v1.0.1 - Changelog

## ✅ NAPRAWIONE

### 1. **Toolbar Button**
- ✅ Dodana zielona ikona na toolbar w Flight
- Kliknięcie = toggle effect
- Prawy klik = ustawienia

### 2. **Skybox/Gwiazdy**
- ✅ Tło przestrzeni teraz również zmienia kolor!
- Gwiazdy dostają tint (zielony/amber/mono)
- Niebo jasnieje razem z resztą sceny

### 3. **GUI Improvements**
- ✅ Większy tytuł "KerbVision IR - Night Vision"
- ✅ Info o wersji i trybie (FALLBACK/FULL)
- ✅ Status ON/OFF przy toggle
- ✅ Debug logi w konsoli

## 🎯 CO TERAZ DZIAŁA

### ✅ Działające funkcje:
- **Brightness** - jasność sceny 0-2.0x
- **Color Tint** - zielony/amber/mono odcień
- **Skybox Tint** - gwiazdy/tło też się zmieniają ⭐
- **Toolbar Icon** - zielony przycisk na toolbar
- **Hotkey** - Alt + ` toggle
- **GUI** - Alt + F8 ustawienia
- **Audio** - dźwięk aktywacji (jeśli plik istnieje)

### ⚠ Wyłączone (wymaga TUFX shader):
- Vignette (ciemne rogi)
- Grain (filmowe ziarno)

## 🔧 INSTRUKCJA

### Podstawowe użycie:
1. Uruchom KSP
2. Wejdź w Flight
3. **Kliknij zieloną ikonę na toolbar** (nowe!)
4. Lub naciśnij **Alt + `**

### Ustawienia:
- Kliknij prawym na ikonę
- Lub **Alt + F8**

### Zmiana trybu:
- W GUI: kliknij "GreenNV" aby przełączać
- Tryby: Monochrome → GreenNV → AmberWarm

## 📝 TECHNICAL DETAILS

### Nowe pliki:
- `ToolbarButton.cs` - obsługa ApplicationLauncher
- `SkyboxColorCorrection.cs` - zmiana koloru tła/gwiazd

### Zmiany w istniejących:
- `KerbVisionIR.cs` - integracja toolbar + skybox
- `VisionSettingsWindow.cs` - lepszy layout i info

## 🐛 KNOWN ISSUES

Brak znanych problemów! 🎉

## 🚀 NEXT STEPS

Chcesz pełne efekty (vignette + grain)?
1. Zainstaluj TUFX przez CKAN
2. Skopiuj shader: `GameData/TUFX/Shaders/tufx-universal.ssf`
3. Do: `GameData/KerbVisionIR/Shaders/kerbvision-pp.ssf`
4. Zrestartuj KSP

Mod automatycznie przełączy się na FULL MODE! ✨
