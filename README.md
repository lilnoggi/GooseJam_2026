# Cheating Geese (Working Title)
A dark fantasy, psychological card battler developed for **Goose Jam 2** (August 11 - August 25, 2026). 

The core combat loop relies on manipulating the enemy AI through tense, bluffing-based standoffs using a custom 3D pixel-shader aesthetic.

## Development Setup
* **Engine:** Unity 6.5 (6000.5.7f1) - **DO NOT UPGRADE MID-JAM**
* **Render Pipeline:** URP
* **Task Management:** https://trello.com/b/FS1tSkei/cheating-geese

## Git & Version Control Rules
**The `main` branch is sacred.** It must always contain a playable, stable build. All changes must be made on a separate branch and merged via a Pull Request (PR).

### Branch Naming Conventions
* **Features:** `feature/initials/feature-name` 
* **Art/Audio:** `art/initials/asset-name` 
* **Bug Fixes:** `fix/initials/bug-name`

### Unity Scene Hygiene
**DO NOT** open and edit a `.unity` scene file if someone else is currently working in it. 
* Announce in the Discord when you are opening a scene. 
* Work in Prefabs inside the `_Prefabs` folder whenever possible to avoid scene merge conflicts. 
* **Always ensure `.meta` files are staged and committed alongside your assets.**
