# Asset Tasks For User

Rin already generated placeholder art so the Unity scene can compile and build. These are not final visuals.

Use this document as the manual asset-generation checklist.

## Batch A - Style Lock

Generate 3 variations.

```txt
Create a premium cozy 2D cartoon style reference sheet for a mobile idle clicker tycoon game called InClicker - Restaurant, set in a small Indonesian warteg/family restaurant.

Art direction:
- cozy Indonesian slice-of-life
- clicker tycoon mobile game
- warm tungsten lighting
- soft shadows
- clean rounded shapes
- subtle dark brown outlines
- polished casual mobile game quality
- grounded local characters, not idol anime
- slightly chibi but mature enough for story
- no pixel art
- no 3D render
- no text
- no watermark

Include in one clean reference sheet:
1. young male chef, full body, front-facing idle pose, practical chef outfit with apron
2. young female chef, full body, front-facing idle pose, practical apron, warm confident expression
3. elderly grandmother cook, full body or half body, kind face, batik blouse or local home clothes, apron
4. one signature Indonesian rice dish icon, readable at small size
5. one vertical cozy warteg interior thumbnail, portrait mobile composition, food display case, wooden tables, hanging lamps, cozy night mood
6. three UI button samples: amber main button, green upgrade button, red/orange rush button

Composition:
- clean cream background
- separated elements with enough spacing
- consistent proportions and palette
- mobile game asset sheet, not poster
- readable at phone size
```

Save raw outputs into:

```txt
Assets/_IncomingArt/batch_a_style_lock/
```

## Batch B - Food Icons

After we choose the style from Batch A, generate 2 variations.

```txt
Create a premium cozy 2D cartoon food icon sheet for a mobile idle clicker restaurant game, matching the attached/reference style.

Art direction:
- polished casual mobile game icons
- warm tungsten lighting
- soft shadows
- clean rounded shapes
- subtle dark brown outline
- appetizing but simplified
- readable at 64px
- each food has a clear silhouette
- chunky 2D game icon, not realistic painting
- no text
- no watermark
- no characters
- no background scene

Important layout:
- exactly 10 icons
- arrange in a clean 5 by 2 grid
- each icon centered inside its own invisible square space
- transparent background or plain light cream background
- large spacing between icons so they can be cropped easily
- consistent size and perspective, 3/4 top-down view

Create these exact 10 food icons:
1. Nasi Galau Level 5: rice bowl with red sambal and cucumber
2. Mie Patah Hati: noodle bowl with soft egg and chili oil
3. Tempe Bucin Original: crispy golden tempeh pieces
4. Cap Cay Overthinking: colorful vegetable stir-fry bowl
5. Soto Eksistensial: clear yellow soto soup with lime and herbs
6. Bakso Quarter Life Crisis: giant meatball soup bowl
7. Gado-Gado Red Flag: peanut sauce salad with vegetables and egg
8. Rendang Privilege: premium dark rendang plate with rice
9. Sop Buntut Nostalgia: oxtail soup bowl with carrot and potato
10. Nasi Padang World Edition: mini nasi padang platter with rice, rendang, sambal, greens
```

Save raw outputs into:

```txt
Assets/_IncomingArt/batch_b_food_icons/
```

Final cropped files should become:

```txt
Assets/Art/FoodIcons/food_01_nasi_galau.png
Assets/Art/FoodIcons/food_02_mie_patah_hati.png
Assets/Art/FoodIcons/food_03_tempe_bucin.png
Assets/Art/FoodIcons/food_04_cap_cay_overthinking.png
Assets/Art/FoodIcons/food_05_soto_eksistensial.png
Assets/Art/FoodIcons/food_06_bakso_qlc.png
Assets/Art/FoodIcons/food_07_gado_gado_red_flag.png
Assets/Art/FoodIcons/food_08_rendang_privilege.png
Assets/Art/FoodIcons/food_09_sop_buntut_nostalgia.png
Assets/Art/FoodIcons/food_10_nasi_padang_world.png
```

## Batch C - Gameplay Background

Generate 2 variations.

```txt
Create a vertical 9:16 cozy 2D cartoon warteg interior background for a mobile idle clicker tycoon game.

Scene:
- small Indonesian warteg called InClicker - Restaurant
- warm tungsten hanging lamps
- glass food display counter
- rice cooker, trays, sambal jars, simple wooden tables
- compact interior, readable for portrait mobile gameplay
- clear empty mid/foreground space for characters and tap effects
- top area should leave room for currency UI
- bottom area should leave room for upgrade buttons
- polished casual mobile game background
- clean rounded shapes
- subtle dark brown outlines
- no text
- no watermark
- no characters
- no UI

Output:
- portrait mobile composition
- 1080 x 1920 preferred
- cozy but not dark
```

Save raw outputs into:

```txt
Assets/_IncomingArt/batch_c_background/
```

Final file:

```txt
Assets/Art/Backgrounds/warteg_gang_sempit_vertical.png
```

## Batch D - Characters

After style is locked, generate individual transparent PNGs.

```txt
Create one full-body transparent PNG character sprite for a cozy Indonesian mobile restaurant clicker tycoon game, matching the chosen reference style.

Requirements:
- front-facing idle pose
- clean silhouette
- subtle dark brown outline
- warm soft shading
- transparent background
- no text
- no watermark
- mobile game sprite, readable at phone size
```

Needed files:

```txt
Assets/Art/Characters/chef_male.png
Assets/Art/Characters/chef_female.png
Assets/Art/Characters/grandma_cook.png
```

