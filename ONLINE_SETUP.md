# iOS / online integration status

Bundle ID: `com.kuroki.shako`.
Existing Unity Cloud project: `dfb5afb5-bb7d-486a-8a1f-cc036125f6db`.

## Implemented locally

- Unity IAP 5.4.3, Multiplayer Services 2.3.3, Netcode for GameObjects 2.13.3, Leaderboards 2.3.4, Cloud Code 2.10.4.
- Fight opens Public matchmaking / Create friend room / Join code / Practice vs BOT.
- Public matching uses MPS Quick Join and Relay, a two-player public session named `shako-duel-v1`, then waits 25 seconds before a BOT fallback. An authentication/service failure is shown as an error with retry or explicit BOT practice, not disguised as a successful search.
- Friend rooms are private, code-based, and wait for a human. There is no friend list/invitation push service yet.
- Host calculates combat, client sends bounded world-space input; both clients receive fighter state, reaction events, equipment, names, rounds and final results. Snapshots 20 Hz, inputs 30 Hz; client render smoothing. No player NetworkObject prefab is required. New NGO manager persists through scene transitions.
- Friend rematch requires both votes. Public result's Next match leaves the session and starts a new search. Disconnect voids the unfinished match; it does not silently substitute a CPU. Online pause cannot stop the opponent.
- Ranking reads top 10 and own rank from UGS. No fabricated production scores. Board IDs: `shako_monthly_rating`, `shako_monthly_streak`.
- Backend/CloudCode/ShakoSubmitResult.js checks authenticated lobby membership, public two-player room, first-to-three scores, agreement of both reports, season, and deduplicates a public session. Initial rating 1000, Elo K=24. CPU/private matches excluded. Mock integration tests cover duplicates/disputes/outsiders/private rooms/concurrent reports.
- Rewarded ads remain optional, 30 shells, twice daily. No new banner or interstitial format. Existing AdMob IDs unchanged.

## Required Unity Dashboard work (not performed)

1. Verify ownership/linkage of the project above. Enable Authentication (anonymous for this prototype), Lobby and Relay for the same environment. Configure budgets as appropriate in the dashboard; no paid service plan was purchased.
2. Create two descending monthly boards. Rating uses Latest score; streak uses Best score. Schedule reset on the first of each month at 00:00 UTC and keep archives.
3. Enable Cloud Save and Cloud Code. Publish `Backend/CloudCode/ShakoSubmitResult.js` as `ShakoSubmitResult` with its declared parameters. Give the Cloud Code service permission to access private Custom Data and write leaderboards. Deny player-direct leaderboard writes; clients only read boards.
4. Test Relay matchmaking with two separate authenticated players on different networks; verify console environment, errors, timeout fallback, cancellation, disconnect, and both votes. Local loopback tests do NOT establish that cloud services are enabled.
5. Before public competitive release: replace host trust with authoritative battle validation/server simulation, test latency/loss on phones, persist/retry result delivery, and load-test Cloud Save ledger initialization/concurrent-player updates and leaderboard projection ordering. The paired-report script prevents unilateral ordinary reports, not collusion or a modified host. It is a beta implementation, not an anti-cheat guarantee.
6. Anonymous identity persists per installation only. Game Center/account linking, account recovery and authenticated cloud inventory migration remain required for production multi-device purchases/progress. The current profile/currency remain local.

## IAP — four consumable shell packs

`Assets/Mantis/Resources/AppleProduct.json` now defines shako_shells_800, shako_shells_2800, shako_shells_6000 and shako_shells_16000 as consumables. See IOS_PRODUCTS.md for the exact registration table. shako_premium is unused. Product contents are settled; paymentsEnabled remains false pending App Store registration and backend verification.

The client store integration handles product retrieval and localized price, purchase pending/failure/deferred states, restoration, transaction deduplication and confirmation only after fulfillment. The shell shop exposes an App Store panel. It does not start purchases while paymentsEnabled is false.

The purchase-verification endpoint `ShakoRedeemApplePurchase` is NOT yet implemented or deployed. Do not enable payments until the four products are registered and the verification/fulfillment backend is ready. Current calls fail closed and leave an unverified order pending instead of granting currency or confirming it.

Required endpoint contract:

- Authenticate the UGS caller; validate Apple-signed transaction using the App Store Server API/library, bundle `com.kuroki.shako`, product, transaction, revocation, environment, ownership and expiry where applicable.
- Bind transaction to an account and persist idempotent entitlement/currency changes before responding. Handle pending retries and restore without duplicate grants, including reinstall/account changes.
- Request: transactionId, productId, receipt, jws. Response: verified, transactionId, productId, shells, premium. Subscription expiry/revocation needs an extended entitlement model; do not configure subscriptions with the current permanent premium boolean.
- Private Apple server keys belong in server secret storage, never Assets or git. App Store Connect tax/banking agreements, product configuration, Sandbox account and TestFlight testing are outside the current local verification.

## Tests / platform limits

- `-networkHostTest` and `-networkClientTest`: two separate Windows processes over localhost UDP port 7789; tests do not call UGS. Isolated progress.
- `-onlineUiTest`: match selection, code input, disabled unconfigured IAP, duplicate/unverified grants, and ten-row leaderboard layout using test-only fixtures.
- `node Backend/CloudCode/test-ranking.cjs`: mocked Cloud Save/Lobby/Leaderboards APIs, not live cloud deployment.
- Windows builds compile. This host lacks Unity iOS Build Support, Xcode, signing and iPhone access. Native StoreKit purchases, consent/ad delivery and iOS Relay behavior are unverified.
- 2026-09-28 local verification passed: two-process connection/state/name/score/rematch, Japanese and English menu layouts, isolated receipt deduplication/rejection, ten ranking rows, and first-to-three presentation regression. Reports and screenshots are under QA/Online and QA/FinishVerified. Ranking fixture screenshots are not live leaderboard evidence.

References: https://docs.unity.com/en-us/mps-sdk/sessions ; https://docs.unity.com/en-us/iap/receipt-validation ; https://docs.unity.com/en-us/leaderboards/access-cloud-code

## Game Center display identity (2026-09-28)
Independent character-name registration and editing have been removed. The iOS GameKit bridge authenticates GKLocalPlayer and reads displayName; unauthenticated users show Guest. Home labels update when the identity changes. Old locally saved names are ignored; equipment and shell balances are retained. Windows verification simulates display-name changes; iOS native compilation/authentication still requires Xcode and a provisioned device.
This change does not migrate the existing UGS matchmaking, Relay transport or leaderboards to Game Center. Those services remain pending migration. The Xcode postprocessor adds GameKit and Game Center capability; App Store Connect and signing configuration are still required.

