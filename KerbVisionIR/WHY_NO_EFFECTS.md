# ⚠️ WYJAŚNIENIE: Dlaczego efekty nie działają

## 🔍 DIAGNOZA

### Problem znaleziony:
`tufx-universal.ssf` **NIE ZAWIERA** shaderów PostProcessing Stack!

Logi pokazują:
```
[WRN] Failed to load Uber shader from bundle
[WRN] Failed to load Copy shader from bundle
```

### Co jest w tufx-universal.ssf?
Bundle TUFX zawiera shadery dla **TUFX-specyficznych efektów**:
- Atmospheric scattering
- Ocean rendering  
- Planet atmosphere
- Sky rendering

**NIE zawiera:**
- Hidden/PostProcessing/Uber
- Hidden/PostProcessing/Copy
- Unity PostProcessing Stack v2 shaders

## 🎯 DLACZEGO?

PostProcessing Stack V2 to osobny package Unity:
- Wymaga importu do projektu Unity
- Shadery kompilują się do AssetBundle podczas buildu
- TUFX używa własnych shaderów, nie PostProcessing Stack

KerbVisionIR jest oparty na **Unity PostProcessing Stack V2** który:
- Jest przestarzały (deprecated w Unity 2018+)
- Wymaga własnych shaderów
- Nie jest częścią KSP ani TUFX

## ✅ ROZWIĄZANIA

### Opcja 1: FALLBACK MODE (obecnie działające)
✅ **Brightness** - RenderSettings.ambientLight  
✅ **Color Tint** - ambient color  
✅ **Skybox** - Camera.backgroundColor  
❌ **Vignette, Saturation, Contrast, Grain** - wymaga shaderów

**To jest aktualne działanie - I TO JEST OK!**

### Opcja 2: Własny prosty post-processing
Napisać **własne proste efekty** bez PostProcessing Stack:

**Vignette** - prosty shader HLSL:
```hlsl
fixed4 frag (v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
    float dist = distance(i.uv, float2(0.5, 0.5));
    col.rgb *= 1.0 - (dist * _Intensity);
    return col;
}
```

**Saturation** - prosty algorytm:
```csharp
Color.Lerp(grayscale, original, saturation)
```

**Contrast** - matematyka:
```csharp
color = (color - 0.5) * contrast + 0.5
```

**PROBLEM:** Trzeba to skompilować do AssetBundle w Unity Editor!

### Opcja 3: Użyć TUFX Color Grading
Jeśli użytkownik MA zainstalowany TUFX, możemy użyć jego efektów:
- TUFX ma własny color grading
- TUFX ma vignette
- Ale wymaga całego TUFX jako dependency

## 📊 REKOMENDACJA

**FALLBACK MODE jest wystarczający dla podstawowych funkcji night vision!**

### Co działa (i jest główne):
1. ✅ **Brightness** - najważniejsze dla NV
2. ✅ **Color Tint** - zielony/amber odcień  
3. ✅ **Skybox** - gwiazdy/tło
4. ✅ **Hotkey, GUI, Toolbar**

### Co nie działa (dodatkowe):
- ❌ Vignette - kosmetyczne
- ❌ Saturation/Contrast - kosmetyczne  
- ❌ Grain - kosmetyczne

## 💡 CO DALEJ?

### Opcja A: Zaakceptuj FALLBACK MODE
- Działa od razu, bez dependencies
- Główne funkcje NV działają
- Proste, stabilne
- **Polecam to!**

### Opcja B: Zbuduj własne shadery
1. Otwórz Unity Editor 2019.2 (jak KSP)
2. Stwórz projekt
3. Napisz proste shadery (vignette, saturation, contrast)
4. Zbuduj AssetBundle
5. Dodaj do projektu
6. **~4-8h pracy**

### Opcja C: Dodaj TUFX jako dependency
- Użytkownik musi zainstalować TUFX
- Używamy TUFX color grading zamiast PostProcessing Stack
- Przepisz kod aby używać TUFX API
- **~6-12h pracy + wymaga TUFX**

## 🎯 MOJE ZALECENIE

**Zostań przy FALLBACK MODE!**

Dlaczego:
1. ✅ **Główne funkcje działają** (brightness, tint, skybox)
2. ✅ **Bez dependencies** (nie wymaga TUFX)
3. ✅ **Stabilne** (mniej kodu = mniej bugów)
4. ✅ **Szybkie** (brak post-processing overhead)
5. ⚠️ Vignette/saturation są **nice-to-have**, nie **must-have**

### Alternatywa: Oznacz jako "Lite"
- KerbVisionIR Lite - FALLBACK MODE (bez dependencies)
- KerbVisionIR Full - z TUFX (wymaga TUFX jako dependency)

---

**W tej chwili masz działający mod night vision z podstawowymi funkcjami!** 🎉

Dla pełnych efektów post-processingu potrzeba:
1. Unity Editor
2. PostProcessing Stack V2 package
3. Zbudować własne AssetBundle (~4-8h)

LUB

Przepisać na TUFX API + wymagać TUFX (~6-12h)

**Co wybierasz?** 🤔
