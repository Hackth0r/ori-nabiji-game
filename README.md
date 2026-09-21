# Ori Nabiji Game

Private Unity mobile-game project built from the MIT-licensed **TA-Casual-Farm** test project by Nikolay Chernyshov.

The target is an original Georgian supermarket-management game using the proven loop:

**collect products → carry visible stacks → stock shelves → customers shop → checkout → collect physical cash → stand on upgrade pads → automate the store**

## Production foundation now included

- Unity project imported with upstream MIT attribution preserved.
- Android/iOS production identifiers and mobile-oriented player settings.
- Main gameplay scene enabled in Build Settings.
- Persistent local save system with atomic file writes and autosave on pause/focus loss/quit.
- Persistent player money, purchase-pad progress/unlocks, and uncollected checkout cash.
- Progressive stand-on purchase pads rather than click-to-buy.
- Physical cash collection at checkout with a runtime fallback pickup when no prefab is assigned.
- Mobile drag-to-move controls plus editor/desktop keyboard and click-to-move.
- Safer customer pooling, shelf selection, inventory transfer, product pooling, and null/configuration handling.
- StockWorker component for physical employee restocking routes.
- ProductSettings layer for supermarket categories while retaining compatibility with the original serialized project.
- SafeAreaFitter for notches and mobile safe areas.
- Production build validator that blocks builds with missing scenes, placeholder identifiers, or Development mode.
- GitHub CI repository validation for Unity .meta/GUID integrity and required production settings.

## Project identity

- Product: **Ori Nabiji Game**
- Android/iOS identifier: `com.hackth0r.orinabiji`
- Android: ARMv7 + ARM64, minimum API 24
- iOS minimum version: 13.0
- Orientation: landscape autorotation only

## Unity

The imported project was authored in Unity **2022.1.12f1**. Open and verify the project in Unity before shipping. For an actual store release, migrate and test on a supported Unity LTS editor rather than changing the serialized editor version blindly in Git.

## Scene setup still required for final content

The code foundation is production-oriented, but the imported scene is still the original prototype content. In Unity, the final content pass must:

1. Replace farm art/layout with original supermarket environment and product assets.
2. Create ProductSettings assets for each SKU/category.
3. Assign cash pickup art if you do not want the built-in runtime green-cash fallback.
4. Configure StockWorker routes and cashier automation unlock events.
5. Put SafeAreaFitter on the root mobile UI RectTransform.
6. Bake the final NavMesh after the store layout is complete.
7. Test economy pacing, retention, device performance, and all unlock/save paths.

## Release-only external requirements

These cannot safely be committed to the repository:

- Android keystore / Google Play signing credentials.
- Apple certificates, provisioning profiles, and App Store Connect credentials.
- Ad/analytics SDK credentials and consent configuration.
- Store listing content and privacy-policy URLs.
- Permission to commercially use the real **Ori Nabiji / ორი ნაბიჯი** trademark, logo, store trade dress, or branded product artwork.

Do not commit signing secrets or production API keys.
