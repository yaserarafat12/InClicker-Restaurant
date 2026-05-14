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
Assets/Art/FoodIcons/       Final food icons
Assets/Art/Backgrounds/     Final backgrounds and scene layers
Assets/Art/UI/              Final UI sprites
Assets/Data/VN/             MVP story panel ScriptableObjects
Assets/Scenes/Main.unity    First playable scene
```

## Generate MVP Scene

Run this from the project root:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.74f1\Editor\Unity.exe" -batchmode -quit -projectPath "D:\InUniverse\Game\InClicker-Restaurant" -executeMethod InUniverse.InResto.EditorTools.InClickerMvpBuilder.Build -logFile -
```

Then open the project in Unity Hub and press Play on `Assets/Scenes/Main.unity`.

