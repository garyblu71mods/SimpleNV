# Changelog

All notable changes to KerbVisionIR are documented in this file.

## 2.0.3 - 2026-04-14

### Changed

- Brightness maximum raised again: `MaxBrightnessMultiplier` 8× → 20× — slider top end is now intentionally over-bright; lower to taste
- Map view now fully suspends night vision and post-processing on entry and auto-resumes with fade-in on exit (defensive state-flag reset added to prevent stale behaviour on repeated map visits)

## 2.0.2 - 2026-03-15

### Changed

- Brightness slider range doubled: `MaxBrightnessMultiplier` raised from 4× to 8× — allows up to 3× more amplification than before
- Removed internal 100-point cap on `ColorGrading.brightness` — full slider range now usable

### Fixed

- Ship no longer appears completely black on the dark side of a moon orbiting the dark side of a planet: NV sensor noise floor (`nvFloor`) now guarantees a minimum ambient signal proportional to brightness setting, even when KSP sets scene ambient light to zero
- Post-processing no longer bleeds into map view: `PostProcessVolume.weight` and `enabled` are now explicitly zeroed on NV disable, eliminating residual color grading in the map camera
- Night vision now auto-resumes with fade-in animation after returning from map view if it was active before entering the map

## 2.0.1 - 2026-03-15

### Changed

- Updated on-screen status text to simple messages: `Night Vision ON` and `Night Vision OFF`
- Tuned grain response curve for smoother slider progression (less dead zone at low values, less saturation in mid range)
- Documentation refresh for packaging and behavior notes

## 2.0.0 - 2026-03-15

### Added

- Stable monochrome-first night vision pipeline
- Green tint applied through `ColorGrading` channel mixer
- In-flight controls for brightness, vignette, grain, scanlines, and tint strength
- Key binding support with optional Alt requirement
- Version/date footer in GUI

### Changed

- GUI cleanup and presentation update
- Map view behavior hardened (effect disabled in map)
- Packaging/layout aligned for `GameData/KerbVisionIR`
- Legacy scattering startup disabled to avoid rendering conflicts

### Fixed

- Green tint disappearing after transition
- Milk/fog look caused by stacked overlays
- Scanline layering over UI elements
- Rapid accidental ON/OFF toggling from repeated key input

## 1.1.1 - 2025-09-18

- Last pre-2.0 baseline used before KerbVisionIR night-vision focused update track.
