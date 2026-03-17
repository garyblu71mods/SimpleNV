# CKAN Update Checklist (KerbVisionIR)

## 1. Publish release on SpaceDock

- URL: `https://spacedock.info/mod/4105/KerbVision%20IR`
- Upload ZIP: `KerbVisionIR_v2_0_1.zip`
- ZIP must contain: `GameData/KerbVisionIR/...`
- Set version to `2.0.1` on SpaceDock

## 2. Keep identifier stable

- CKAN `identifier` must remain: `KerbVisionIR`
- Do not rename identifier for updates

## 3. NetKAN PR

- Fork: `KSP-CKAN/NetKAN`
- Add/update file: `NetKAN/KerbVisionIR.netkan`
- Use content from `CKAN/KerbVisionIR.netkan` (uses `$kref: spacedock/4105`)
- Open PR — CKAN bot pulls release directly from SpaceDock

## 4. Validate metadata

- Dependency: `ClickThroughBlocker`
- Dependency: `ToolbarController`
- `ksp_version_min`: `1.12.3`
- install stanza points to: `GameData/KerbVisionIR`
- `.version` file URL: `https://raw.githubusercontent.com/garyblu71mods/SimpleNV/restart-clean/GameData/KerbVisionIR/KerbVisionIR.version`

## 5. After merge

- NetKAN bot generates `.ckan` from SpaceDock release
- Update appears in CKAN client after index refresh

