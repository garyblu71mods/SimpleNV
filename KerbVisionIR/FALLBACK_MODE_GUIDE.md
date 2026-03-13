# ✅ FALLBACK MODE - GOTOWE DO DZIAŁANIA!

## 🎉 CO SIĘ ZMIENIŁO

Mod TERAZ działa **BEZ** shadera TUFX! 

### ✅ **DZIAŁA OD RAZU:**
- **Brightness** - zwiększa jasność sceny (0-2.0x)
- **Color Tint** - zielony/amber/mono odcień
- **Hotkey** - Alt + ` włącza/wyłącza
- **GUI** - Alt + F8 otwiera ustawienia
- **Audio** - dźwięk aktywacji (jeśli plik istnieje)

### ⚠ **WYŁĄCZONE (wymaga TUFX shader):**
- Vignette (ciemne rogi)
- Grain (ziarno filmowe)
- Post-processing effects

## 🚀 JAK UŻYĆ

### 1. Zainstaluj
```
Rozpakuj do GameData/
```

### 2. Uruchom KSP
- Wejdź w tryb Flight
- Naciśnij **Alt + `** (backtick)

### 3. Sprawdź efekt
- Scena powinna się rozjaśnić
- Pojawi się zielony odcień (Green NV mode)
- Niebo będzie zielonkawe

### 4. Dostosuj ustawienia
- Naciśnij **Alt + F8**
- Zmień Brightness (jasność)
- Zmień Mode (Monochrome/Green/Amber)
- Zapisz i zamknij

## 📋 CO WIDAĆ W GUI

```
⚠ FALLBACK MODE - Lighting Only
Install TUFX shader for full effects
```

To informuje że działasz w trybie awaryjnym.

## 🔧 UPGRADE DO PEŁNEJ WERSJI

Chcesz vignette i grain?

1. Zainstaluj TUFX przez CKAN
2. Skopiuj shader:
```powershell
Copy-Item "GameData\TUFX\Shaders\tufx-universal.ssf" `
          "GameData\KerbVisionIR\Shaders\kerbvision-pp.ssf"
```
3. Zrestartuj KSP
4. Mod automatycznie przełączy się na FULL MODE

## 🎯 TESTY

### Test 1: Podstawowy
1. Flight scene
2. Alt + ` 
3. ✅ Jasność wzrosła?
4. ✅ Zielony odcień?

### Test 2: Zmiana trybu
1. Alt + F8 (settings)
2. Kliknij "GreenNV" aby przełączyć
3. ✅ Mono = biały, Amber = pomarańczowy

### Test 3: Brightness
1. Settings window
2. Przesuń slider Brightness
3. ✅ Scena ciemnieje/jaśnieje?

## 📝 LOGI DO SPRAWDZENIA

W `KSP.log` szukaj:
```
[KerbVisionIR] Awake - Initializing...
[KerbVisionIR] Shader bundle not found - FALLBACK MODE
[KerbVisionIR] Will run in lighting only mode
[KerbVisionIR] Initialization complete
```

Gdy włączysz (Alt + `):
```
[KerbVisionIR] Effect enabled
[KerbVisionIR] Running in FALLBACK MODE - lighting only
```

## ❓ TROUBLESHOOTING

### Nic się nie dzieje przy Alt + `
- Sprawdź czy jesteś w Flight (nie VAB/SPH)
- Sprawdź KSP.log czy mod się załadował
- Spróbuj Alt + F8 (settings window)

### Nie widzę różnicy
- Zwiększ Brightness do 2.0
- Zmień Mode na Amber (bardziej widoczne)
- Sprawdź czy jesteś w dzień/noc (lepiej widać w nocy)

### Efekt jest słaby
To normalne w trybie FALLBACK - tylko lighting!
Dla pełnych efektów dodaj shader TUFX.

## 🎊 GOTOWE!

Mod działa od razu po instalacji, bez zewnętrznych zależności!

**Miłych lotów nocnych!** 🌙✨
