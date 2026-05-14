# InClicker - Restaurant

Unity 2D mobile clicker-tycoon game about growing a tiny Indonesian warteg into a restaurant empire.

## Canonical Local Path

```txt
D:\InUniverse\Game\InClicker-Restaurant
```

## MVP Target

- Android first.
- One polished vertical slice for `Warteg Gang Sempit`.
- Core loop: passive income, three-pillar upgrades, Rush Hour tap energy, offline claim, and a small VN story layer.
- Supabase is deferred until local save/gameplay is stable.

## Working Folders

```txt
Assets/_IncomingArt/        Raw generated sheets before cleanup
Assets/Art/Characters/      Final character sprites
Assets/Art/Customers/       Final customer sprites
Assets/Art/FoodIcons/       Final food icons
Assets/Art/Locations/       Final location backgrounds/layers
Assets/Art/Backgrounds/     Final backgrounds and scene layers
Assets/Art/UI/              Final UI sprites
Assets/Art/Logos/           Logo and app icon
Assets/Art/Effects/         Tap, coin, rush, steam sprites
Assets/Art/Weather/         Rain, dust, glow sprites
Assets/Art/Story/           VN panel images
Assets/Data/VN/             MVP story panel ScriptableObjects
Assets/Scenes/Main.unity    First playable scene
```

## Asset Direction

The final visual direction is premium cozy 2D cartoon, not pixel art.

Production docs:

```txt
docs/ART_DIRECTION.md
docs/ASSET_BIBLE.md
docs/RESTAURANT_LAYOUT.md
docs/ANIMATION_STRATEGY.md
docs/GPT_IMAGE_PROMPTS.md
```

Generate art in this order:

1. Style lock
2. Core characters
3. Customer cast
4. Food icons
5. Location concepts
6. UI and logo
7. Effects/weather
8. VN panels

## Generate MVP Scene

Run this from the project root:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.74f1\Editor\Unity.exe" -batchmode -quit -projectPath "D:\InUniverse\Game\InClicker-Restaurant" -executeMethod InUniverse.InResto.EditorTools.InClickerMvpBuilder.Build -logFile -
```

Then open the project in Unity Hub and press Play on `Assets/Scenes/Main.unity`.
