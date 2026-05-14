# Animation Strategy

We will keep animation premium without producing hundreds of hand-drawn frames.

## Character Animation

MVP:

- idle bob: move sprite up/down by 4-8 px,
- blink: optional 2-frame eye overlay later,
- tap reaction: scale punch,
- emotion swap: neutral/happy/worried face variant only for main cast.

Later:

- 3-frame cooking loop for chefs,
- 2-frame grandma stirring loop,
- customer enter/leave tweens.

## Food Animation

- steam particles above hot food,
- small sparkle on upgrade/new dish,
- tap dish squash/stretch on tap,
- rush dish orange rim glow.

## UI Animation

- button press: scale 0.95 then 1.0,
- upgrade success: card pulse + sparkle,
- money gain: floating `+Rp`,
- offline claim: chest scale-in, open swap, coin particles,
- VN: fade in/out and typewriter text.

## Location Animation

MVP location 1:

- lamp glow flicker,
- steam near food display,
- dust mote particles,
- subtle parallax if background is layered.

Weather:

- rain is a particle system,
- night is overlay + lamp glow,
- rush is overlay + speed lines.

## Asset Requirements For Animation

Final art should support simple Unity movement:

- characters need clean full-body transparent PNGs,
- backgrounds should eventually be separated into layers,
- effects should be small transparent PNG sprites,
- face variants are optional and can come after MVP.

