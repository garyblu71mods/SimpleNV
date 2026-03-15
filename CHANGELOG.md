# Changelog

All notable changes to KerbVisionIR are documented in this file.

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