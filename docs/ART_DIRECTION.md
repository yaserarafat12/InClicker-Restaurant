# Art Direction - InClicker Restaurant

## Final Visual Choice

Use premium cozy 2D cartoon.

Do not use pixel art for the main game. Pixel art would make production faster in some areas, but it fights the intended mood: drama cozy slice-of-life with warm Indonesian warteg emotion. The target is polished casual mobile game art: readable, rounded, warm, and expressive without becoming expensive frame-by-frame animation.

## Product Feel

- 70% clicker tycoon, 30% story.
- Story supports emotional depth, but the core screen must always feel like a playable restaurant clicker.
- Cozy slice-of-life drama, not parody-only.
- Local Indonesian warmth, but still clean enough for global mobile players.
- Progress should feel stable: numbers rise clearly, upgrades matter, and locations evolve visually.

## Shape Language

- Rounded chunky shapes.
- Subtle dark brown outlines.
- Soft shadows under objects and characters.
- Simple readable silhouettes at phone size.
- Slight chibi proportion, but not childish.
- Faces expressive enough for VN panels.
- Avoid thin linework and tiny details.

## Palette

Core:

- Warm amber light: `#F2A23A`
- Sambal red: `#C64221`
- Warteg green: `#3F7D45`
- Rice cream: `#F6E7C8`
- Dark wood: `#4A2A16`
- Charcoal text: `#271910`

Do not let the whole game become beige/brown only. Use green, red, cream, amber, stainless grey, and occasional blue night accents.

## Lighting

- Warm tungsten lamp as the main light.
- Background corners can be darker, but gameplay objects must stay readable.
- Food icons should look juicy and warm, not realistic oily.
- VN panels can be more cinematic, but still same style.

## Avoid

- Low-res pixel art.
- 3D glossy render.
- Semi-realistic painting that loses readability at 64px.
- Random anime idol styling.
- Flat emoji-like icons.
- Dark restaurant backgrounds where food/characters disappear.
- Text baked into generated images.
- Watermarks.

## Animation Philosophy

We will not generate hundreds of animation frames. We will fake life with Unity transforms, 2-frame swaps, particles, shader/material tweaks, and layered backgrounds.

Use generated art as clean static sprites, then animate in Unity:

- idle bob: transform Y sine motion,
- blink: swap eye overlay or 2-frame face sprite,
- tap feedback: squash/stretch scale punch,
- steam: transparent particle sprites,
- rain: particle system over background,
- lamp flicker: alpha/brightness tween,
- rush hour: orange overlay, shake, speed lines, particles,
- customer movement: short tween between seat/queue points,
- VN emotion: swap mouth/eye/pose only when needed.

