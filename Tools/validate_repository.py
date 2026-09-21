from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"

errors = []

for path in ASSETS.rglob("*"):
    if path.name.startswith(".") or path.suffix == ".meta":
        continue
    meta = Path(str(path) + ".meta")
    if not meta.exists():
        errors.append(f"Missing Unity meta file: {meta.relative_to(ROOT)}")

guids = {}
for meta in ASSETS.rglob("*.meta"):
    text = meta.read_text(encoding="utf-8", errors="ignore")
    match = re.search(r"^guid:\s*([0-9a-fA-F]{32})\s*$", text, re.MULTILINE)
    if not match:
        errors.append(f"Missing/invalid guid: {meta.relative_to(ROOT)}")
        continue
    guid = match.group(1).lower()
    if guid in guids:
        errors.append(
            f"Duplicate guid {guid}: {guids[guid]} and {meta.relative_to(ROOT)}"
        )
    guids[guid] = meta.relative_to(ROOT)

build_settings = (ROOT / "ProjectSettings" / "EditorBuildSettings.asset").read_text(encoding="utf-8")
if "enabled: 1" not in build_settings or "Assets/Scenes/SampleScene.unity" not in build_settings:
    errors.append("SampleScene is not enabled in EditorBuildSettings.")

player_settings = (ROOT / "ProjectSettings" / "ProjectSettings.asset").read_text(encoding="utf-8")
if "\\n" in player_settings:
    errors.append("ProjectSettings contains a literal escaped newline; YAML serialization is invalid.")

for required in [
    "productName: Ori Nabiji Game",
    "companyName: Hackth0r",
    "com.hackth0r.orinabiji",
]:
    if required not in player_settings:
        errors.append(f"ProjectSettings missing production value: {required}")

if errors:
    print("\n".join(f"ERROR: {error}" for error in errors))
    sys.exit(1)

print(f"Repository validation passed. Checked {len(guids)} Unity GUIDs.")
