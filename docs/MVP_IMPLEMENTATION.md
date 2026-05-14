# MVP Implementation Notes

## Locked Decisions

- Public name: `InClicker - Restaurant`
- Technical repo slug: `inclicker-restaurant`
- Canonical path: `D:\InUniverse\Game\InClicker-Restaurant`
- Engine: Unity 6000.0.74f1, 2D portrait mobile
- First platform: Android
- Cloud: local save first, Supabase later

## Current Vertical Slice

The generated `Main.unity` scene should include:

- top bar with money and bottleneck status,
- Rush Hour energy bar,
- tappable warteg scene,
- three upgrade cards for Dapur, Area Makan, and Kasir,
- offline claim popup shell,
- lightweight VN overlay with three MVP story panels.

## Asset Intake

Ask the user for generated art in sheet form, then crop/import into the final folders:

- `Assets/Art/Characters/chef_male.png`
- `Assets/Art/Characters/chef_female.png`
- `Assets/Art/Characters/grandma_cook.png`
- `Assets/Art/FoodIcons/food_01_nasi_galau.png` through `food_10_nasi_padang_world.png`
- `Assets/Art/Backgrounds/warteg_gang_sempit_vertical.png`
- `Assets/Art/UI/button_primary.png`, `button_upgrade.png`, `button_rush.png`

