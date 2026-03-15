# CKAN Update Checklist (KerbVisionIR)

## 1. Create GitHub release

- Tag: `v2.0.1`
- Title: `KerbVisionIR 2.0.1`
- Attach ZIP: `KerbVisionIR_v2_0_1.zip`
- ZIP must contain: `GameData/KerbVisionIR/...`

## 2. Keep identifier stable

- CKAN `identifier` must remain: `KerbVisionIR`
- Do not rename identifier for updates

## 3. NetKAN PR

- Fork: `KSP-CKAN/NetKAN`
- Add/update file: `NetKAN/KerbVisionIR.netkan`
- Use content from `CKAN/KerbVisionIR.netkan`
- Open PR with release link

## 4. Validate metadata

- Dependency: `ClickThroughBlocker`
- Dependency: `ToolbarController`
- `ksp_version_min`: `1.12.3`
- install stanza points to: `GameData/KerbVisionIR`

## 5. After merge

- NetKAN bot generates `.ckan`
- Update appears in CKAN client after index refresh
