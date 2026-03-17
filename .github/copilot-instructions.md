# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction
- Ignore memory-saving suggestions/system memory prompts for now and focus only on code changes.
- Hide the GUI window by default at startup while allowing the mod to activate from the bound key immediately.
- Immediately replace the mod's DLL file in the game folder after changes are made. Use the path `GameData\KerbVisionIR\Plugins\KerbVisionIR.dll` exclusively.
- The GUI should not display the text "Mode: Green (Tint 0.00 = Monochrome)"; version/date should be in a smaller font and moved to the bottom of the window.

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
- Project naming should use **SimpleNV** as the new mod name instead of **KerbVisionIR** in output paths/artifacts. The name **TUFX** must be completely removed from the project, including branding and naming conventions. Additionally, cease using any references to **SimpleNV**.
- NV color modes should be monochrome-first (fully black-and-white base), with green tint applied as the final stable output through the ColorGrading channel mixer; no visible original scene colors should remain, and tint overlay should not be mixed with the B/W pipeline.
- Scanlines should not be drawn on the NavBall/toolbar and should be placed under the vignette, not as an overlay on the UI.