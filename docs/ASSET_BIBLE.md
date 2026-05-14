# Asset Bible - InClicker Restaurant

This is the source of truth for all 2D assets. If an asset is not listed here, do not generate it yet unless Rin adds it.

## Folder Contract

```txt
Assets/_IncomingArt/
  batch_01_style_lock/
  batch_02_core_characters/
  batch_03_food_icons/
  batch_04_location_concepts/
  batch_05_customer_cast/
  batch_06_ui_logo/
  batch_07_vn_panels/
  batch_08_effects_weather/

Assets/Art/
  Characters/
  Customers/
  FoodIcons/
  Locations/
  Backgrounds/
  UI/
  Logos/
  Effects/
  Weather/
  Story/
```

## Phase 1 - Style Lock

Goal: choose one final look before generating many assets.

Required raw sheets:

| File | Purpose |
|---|---|
| `style_lock_v01.png` | first visual direction |
| `style_lock_v02.png` | second visual direction |
| `style_lock_v03.png` | third visual direction |

Decision criteria:

- characters feel local, warm, and readable,
- food icons read at small size,
- UI buttons look mobile-game-ready,
- warteg background works in portrait 9:16,
- style is not too anime, not too realistic, not 3D.

## Phase 2 - Core Characters

Final sprites, transparent PNG, 1024-1536px tall source.

| File | Role | Notes |
|---|---|---|
| `chef_male_idle.png` | main chef / player-facing staff | practical white shirt, dark apron, humble confident face |
| `chef_female_idle.png` | second chef / operations lead | red or green apron, warm confident face |
| `grandma_cook_idle.png` | emotional anchor | batik blouse, apron, memorable silhouette |
| `ojan_manager_idle.png` | future manager | energetic local friend, street-smart |
| `rin_manager_idle.png` | future manager | calm strategist, tidy silhouette |
| `dani_manager_idle.png` | future manager | delivery/operations vibe |

Minimal animation plan:

- use idle bob on the whole sprite,
- blink via optional eye overlay later,
- no walking cycle for MVP.

## Phase 3 - Customer Cast

Final sprites, transparent PNG, 768-1024px tall source. Customers can be half-body or simplified full-body.

| File | Customer Type | Gameplay Flavor |
|---|---|---|
| `customer_office_worker.png` | office worker | lunch rush |
| `customer_student.png` | student | cheap menu fan |
| `customer_driver.png` | ojek/delivery driver | fast eater |
| `customer_ibu_komplek.png` | neighborhood mom | gossip/story line |
| `customer_food_reviewer.png` | reviewer | unlocks reputation beat |
| `customer_tourist.png` | tourist | late-game/global hint |
| `customer_regular_oldman.png` | regular | cozy loyalty |
| `customer_couple.png` | couple | table capacity flavor |

Minimal animation:

- idle bob,
- small head tilt,
- chat bubble on tap,
- simple fade-in/fade-out when entering/leaving.

## Phase 4 - Food Icons

Final icons: 512x512 transparent PNG. Must be readable at 64px.

| File | Menu |
|---|---|
| `food_01_nasi_galau.png` | Nasi Galau |
| `food_02_mie_patah_hati.png` | Mie Patah Hati |
| `food_03_tempe_bucin.png` | Tempe Bucin |
| `food_04_cap_cay.png` | Cap Cay Overthinking |
| `food_05_soto.png` | Soto Eksistensial |
| `food_06_bakso.png` | Bakso QLC |
| `food_07_gado_gado.png` | Gado-Gado Red Flag |
| `food_08_rendang.png` | Rendang Privilege |
| `food_09_sop_buntut.png` | Sop Buntut Nostalgia |
| `food_10_nasi_padang.png` | Nasi Padang World |

Also create:

| File | Use |
|---|---|
| `tap_dish_level_01.png` | main tap dish for location 1 |
| `tap_dish_level_02.png` | upgraded tap dish |
| `tap_dish_rush.png` | rush hour dish highlight |

## Phase 5 - Locations 1-10

Each location needs one 1080x1920 portrait concept first, then final layered background later.

| File | Location | Visual Beat |
|---|---|---|
| `location_01_warteg_gang.png` | Warteg Gang Sempit | tiny warm warteg, glass display, narrow alley |
| `location_02_rumah_naraya.png` | Rumah/Gang Naraya | home-front eatery, neighborhood growth |
| `location_03_warung_rakyat.png` | Warung Rakyat | busier counter, more seats, street signage |
| `location_04_warung_keluarga.png` | Warung Keluarga | family restaurant, cleaner tables, AC hint |
| `location_05_restoran_sederhana.png` | Restoran Sederhana | printed menu, staff uniform, brighter interior |
| `location_06_resto_menengah.png` | Restoran Kelas Menengah | tiled floor, reservation desk, better lighting |
| `location_07_fine_dining.png` | Fine Dining | Indonesian fine dining, elegant but warm |
| `location_08_franchise_nasional.png` | Franchise Nasional | branded outlet, multiple counters |
| `location_09_tokyo_branch.png` | Tokyo Branch | Indonesian food in Tokyo, neon + warm warteg soul |
| `location_10_world_domination.png` | World Domination | flagship restaurant, global celebratory mood |

Layered final target per location:

- `back_wall`
- `lamp_layer`
- `counter_layer`
- `table_layer`
- `foreground_props`
- `weather_overlay` if needed

For MVP, only location 1 needs final layering. Other locations can stay concept art until gameplay reaches them.

## Phase 6 - UI And Logo

| File | Use |
|---|---|
| `logo_inclicker_restaurant.png` | final logo |
| `app_icon.png` | app launcher icon |
| `button_primary.png` | amber CTA |
| `button_upgrade.png` | green upgrade |
| `button_rush.png` | red/orange rush |
| `panel_upgrade.png` | upgrade card frame |
| `panel_story.png` | VN/dialog panel frame |
| `currency_coin.png` | coin/cash icon |
| `energy_icon.png` | rush energy icon |
| `location_badge.png` | location progress badge |
| `locked_icon.png` | locked feature |
| `claim_chest_closed.png` | offline claim closed |
| `claim_chest_open.png` | offline claim open |

UI style:

- 8px-ish rounded cards,
- amber/green/red button roles,
- dark warm outline,
- readable labels,
- no decorative clutter.

## Phase 7 - Effects And Weather

Most effects should be small transparent sprites or Unity particles.

| File | Use |
|---|---|
| `fx_coin_small.png` | coin particle |
| `fx_cash_note.png` | cash particle |
| `fx_tap_ring.png` | tap ripple |
| `fx_speed_line.png` | rush hour |
| `fx_steam_01.png` | food steam |
| `fx_steam_02.png` | food steam variant |
| `fx_sparkle.png` | upgrade success |
| `weather_rain_drop.png` | rain particle |
| `weather_dust_mote.png` | warm air particle |
| `weather_light_glow.png` | lamp glow overlay |

Weather implementation:

- rain: particle system, no need full image sequence,
- night ambience: dark transparent overlay + lamp glow,
- cozy steam: looping alpha/position particles,
- rush hour: orange overlay + speed line particles + small shake.

## Phase 8 - VN Panels

VN can come after core gameplay art. Do not block MVP on 54 panels.

First final panels:

| File | Panel | Mood |
|---|---|---|
| `vn_00_01_opening.png` | opening | Nenek and player start tiny warteg |
| `vn_01_01_first_customer.png` | first customer | first emotional win |
| `vn_01_02_first_growth.png` | growth tease | dream of bigger place |

VN style:

- same character design,
- more cinematic composition,
- 16:9 or 9:16 depending presentation,
- no baked dialogue text,
- leave safe space for dialogue UI.

## Generation Rules

- Generate sheets for exploration, individual transparent PNGs for final import.
- Never mix UI, backgrounds, and characters in a final batch unless it is a style reference sheet.
- Always request no text and no watermark.
- Prefer cream background for sheets, transparent background for final sprites/icons.
- Keep each asset centered and uncropped.
- For sheets: leave large spacing for easy crop.

