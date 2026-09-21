# Ori Nabiji Game

Private Unity mobile-game project based on the MIT-licensed **TA-Casual-Farm** test project by Nikolay Chernyshov.

## Goal

Build an original Georgian supermarket management game with the same high-level gameplay loop that makes games such as *My Mini Mart* engaging:

**collect products → carry visible stacks → stock shelves → serve customers → collect cash → stand on upgrade pads → unlock/automate more of the store**

The project must use original branding, art, layouts, UI, audio, and content. Use of the real **Ori Nabiji / ორი ნაბიჯი** trademark or store identity for a commercial release requires permission from the brand owner.

## Current base

- Unity 2022.1.12f1
- NavMesh customers and queues
- inventory transfer / visible product slots
- shelves and cultivation/production stations
- money account
- purchase spots
- customer spawning and checkout

## Ori Nabiji conversion started

- Purchase spots now drain money progressively while the player stands on them.
- Checkout supports physical cash pickups instead of immediately crediting the player.
- Existing direct-money checkout remains as a fallback until a cash pickup prefab is assigned.

## Next implementation

1. Generalize Vegetable classes into Product classes.
2. Replace farm production with supermarket supply/product stations.
3. Build the first supermarket level and shelf categories.
4. Add worker automation.
5. Add mobile joystick and My Mini Mart-style camera/pacing.
6. Replace all placeholder art with original Ori Nabiji-inspired assets.

See the included MIT license for the upstream code attribution.
