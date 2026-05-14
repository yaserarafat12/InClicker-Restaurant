# Restaurant Layout System

This document locks the restaurant camera and asset separation. Do not generate final location assets before following this.

## Final Camera Choice

Use a fixed portrait **2.5D front cutaway diorama**.

This means:

- not top-down,
- not pure flat side view,
- not a wide cinematic background,
- not a full room simulation where every table is independently navigable.

The player sees the restaurant from the front with a slight 3/4 angle. The scene is readable like a cozy stage: back wall, counter, staff, customers, tables, foreground props.

## Why This Is The Best Fit

Top-down would make tables and layout clear, but character emotion becomes weak and every character needs top-down sprites. That is bad for a cozy story clicker.

Pure flat front view keeps characters readable, but tables and depth feel fake.

2.5D front cutaway gives the best balance:

- character sprites can be front-facing and expressive,
- customers can appear in a queue or at tables,
- the food counter is a strong visual anchor,
- tap area stays clear,
- locations 1-10 can evolve visually without rebuilding the whole control scheme,
- VN characters and gameplay characters can share the same art language.

## Portrait Screen Composition

```txt
┌─────────────────────────┐
│ Top UI                  │
│ money, location, rate   │
├─────────────────────────┤
│ Back wall layer         │
│ menu board, lamp, decor │
│                         │
│ Counter / kitchen layer │
│ food display, staff     │
│                         │
│ Customer / table layer  │
│ queue, seats, reactions │
│                         │
│ Tap dish / effect zone  │
├─────────────────────────┤
│ Upgrade / menu / story  │
│ card panel              │
└─────────────────────────┘
```

## Layer Contract Per Location

Each final location should be separated into these files where possible:

```txt
location_XX_back_wall.png
location_XX_lamps_glow.png
location_XX_counter.png
location_XX_staff_anchor_props.png
location_XX_tables.png
location_XX_foreground_props.png
location_XX_weather_overlay.png
```

If GPT Images cannot produce clean layers immediately, generate a full concept first, then regenerate layer-specific images after style approval.

## Anchor Zones

Use consistent anchor zones across all locations:

- Staff zone: behind or beside counter, middle third of scene.
- Customer queue zone: right or left side, never covering the main dish.
- Table zone: lower midground, small enough not to fight the UI.
- Tap dish zone: lower center, above upgrade panel.
- Weather/effects zone: overlay only, never baked into core props unless location-specific.

## Location Evolution

The player should feel the restaurant grows, but the interaction layout stays familiar.

| Location | Shape Rule |
|---|---|
| 1 Warteg Gang Sempit | cramped, warm, tiny counter, narrow alley hint |
| 2 Rumah/Gang Naraya | home-front eatery, more seats, neighborhood feel |
| 3 Warung Rakyat | wider counter, busier queue, street visibility |
| 4 Warung Keluarga | cleaner family restaurant, better tables |
| 5 Restoran Sederhana | printed menu, uniform staff, polished counter |
| 6 Restoran Menengah | reservation hint, modern kitchen, more depth |
| 7 Fine Dining | elegant Indonesian dining, still warm |
| 8 Franchise Nasional | branded outlet, multiple counters |
| 9 Tokyo Branch | Indonesian warmth plus Tokyo street/neon outside |
| 10 World Domination | flagship restaurant, celebratory global scale |

## Asset Implication

Do not draw characters permanently inside final backgrounds.

Characters, customers, tables, tap dish, particles, and UI must stay separate enough to animate and rearrange. Backgrounds may contain static decor, but not important gameplay actors.

## Prompt Rule

Every final restaurant/location prompt must say:

```txt
fixed portrait 2.5D front cutaway diorama, not top-down, not flat side view, clear empty anchor zones for staff, customer queue, tap dish, and bottom UI, no characters, no UI, no text
```

